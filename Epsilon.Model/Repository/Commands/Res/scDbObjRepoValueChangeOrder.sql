-- Version = 4.11.18, Package = Ep.DbObjRepoValueHome, Requires={ Ep.sDbObjRepoValueEnsureIdx  }

ALTER procedure Ep.scDbObjRepoValueChangeOrder
(
	@_Events nvarchar(MAX)  out,
	@_ActorId		int,
	@_CultureId		int,
	@_CommandId		nvarchar(128),

	@DbObjId		 int,
	@SourceValueId	 int,
	@TargetValueId	 int

)
as begin



	exec Ep.sDbObjRepoValueEnsureIdx @DbObjId;


	declare @IdxTarget int;
	select @IdxTarget = Idx from Ep.tDbObjRepoValue where DbObjId = @DbObjId AND RepoValueId = @TargetValueId

	--shift all values from the target
	update Ep.tDbObjRepoValue set Idx = Idx + 1 where DbObjId = @DbObjId AND Idx >= @IdxTarget
	--Insert the source in the space created
	update Ep.tDbObjRepoValue set Idx = @IdxTarget  where DbObjId = @DbObjId AND RepoValueId = @SourceValueId


	
	exec Ep.sDbObjRepoValueEnsureIdx @DbObjId;

	DECLARE @event nvarchar(MAX) = (
	select EventName = 'Ep.Basic.Message.Repo.Events.DbObjValueOrderChanged', DbObjId = @DbObjId
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
	

end
