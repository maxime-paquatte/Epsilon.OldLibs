-- Version = 6.6.16, Requires={ }

ALTER procedure Ep.svDbObjRepoValue
(
	
	@_ActorId			int,
	@_CultureId			int,

	@DbObjId			int ,
	@RepoId				int 
)
as begin


	declare @dt int;
	select @dt = DbObjTypeId from Ep.tDbObj where DbObjId = @DbObjId;

	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select "@json:Array" = 'true', drv.*
	from Ep.tDbObjRepoValue dr			
	inner join Ep.vRepoValue drv
		on dr.RepoValueId = drv.RepoValueId AND drv.CultureId = @_CultureId
	inner join Ep.tRepo r
		on r.RepoId = drv.RepoId
	where  dr.DbObjId = @DbObjId  
	AND drv.RepoId = @RepoId
	FOR XML PATH('Values'),  ELEMENTS, TYPE

end
