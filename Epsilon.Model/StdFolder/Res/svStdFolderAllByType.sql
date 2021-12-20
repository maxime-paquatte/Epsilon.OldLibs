-- Version =*, Requires={  }

ALTER procedure Ep.svStdFolderAllByType
(
	
	@_ActorId		int,
	@_CultureId		int,

	@FolderType		nvarchar(96)
)
as begin

	select f.*,
		Items = JSON_QUERY(Ep.fFolderItem(@FolderType, f.StdFolderId))
	from Ep.tStdFolder f
	where f.StdFolderId = 0
	AND f.ParentId is null
	order by f.StdFolderName
	FOR JSON PATH

end
