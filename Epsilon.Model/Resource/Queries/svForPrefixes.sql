
ALTER procedure EpRes.svForPrefixes
(
	@_ActorId	int,
	@_CultureId	int,

	@CultureId int,
	@Prefixes nvarchar(MAX) = ''
)
as begin

	--declare @CultureId int = 9,	@Prefixes nvarchar(MAX) = 'Res.'
	SELECT r.*, rv.ResValue
	from EpRes.tRes r
	inner join STRING_SPLIT(@Prefixes, '|')  pref
		on r.ResName like pref.value + '%'COLLATE Latin1_General_CI_AS 
	left outer join EpRes.tResValue rv 
		on rv.ResId = r.ResId AND rv.CultureId = @CultureId
	
	FOR JSON PATH

END