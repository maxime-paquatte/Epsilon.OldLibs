-- Version = 4.11.18, Package = Ep.ActorHome, Requires={ Ep.sDbObjCreate }

ALTER procedure Ep.sActorCreate
(
	@CreatorActorId		int,
	@DbObjType			nvarchar(128),

	@ActorId int out
)
as begin
--[beginsp]

	set xact_abort on
	Begin tran
	insert into Ep.tActor default values
	select @ActorId = SCOPE_IDENTITY();
	commit
	
--[endsp]
end
