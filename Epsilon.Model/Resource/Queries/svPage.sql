
ALTER procedure EpRes.svPage
(
	@_ActorId	int,
	@_CultureId	int,

	@Prefixes nvarchar(MAX) = '',
	@Skip int,
	@Take int
)
as begin
    
    set @Prefixes = REPLACE(@Prefixes, '*', '%');

--declare 	@Skip int = 0, 	@Take int = 50
	SELECT r.ResId, r.ResName, r.Args, r.Comment,
	[Values] = (
		select r.ResId,c.CultureId, c.CultureKey, ResValue = ISNULL(rv.ResValue, '')
		from Ep.tCulture c
		left outer join EpRes.tResValue rv
			on   rv.ResId = r.ResId AND rv.CultureId = c.CultureId
		where c.CultureId > 0
		order by c.CultureId
		FOR JSON PATH  
	)
	from EpRes.tRes r		
	inner join STRING_SPLIT(@Prefixes, '|')  pref
		on r.ResName like pref.value + '%'COLLATE Latin1_General_CI_AS 
	ORDER BY r.ResId DESC OFFSET @Skip ROWS 
	FETCH NEXT IIF(@Take = 0, 50, @Take) ROWS ONLY
	FOR JSON PATH, INCLUDE_NULL_VALUES    

END