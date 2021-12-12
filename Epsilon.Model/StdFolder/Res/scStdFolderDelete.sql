-- Version =*, Requires={  }

ALTER procedure Ep.scStdFolderDelete
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),
	
	@StdFolderId			int
)
as begin


	SET XACT_ABORT ON;
	BEGIN TRANSACTION;

	IF NOT EXISTS (select 1 from Ep.tStdFolder where StdFolderId = @StdFolderId AND OwnerId = @_ActorId)
		raiserror('Only owner can delete the folder', 18,1);
	
	declare @parentId int;
	select @parentId = ParentId from Ep.tStdFolder where StdFolderId = @StdFolderId;



	update Emat.tImportTemplate set StdFolderId = @parentId where StdFolderId = @StdFolderId
	update Ep.tStdFolder set ParentId = @parentId where ParentId = @StdFolderId

	delete from Ep.tStdFolder where StdFolderId = @StdFolderId

	DECLARE @event nvarchar(MAX) = (
	select EventName = 'Ep.Basic.Message.StdFolder.Events.Deleted', StdFolderId = @StdFolderId
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
	;

	COMMIT TRANSACTION;

end
