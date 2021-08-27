
ALTER procedure EpRes.scValueSet
(
	@_ActorId	int,
	@_CultureId	int,
	@_CommandId	nvarchar(256),
	@_Events nvarchar(MAX)  out,

	@ResId int,
	@CultureId int,
	@ResValue nvarchar(MAX) = ''
)
as begin

	Update EpRes.tResValue set ResValue = @ResValue where ResId = @ResId AND CultureId = @CultureId
	IF @@ROWCOUNT = 0
		insert into  EpRes.tResValue (ResId, CultureId, ResValue) VALUES (@ResId, @CultureId, @ResValue )

	DECLARE @e NVARCHAR(MAX) =	(SELECT EventName = 'Epsilon.Model.Resource.Events.ResValueChanged', 
		ResId = @resId, CultureId= @CultureId, ResValue = @ResValue FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )  
	SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@e))

END