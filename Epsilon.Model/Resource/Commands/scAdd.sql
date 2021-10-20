
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
	set @ResName = TRIM(@ResName);
	declare @resId int, @name nvarchar(512)
	select @resId = ResId, @name = ResName from EpRes.tRes where ResName = @ResName COLLATE Latin1_General_CI_AS
	
	--never createated
	IF @resId is null
	BEGIN
		insert into EpRes.tRes (ResName, Args) VALUES(@ResName, @Args)
		set @resId = SCOPE_IDENTITY();

		DECLARE @e NVARCHAR(MAX) =	(SELECT EventName = 'Epsilon.Model.Resource.Events.ResAdded', ResId = @resId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )  
		SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@e))
	END
	--Fix bug when the res is renamed with an other case
	ELSE IF @name != @ResName COLLATE Latin1_General_CS_AS
		UPDATE EpRes.tRes set ResName = @ResName where ResId = @resId

END

select 1 from EpRes.tRes where ResName = 'Res.Model.Cms.Page.Config' COLLATE Latin1_General_CS_AS