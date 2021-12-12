-- Version =*, Requires={  }

ALTER procedure Ep.svStdFolderAllByType
(
	
	@_ActorId		int,
	@_CultureId		int,

	@FolderType		nvarchar(96)
)
as begin

	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select 
		"@json:Array" = 'true',
		f.*,
		Ep.fFolderItem(@FolderType, f.StdFolderId)
	from Ep.tStdFolder f
	where f.StdFolderId = 0
	AND f.ParentId is null
	order by f.StdFolderName

	FOR XML PATH('data'), root('data'),  ELEMENTS, TYPE

end
