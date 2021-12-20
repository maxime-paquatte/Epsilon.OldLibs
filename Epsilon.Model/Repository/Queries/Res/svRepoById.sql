-- Version = 4.7.31, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.svRepoById
(
	
	@_ActorId			int,
	@_CultureId			int,

	@RepositoryId		int
)
as begin



	select
		RepositoryId = r.RepoId,
		RepositoryName = r.Name,
		r.ShowDate,		
		r.Sortable,
		r.Locked,
		r.Required,
		r.SystemKey,
		(
			select rdt.DbObjTypeId
			from Ep.tRepoDbObjType rdt
			where rdt.RepoId = r.RepoId
			FOR JSON PATH, INCLUDE_NULL_VALUES
		)
	from Ep.tRepo r
	where r.RepoId = @RepositoryId
	FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER

end
