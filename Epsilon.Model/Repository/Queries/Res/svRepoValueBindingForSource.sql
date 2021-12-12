-- Version = 20.04.23, Requires={  }

ALTER procedure Ep.svRepoValueBindingForSource
(
	
	@_ActorId			int,
	@_CultureId			int,

	@RepoValueSourceId	int 
)
as begin


	
	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select "@json:Array" = 'true', 
			 r.RepoId, RepoName = r.Name, rv.RepoValueId, rv.Name, rv.Value, rv.Path, rvb.*
		from Ep.tRepoValueBinding rvb
		inner join Ep.vRepoValue rv on rv.RepoValueId = IIF(rvb.RepoValueTargetId = @RepoValueSourceId, rvb.RepoValueSourceId, rvb.RepoValueTargetId) AND rv.CultureId = @_CultureId
		inner join Ep.tRepo r on rv.RepoId = r.RepoId

		where rvb.RepoValueSourceId = @RepoValueSourceId OR rvb.RepoValueTargetId = @RepoValueSourceId
	FOR XML PATH('data'), root('data'),  ELEMENTS, TYPE



end
