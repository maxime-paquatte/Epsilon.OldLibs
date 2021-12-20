-- Version = 20.04.23, Requires={  }

ALTER procedure Ep.svRepoValues
(
	
	@_ActorId			int,
	@_CultureId			int,

	@RepositoryId		int,
	@CultureId int
)
as begin

	select
		rv.RepoValueId,
		rv.RepoId,
		RepositoryValueName = rv.Name,
		rv.Content,
		rv.Value,
		rv.Path,
		rv.SystemKey,
		BindingCount = (select count(*) from Ep.tRepoValueBinding b where b.RepoValueSourceId = rv.RepoValueId OR (b.RepoValueTargetId = rv.RepoValueId AND  b.BothWay = 1))

	from Ep.vRepoValue rv
	inner join Ep.tRepo r on rv.RepoId = r.RepoId
	where r.RepoId = @RepositoryId AND rv.CultureId = @CultureId
	order by rv.Name

	FOR JSON PATH, INCLUDE_NULL_VALUES
end
