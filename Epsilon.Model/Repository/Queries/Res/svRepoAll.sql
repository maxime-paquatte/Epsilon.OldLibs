-- Version = 4.7.31, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.svRepoAll
(
	
	@_ActorId			int,
	@_CultureId			int
)
as begin

	select
		RepositoryId = r.RepoId,
		RepositoryName = r.Name,
		r.ShowDate,
		r.OrderByValue,
		r.Sortable,
		r.Locked,
		r.Required,
		r.SystemKey,
        DbObjTypes = (
			select rdt.DbObjTypeId
			from Ep.tRepoDbObjType rdt
			where rdt.RepoId = r.RepoId
			FOR JSON PATH, INCLUDE_NULL_VALUES
		)
	from Ep.tRepo r
	where r.RepoId > 0
	order by r.Locked DESC, r.Required DESC, r.Name
	FOR JSON PATH, INCLUDE_NULL_VALUES

end
