-- Version = 6.6.16, Requires={ Ep.sRepoValueComputeSystemKey, Ep.sRepoValueComputePath }

ALTER procedure Ep.seRepoValueChanged
(
	
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepoValueId		int
)
as begin


	declare @RepoId int;
	select @RepoId = RepoId from Ep.tRepoValue where RepoValueId = @RepoValueId

	exec Ep.sRepoValueComputeSystemKey;
	exec Ep.sRepoValueComputePath @RepoId;

	

end
