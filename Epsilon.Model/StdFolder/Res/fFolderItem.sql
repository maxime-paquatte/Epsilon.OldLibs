-- Version =*, Requires={  }

CREATE FUNCTION Ep.fFolderItem(@FolderType nvarchar(96), @FolderId int)
RETURNS XML 
WITH RETURNS NULL ON NULL INPUT 
AS BEGIN

	DECLARE @RetVal XML;	
	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
  
	SELECT @RetVal = (
		SELECT "@json:Array" = 'true', *,
			CASE WHEN ParentId=@FolderId
			THEN Ep.fFolderItem(@FolderType, StdFolderId)
			END
		FROM Ep.tStdFolder WHERE FolderType = @FolderType AND ParentId=@FolderId
		order by StdFolderName
		FOR XML PATH('Folders'), TYPE
	)
	RETURN @RetVal;
END