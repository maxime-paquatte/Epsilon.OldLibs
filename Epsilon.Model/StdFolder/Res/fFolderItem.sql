-- Version =*, Requires={  }

ALTER FUNCTION Ep.fFolderItem(@FolderType nvarchar(96), @FolderId int)
RETURNS  nvarchar(MAX) 
WITH RETURNS NULL ON NULL INPUT 
AS BEGIN

	DECLARE @RetVal nvarchar(MAX);	
  
	SELECT @RetVal = (
		SELECT *,Items =
			CASE WHEN ParentId=@FolderId
			THEN  JSON_QUERY(Ep.fFolderItem(@FolderType, StdFolderId))
			END
		FROM Ep.tStdFolder WHERE FolderType = @FolderType AND ParentId=@FolderId
		order by StdFolderName
		FOR JSON PATH
	)
	RETURN @RetVal;
END