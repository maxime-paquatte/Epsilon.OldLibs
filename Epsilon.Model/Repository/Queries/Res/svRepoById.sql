-- Version = 4.7.31, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.svRepoById
(
	
	@_ActorId			int,
	@_CultureId			int,

	@RepositoryId		int
)
as begin



	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select
		RepositoryId = r.RepoId,
		RepositoryName = r.Name,
		r.ShowDate,		
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
	where r.RepoId = @RepositoryId
	FOR XML PATH('data'), root('data'),  ELEMENTS, TYPE

end
