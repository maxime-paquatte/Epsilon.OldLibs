-- Version = 4.11.18, Package = Ep.DbObjRepoValueHome, Requires={  }

ALTER procedure Ep.scSetDbObjRepoValueDate
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@DbObjId		int = 0,
	@RepoValueId	int = 0,
	@DateAdded		datetime2
)
as begin


	SET XACT_ABORT ON;
	BEGIN TRANSACTION;

	update [Ep].[tDbObjRepoValue] 
	set DateAdded = @DateAdded
	where DbObjId = @DbObjId AND RepoValueId = @RepoValueId


	IF @@ROWCOUNT > 0
	BEGIN
		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.DbObjRepoValueChanged', DbObjId = @DbObjId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		
	END

	COMMIT TRANSACTION;

end
