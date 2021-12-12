-- Version = 4.11.18, Package = Ep.DbObjHome, Requires={  }

ALTER procedure Ep.sDbObjCreate
(
	@CreatorActorId		int,
	@DbObjType			nvarchar(128),

	@DbObjId int out
)
as begin
--[beginsp]

	insert into Ep.tDbObj (DbObjTypeId, CreatorId)
	select dt.DbObjTypeId, @CreatorActorId from Ep.tDbObjType dt
	where dt.Name = @DbObjType

	IF @@ROWCount = 0 RAISERROR ('@DbObjType not found %s', 16, 1, @DbObjType); 

	set @DbObjId = SCOPE_IDENTITY();

--[endsp]
end
