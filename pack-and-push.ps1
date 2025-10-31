<#
Script: pack-and-push.ps1
Purpose: Build the solution, produce missing .nupkg into ./publish, and push newly created packages to a NuGet feed.
Usage examples:
  powershell -ExecutionPolicy Bypass -File .\pack-and-push.ps1         # normal run
  powershell -ExecutionPolicy Bypass -File .\pack-and-push.ps1 -DryRun # do everything except push

Assumptions / behavior:
 - The script expects to be run at the solution root (where Ep.Libs.sln sits).
 - Feed URL is defined in $FeedUrl variable at top.
 - API key is read from environment variable NUGET_API_KEY.
 - Uses dotnet CLI to build/pack/push (projects are SDK-style .NET). If dotnet is missing, the script will abort.
 - Skips publishing if the package file already exists in ./publish.
 - Uses --skip-duplicate when pushing to avoid errors for already-published versions.
#>

param(
    [switch]$DryRun
)

Set-StrictMode -Version Latest

# Configuration
$FeedUrl = 'https://ci.appveyor.com/nuget/epsilon-xrc19y1yikf9'   # <-- confirmed by user
$ApiKey = $env:NUGET_API_KEY
$SolutionFile = 'Ep.Libs.sln'
$PublishDir = Join-Path -Path (Get-Location) -ChildPath 'publish'

Write-Host "Feed URL: $FeedUrl"
Write-Host "Solution: $SolutionFile"
Write-Host "Publish output: $PublishDir"
Write-Host "DryRun: $($DryRun.IsPresent)"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet CLI not found in PATH. Install .NET SDK or make dotnet available in PATH."
    exit 2
}

if (-not (Test-Path $SolutionFile)) {
    Write-Error "Solution file '$SolutionFile' not found in current directory: $(Get-Location)"
    exit 3
}

if (-not (Test-Path $PublishDir)) {
    New-Item -ItemType Directory -Path $PublishDir | Out-Null
}

# Build solution in Release
Write-Host "Building solution..."
$buildArgs = "build `"$SolutionFile`" -c Release"
$buildResult = & dotnet build $SolutionFile -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet build failed. Aborting."
    exit $LASTEXITCODE
}
Write-Host "Build succeeded."

# Discover csproj files
$projFiles = Get-ChildItem -Path . -Filter *.csproj -Recurse | Select-Object -ExpandProperty FullName
if ($projFiles.Count -eq 0) {
    Write-Error "No .csproj files found beneath current directory."
    exit 4
}

$createdPackages = @()
$skippedPackages = @()
$failedPackages = @()

foreach ($proj in $projFiles) {
    # Parse minimal XML to get PackageId and Version and whether packable
    try {
        [xml]$xml = Get-Content $proj -Raw
    } catch {
        Write-Warning "Failed to read project XML: $proj. Skipping."
        continue
    }

    # Resolve PackageId
    $packageId = $null
    $version = $null
    $isPackable = $true

    # Search PropertyGroup entries for PackageId/Version/IsPackable using XPath to avoid property-not-found errors
    $propertyGroups = $xml.Project.SelectNodes('PropertyGroup')
    foreach ($pg in $propertyGroups) {
        $node = $pg.SelectSingleNode('PackageId')
        if ($node -and $node.InnerText.Trim()) { $packageId = $node.InnerText.Trim() }
        $node = $pg.SelectSingleNode('Version')
        if ($node -and $node.InnerText.Trim()) { $version = $node.InnerText.Trim() }
        $node = $pg.SelectSingleNode('IsPackable')
        if ($node -and $node.InnerText.Trim()) {
            try {
                $isPackable = [System.Convert]::ToBoolean($node.InnerText.Trim())
            } catch {
                # If parsing fails, assume true
                $isPackable = $true
            }
        }
    }

    # Fallback: use AssemblyName or project filename
    if (-not $packageId) {
        $asmNode = $xml.Project.SelectSingleNode('PropertyGroup/AssemblyName')
        if ($asmNode -and $asmNode.InnerText.Trim()) {
            $packageId = $asmNode.InnerText.Trim()
        } else {
            $packageId = [System.IO.Path]::GetFileNameWithoutExtension($proj)
        }
    }
    if (-not $version) {
        # Try to read Version from Directory.Build.props or fallback to 1.0.0
        $rootDir = (Get-Location).ProviderPath
        $dirBuild = Join-Path -Path $rootDir -ChildPath 'Directory.Build.props'
        if (Test-Path $dirBuild) {
            try {
                [xml]$dbp = Get-Content $dirBuild -Raw
                $vNode = $dbp.Project.SelectSingleNode('PropertyGroup/Version')
                if ($vNode -and $vNode.InnerText.Trim()) { $version = $vNode.InnerText.Trim() }
            } catch {
                # ignore parse errors
            }
        }
        if (-not $version) { $version = '1.0.0' }
    }

    if (-not $isPackable) {
        Write-Host "Project $proj marked IsPackable=false. Skipping packaging."
        continue
    }

    # Friendly name for logs
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($proj)

    $expectedPkg = Join-Path $PublishDir "$($packageId).$($version).nupkg"
    if (Test-Path $expectedPkg) {
        Write-Host "Package already exists in publish: $([System.IO.Path]::GetFileName($expectedPkg)) - skipping"
        $skippedPackages += $expectedPkg
        continue
    }

    # Pack the project into publish dir
    Write-Host "Packing $projectName ($proj) -> $expectedPkg"
    # Use dotnet pack --no-build (we already built) and output to publish dir. If project defines Version, dotnet pack will use it.
    $packCmd = @('pack', $proj, '-c', 'Release', '--no-build', '-o', "$PublishDir")
    $packResult = & dotnet @packCmd
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "dotnet pack failed for $proj"
        $failedPackages += $proj
        continue
    }

    # Find produced package (dotnet pack may use package id/version). Search publish dir for new file matching pattern
    $pattern = "$($packageId).$($version).nupkg"
    $found = Get-ChildItem -Path $PublishDir -Filter "$($packageId).$($version).nupkg" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $createdPackages += $found.FullName
        Write-Host "Created package: $($found.Name)"
    } else {
        # If not found, try to find any nupkg produced recently (within last 2 minutes) for this project folder
        $projDir = Split-Path $proj -Parent
        $recent = Get-ChildItem -Path $PublishDir -Filter *.nupkg | Where-Object { $_.LastWriteTime -gt (Get-Date).AddMinutes(-5) } | Sort-Object LastWriteTime -Descending
        if ($recent) {
            $createdPackages += $recent[0].FullName
            Write-Host "Created package (fallback): $($recent[0].Name)"
        } else {
            Write-Warning "No nupkg found for $proj after pack."
            $failedPackages += $proj
        }
    }
}

# Push only packages that were created during this run. If a created package fails to push, remove it from publish so it will be regenerated next run.
if ($createdPackages.Count -eq 0) {
    Write-Host "No packages were created during this run. Nothing to push."
} else {
    if (-not $ApiKey) {
        Write-Warning "Environment variable NUGET_API_KEY is not set. Created packages will remain in $PublishDir for manual push or next run after setting the key."
    }

    foreach ($pkg in $createdPackages) {
        Write-Host "Processing created package for push: $pkg"
        if ($DryRun) {
            Write-Host "DryRun: would push $pkg to $FeedUrl with --skip-duplicate"
            continue
        }
        if (-not $ApiKey) {
            Write-Warning "Skipping push for $pkg because NUGET_API_KEY is missing."
            continue
        }

        # Attempt push; on failure remove local package so it will be re-created next run
        & dotnet nuget push $pkg --api-key $ApiKey --source $FeedUrl --skip-duplicate
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Push failed for $pkg - removing local package so it will be regenerated next run."
            try {
                Remove-Item -LiteralPath $pkg -Force -ErrorAction Stop
                Write-Host "Removed local package: $pkg"
            } catch {
                Write-Warning ("Failed to remove local package {0}: {1}" -f $pkg, $_)
            }
            $failedPackages += $pkg
        } else {
            Write-Host "Pushed: $pkg"
        }
    }
}

# Summary
Write-Host "\nSummary:"
Write-Host "  Created: $($createdPackages.Count)"
Write-Host "  Skipped (already in publish): $($skippedPackages.Count)"
Write-Host "  Failures: $($failedPackages.Count)"

if ($failedPackages.Count -gt 0) {
    Write-Host "Failed items:"; $failedPackages | ForEach-Object { Write-Host "  $_" }
    exit 5
}

Write-Host "Done."
