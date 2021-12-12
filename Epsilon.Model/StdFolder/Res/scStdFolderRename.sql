-- Version =*, Requires={  }

ALTER procedure Ep.scStdFolderRename
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),
	
	@StdFolderId		int,
	@StdFolderName		nvarchar(128)
)
as begin


	SET XACT_ABORT ON;
	BEGIN TRANSACTION;

	IF NOT EXISTS (select 1 from Ep.tStdFolder where StdFolderId = @StdFolderId AND OwnerId = @_ActorId)
		raiserror('Only owner can rename the folder', 18,1);
	
	
	update Ep.tStdFolder set StdFolderName = @StdFolderName where StdFolderId = @StdFolderId

	DECLARE @event nvarchar(MAX) = (
	select EventName = 'Ep.Basic.Message.StdFolder.Events.Changed', StdFolderId = @StdFolderId
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
	;

	COMMIT TRANSACTION;

end
