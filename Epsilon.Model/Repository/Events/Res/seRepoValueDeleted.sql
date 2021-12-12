-- Version = 4.8.1, Package = Ep.RepoValueHome, Requires={ Ep.sRepoValueComputePath, Ep.sRepoValueComputeSystemKey }

ALTER procedure Ep.seRepoValueDeleted
(
	
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepoValueId		int
)
as begin


	exec Ep.sRepoValueComputeSystemKey;

	declare @RepoId int;
	select @RepoId = RepoId from [Ep].[tRepoValue] where RepoValueId = @RepoValueId
	exec Ep.sRepoValueComputePath @RepoId;
	

end
