-- Version = 6.6.16, Requires={  }

ALTER VIEW Ep.vDbObjRepoValue as
	select drv.DbObjId, rv.RepoValueId, rv.CultureId, ValueName = rv.Name, rv.Value, r.RepoId, RepoSystemKey= r.SystemKey, RepoName = r.Name, DateAdded
	from [Ep].[tDbObjRepoValue] drv 
	inner join [Ep].[vRepoValue] rv on rv.RepoValueId = drv.RepoValueId
	inner join [Ep].[tRepo] r on r.RepoId = rv.RepoId