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

	select drv.*
	from Ep.tDbObjRepoValue dr			
	inner join Ep.vRepoValue drv
		on dr.RepoValueId = drv.RepoValueId AND drv.CultureId = @_CultureId
	inner join Ep.tRepo r
		on r.RepoId = drv.RepoId
	where  dr.DbObjId = @DbObjId  
	AND drv.RepoId = @RepoId
	FOR JSON PATH, INCLUDE_NULL_VALUES

end
