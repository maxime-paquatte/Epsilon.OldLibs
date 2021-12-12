-- Version = 4.7.31, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.svRepoAll
(
	
	@_ActorId			int,
	@_CultureId			int
)
as begin



	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select
		"@json:Array" = 'true',
		RepositoryId = r.RepoId,
		RepositoryName = r.Name,
		r.ShowDate,
		r.OrderByValue,
		r.Sortable,
		r.Locked,
		r.Required,
		r.SystemKey,
		(
			select "@json:Array" = 'true', rdt.DbObjTypeId
			from Ep.tRepoDbObjType rdt
			where rdt.RepoId = r.RepoId
			FOR XML PATH('DbObjTypes'),  ELEMENTS, TYPE
		)
	from Ep.tRepo r
	where r.RepoId > 0
	order by r.Locked DESC, r.Required DESC, r.Name
	FOR XML PATH('data'), root('data'),  ELEMENTS, TYPE

end
