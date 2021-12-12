using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Epsilon.DbUpdate
{
    public class Updater
    {
        private const int TransactionNameMaxLength = 32;
        private const string AssemblyFilter = "*Epsilon*.dll|*Ep.*.dll";

        private readonly string _connectionString;
        private readonly string _assembliesPath;
        private readonly Action<string> _log;

        private readonly Dictionary<string, string> _versions = new Dictionary<string, string>();
        private readonly HashSet<string> _existing = new HashSet<string>();

        Dictionary<string, SqlResource> _resources;
        private readonly HashSet<string> _changedProcedures = new HashSet<string>();
        private readonly Regex _findSpCall = new Regex(@"exec\s+(?<name>\w*\.s\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _findViewCall = new Regex(@"(?:from|join)\s+(?<name>\w*\.v\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex _findFuncCall = new Regex(@"(?<name>\w*\.f\w*)\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public Updater(string connectionString, string assembliesPath, Action<string> log)
        {
            _connectionString = connectionString;
            _assembliesPath = assembliesPath;
            _log = log;
        }

        public void Update()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                EnsureDbVersionTable(connection);
                FetchAllVersions(connection);
            }

            var version = long.Parse(_versions["Model"]);
            _log("Current Version: " + version);

            var files = FindSqlFiles().ToArray();
            var migrations = FindNewerMigrationScripts(files, version).ToList();
   
            _log("Migrations found");
            foreach (var migration in migrations.OrderBy(p=> p.Time).ThenBy(p=>p.Item))
                _log($"\t{migration.Item}\t{migration.Time}\t{migration.Name}");


            _log("Find Resources");
            _resources = FindResources(files).ToDictionary(p=>p.SchemaAndName);


            _log("APPLY MIGRATIONS");
            foreach (var migration in migrations.OrderBy(p => p.Time))
                ApplyMigration(migration);

            //update sp, functions, views
            _log("UPDATE RESOURCES");
            foreach (var res in _resources.Values)
                UpdateOrCreateRes(res, false);

            //remove missing res
            _log("DELETE RESOURCES");
            foreach (var remainingSpVersion in _existing.Where(p => p != "Model"))
                DeleteSp(remainingSpVersion);

        }

        private void DeleteSp(string spVersion)
        {
            _log(spVersion);
            var parts = spVersion.Split('.');
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var transaction = connection.BeginTransaction();
            try
            {
                var cmd = new SqlCommand($"DROP Procedure [{parts[^2]}].[{parts[^1]}]", connection, transaction);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand($"delete from [Ep].[tDbVersion] where Name = '{spVersion}'", connection, transaction);
                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                _log($"\tMessage: {ex.Message}");

                // Attempt to roll back the transaction.
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred
                    // on the server that would cause the rollback to fail, such as
                    // a closed connection.
                    _log($"\tMessage: {ex2.Message}");
                }
                throw;
            }
        }

        private void ApplyMigration(Migration migration)
        {
            _log(migration.Time + "\t" + migration.Item + "\t" + migration.Name);
            try
            {
                PlayScript("Model", migration.Content, migration.Time.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to play migration {migration.Name} for {migration.Item}", ex);
            }
        }

        private void UpdateOrCreateRes(SqlResource res, bool force, string chain = "")
        {
            var chainParts = chain.Split('|');
            if(chainParts.Contains(res.SchemaAndName))
                return;

            //Force OR never installed OR changed
            if (!_versions.TryGetValue(res.Fullname, out var lastVersion) || lastVersion != res.Sha1 || force)
            {
                //Not already installed (other resource reference)
                if (!_changedProcedures.Contains(res.Fullname))
                {
                    _log(new string('\t', chainParts.Length) + res.Fullname);

                    if (chainParts.Length > 20) throw new InvalidOperationException("Maximum depth exceeded");
                    //Update Dependencies first
                    foreach (var depName in res.Dependencies)
                    {
                        if (_resources.TryGetValue(depName, out var dep))
                            UpdateOrCreateRes(dep, true, chain + "|" + res.SchemaAndName);
                        else throw new Exception($"Missing dependency {depName} for resource {res.Fullname}");
                    }
                    
                    //Replace only first occurrence of alter or create
                    var content = res.File.GetContent();
                    if (string.IsNullOrEmpty(lastVersion))
                    {
                        var regex = new Regex(@"ALTER\s*(?<name>(?:procedure|view|function))", RegexOptions.IgnoreCase);
                        content = regex.Replace(content, "CREATE ${name}", 1);
                    }
                    else
                    {
                        var regex = new Regex(@"CREATE\s*(?<name>(?:procedure|view|function))", RegexOptions.IgnoreCase);
                        content = regex.Replace(content, "ALTER ${name}", 1);
                    }
                    

                    PlayScript(res.Fullname, content, res.Sha1);
                    _changedProcedures.Add(res.Fullname);

                    
                }
            }
            _existing.Remove(res.Fullname);
        }

        private void PlayScript(string name, string content, string version)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var transaction = connection.BeginTransaction(name.Length > TransactionNameMaxLength ? name.Substring(0, TransactionNameMaxLength) : name);

            try
            {
                foreach (var subContent in Regex.Split(content, @"[\r|\n][g|G][o|O]\s*[\r|\n]", RegexOptions.IgnoreCase))
                {
                    var c = subContent.Trim();
                    if (!string.IsNullOrEmpty(c))
                    {
                        var cmd = new SqlCommand(c, connection, transaction);
                        cmd.CommandType = CommandType.Text;
                        
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message + Environment.NewLine + subContent, ex);
                        }
                    }
                }

                var updateVersionQuery = @"
UPDATE Ep.tDbVersion set Ver = @Version WHERE Name = @Name
IF @@ROWCOUNT = 0  INSERT INTO Ep.tDbVersion (Name, Ver) VALUES(@Name, @Version)
";
                var cmd2 = new SqlCommand(updateVersionQuery, connection, transaction);
                cmd2.Parameters.AddWithValue("@Name", name);
                cmd2.Parameters.AddWithValue("@Version", version);
                cmd2.ExecuteNonQuery();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                _log($"\tMessage: {ex.Message}");

                // Attempt to roll back the transaction.
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred
                    // on the server that would cause the rollback to fail, such as
                    // a closed connection.
                    _log($"\tMessage: {ex2.Message}");
                }
                throw new Exception("Error while playing script " + name, ex);
            }
            
        }
        
        private void FetchAllVersions(SqlConnection connection)
        {
            _versions.Clear();
            _existing.Clear();

            var cmd = new SqlCommand($"select Name, Ver from Ep.tDbVersion", connection);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                _versions.Add(reader.GetString(0), reader.GetString(1));
                _existing.Add(reader.GetString(0));
            }
        }

        void EnsureDbVersionTable(SqlConnection connection)
        {
            var query = @"
IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Ep].[tDbVersion]') AND type in (N'U'))
BEGIN
	IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'Ep')
	  EXEC('CREATE SCHEMA [Ep] AUTHORIZATION [db_owner]')

	CREATE TABLE [Ep].[tDbVersion](Name varchar(256) not null, Ver varchar(256) not null, constraint PK_tDbVersion primary key clustered ( Name ) )
	insert into Ep.tDbVersion(Name, Ver)Values('Model', 0);
	
END
	";

            var cmd = new SqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        private IEnumerable<Migration> FindNewerMigrationScripts(SqlFile[] resources, long version)
        {
            var regex = new Regex(@"(?<Item>(?:\w|\.)*)\.Migrations\.(?<Time>\d*)_(?<Name>\w*)\.sql", RegexOptions.Compiled);
            foreach (var resource in resources)
            {
                var r = regex.Match(resource.ResName);
                if (r.Success && long.TryParse(r.Groups["Time"].Value, out var time) && (version == 0 || time > version))
                {
                    yield return new Migration
                    {
                        Item = r.Groups["Item"].Value,
                        Name = r.Groups["Name"].Value,
                        Time = time,
                        Content = resource.GetContent()
                    };
                }
            }
        }

        private IEnumerable<SqlResource> FindResources(SqlFile[] files)
        {
            var schemaRegex = new Regex(@"(?:alter|create)\s*(?:function|procedure|view)\s*(?<ns>\w*)\.(?<name>\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var resNameRegex = new Regex(@"(?<Item>(?:\w|\.)*)\.(?:Commands|Event|Queries|Res)(?:\.Res)?\.(?<Name>\w*)\.sql", RegexOptions.Compiled);
            foreach (var file in files)
            {
                var r = resNameRegex.Match(file.ResName);
                if (r.Success)
                {
                    var firstLine = file.GetFirstLine();
                    var config = firstLine.StartsWith("--{")
                        ? JsonSerializer.Deserialize<ResourceConfig>(firstLine.Trim('-'))
                        : ResourceConfig.Empty;

                    var content = file.GetContent();
                    var schemaMatch = schemaRegex.Match(content);

                    var res = new SqlResource
                    {
                        File = file,
                        Item = r.Groups["Item"].Value,
                        Name = r.Groups["Name"].Value,
                        Schema = schemaMatch.Groups["ns"].Value,
                        Sha1 = Hash(content)
                    };

                    _log(res.Fullname);
                    if (config != ResourceConfig.Empty)
                        _log("\t" + config);

                    foreach (var name in _findSpCall.Matches(content)
                            .Union(_findViewCall.Matches(content))
                            .Union(_findFuncCall.Matches(content))
                            .Where(m => m.Success)
                            .Select(match => match.Groups["name"].Value)
                            .Distinct())
                    {
                        if(name != res.SchemaAndName && !config.DoNotTrack.Contains(name))
                            if(res.Dependencies.Add(name))
                                _log("\t-->" + name);
                    }
                    yield return res;
                }
            }
        }


        private IEnumerable<SqlFile> FindSqlFiles()
        {
            _log("Assemblies");
            var files = AssemblyFilter.Split('|').SelectMany(f => Directory.EnumerateFiles(_assembliesPath, f, SearchOption.AllDirectories));
            foreach (var file in files)
            {
                var assembly = Assembly.LoadFile(file);
                _log("\t" + assembly.FullName);
                foreach (var res in assembly.GetManifestResourceNames()
                    .Where(p => p.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return new SqlFile { Assembly = assembly, ResName = res };
                }
            }
        }

        static string Hash(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }
    }

    class SqlResource
    {
        public SqlFile File { get; set; }

        public string Name { get; set; }
        public string Schema { get; set; }
        public string Item { get; set; }
        public string Sha1 { get; set; }

        public string SchemaAndName => Schema + "." + Name;
        public string Fullname => Item + "." + Schema + "." + Name;
        public string FullnameOld => Item + "." + Name;

        public HashSet<string> Dependencies { get; set; }

        public SqlResource()
        {
            Dependencies = new HashSet<string>();
        }
    }

    class SqlFile
    {
        public Assembly Assembly { get; set; }
        public string ResName { get; set; }

        public string GetContent()
        {
            using Stream stream = Assembly.GetManifestResourceStream(ResName);
            Debug.Assert(stream != null, nameof(stream) + " != null");

            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public string GetFirstLine()
        {
            using Stream stream = Assembly.GetManifestResourceStream(ResName);
            Debug.Assert(stream != null, nameof(stream) + " != null");

            using StreamReader reader = new StreamReader(stream);
            return reader.ReadLine();
        }
    }

    class ResourceConfig
    {
        public string[] DoNotTrack { get; set; }

        public static ResourceConfig Empty { get; } = new ResourceConfig { DoNotTrack = Array.Empty<string>() };

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
