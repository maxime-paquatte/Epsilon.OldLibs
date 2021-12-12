-- Version = 6.6.16, Package = Ep.RepoValueHome, Requires={ Neva.fCharCount }

ALTER VIEW Ep.vRepoValuePath as
	WITH repoValues (RepoId, RepoValueId, CultureId, root, path )
	AS
	(
		select v.RepoId, v.RepoValueId, v.CultureId, v.Name , CAST(v.Value as nvarchar(MAX))
		from [Ep].vRepoValue v
		where RepoValueId > 0 AND Ep.[fCharCount](Name, '.') = 0

		UNION ALL

		select v.RepoId, v.RepoValueId, v.CultureId, v.Name , CAST(p.path + ' > ' + v.Value as nvarchar(MAX))
		from [Ep].vRepoValue v
		inner join repoValues p
			on v.RepoId = p.RepoId AND v.Name like p.root + '.%'
		where Ep.fCharCount(v.Name, '.') = Ep.[fCharCount](p.root, '.') + 1 AND v.CultureId = p.CultureId
	)
	-- Statement using the CTE
	SELECT *
	FROM repoValues
