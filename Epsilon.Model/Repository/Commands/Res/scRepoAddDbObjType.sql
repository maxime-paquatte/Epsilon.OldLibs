-- Version = 4.11.18, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.scRepoAddDbObjType
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

	IF NOt EXISTS (select 1 from [Ep].[tRepoDbObjType] 	where [RepoId] = @RepositoryId AND [DbObjTypeId] = @DbObjTypeId)
	BEGIN

		insert into [Ep].[tRepoDbObjType] ([RepoId], [DbObjTypeId])
		VALUES(@RepositoryId, @DbObjTypeId)



		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.DbObjTypeChanged', RepositoryId = @RepositoryId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		
	END

	COMMIT TRANSACTION;

end
