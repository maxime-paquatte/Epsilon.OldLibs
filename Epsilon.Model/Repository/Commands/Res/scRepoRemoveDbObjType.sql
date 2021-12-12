-- Version = 4.11.18, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.scRepoRemoveDbObjType
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepositoryId		int = 0,
	@DbObjTypeId		int = 0
)
as begin


	SET XACT_ABORT ON;
	BEGIN TRANSACTION;

	delete from [Ep].[tRepoDbObjType] 	where [RepoId] = @RepositoryId AND [DbObjTypeId] = @DbObjTypeId


	IF @@ROWCOUNT > 0
	BEGIN
		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.DbObjTypeChanged', RepositoryId = @RepositoryId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		
	END

	COMMIT TRANSACTION;

end
