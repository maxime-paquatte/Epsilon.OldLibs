-- Version =*, Requires={  }

ALTER procedure Ep.scStdFolderCreate
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@FolderType			varchar(96),
	@StdFolderName		nvarchar(128),
	@ParentId			int
)
as begin


	SET XACT_ABORT ON;
	BEGIN TRANSACTION;

	DECLARE @StdFolderId int;

	
	insert into [Ep].tStdFolder (ParentId, FolderType, StdFolderName, OwnerId)
	VALUES(@ParentId, @FolderType, @StdFolderName, @_ActorId);
	set @StdFolderId = SCOPE_IDENTITY();
	

	DECLARE @event nvarchar(MAX) = (
	select EventName = 'Ep.Basic.Message.StdFolder.Events.Created', StdFolderId = @StdFolderId
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
	;

	COMMIT TRANSACTION;

end
