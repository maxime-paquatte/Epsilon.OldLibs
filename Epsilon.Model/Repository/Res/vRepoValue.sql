-- Version = 6.6.16,  Requires={  }


ALTER VIEW Ep.vRepoValue as
	select c.CultureId, c.CultureKey,  rv.[RepoValueId]
		,rv.[RepoId]
		,rv.[Name]
		,rv.[SystemKey]
		,rv.[OldPlatformId]
		,rv.[AlphaId] ,
		Value = ISNULL(rvr.Value,  rvrD.Value), 
		Path = ISNULL(rvr.Path, rvrD.Path), 
		Content = ISNULL(rvr.Content,  rvrD.Content)
	from Ep.tCulture c
	inner join Ep.tCulture cd on cd.IsDefault = 1
	cross join Ep.tRepoValue rv
	left outer join Ep.tRepoValueRes rvr
		on rvr.RepoValueId = rv.RepoValueId aND rvr.CultureId = c.CultureId
	left outer join Ep.tRepoValueRes rvrD
		on rvrD.RepoValueId = rv.RepoValueId AND rvrD.CultureId = cd.CultureId
	where rv.RepoValueId > 0 AND c.CultureId > 0