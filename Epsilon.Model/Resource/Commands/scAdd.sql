
ALTER procedure EpRes.scAdd
(
	@_ActorId	int,
	@_CultureId	int,
	@_CommandId	nvarchar(256),
	@_Events nvarchar(MAX)  out,

	@ResName nvarchar(512),
	@Args nvarchar(512) = null
)
as begin

	IF not exists (select 1 from EpRes.tRes where ResName = @ResName)
	BEGIN
		insert into EpRes.tRes (ResName, Args) VALUES(@ResName, @Args)
		declare @resId int = SCOPE_IDENTITY();

		DECLARE @e NVARCHAR(MAX) =	(SELECT EventName = 'Epsilon.Model.Resource.Events.ResAdded', ResId = @resId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )  
		SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@e))
	END
END