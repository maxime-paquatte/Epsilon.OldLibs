
ALTER procedure EpRes.scDelete
(
	@_ActorId	int,
	@_CultureId	int,
	@_CommandId	nvarchar(256),
	@_Events nvarchar(MAX)  out,

	@ResId int
)
as begin

	delete from EpRes.tResValue WHERE ResId = @ResId
	delete from EpRes.tRes WHERE ResId = @ResId
	
	DECLARE @e NVARCHAR(MAX) =	(SELECT EventName = 'Epsilon.Model.Resource.Events.Deleted', 
		ResId = @ResId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )  
	SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@e))

END