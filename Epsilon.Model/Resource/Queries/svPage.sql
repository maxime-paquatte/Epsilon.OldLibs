
ALTER procedure EpRes.svPage
(
	@_ActorId	int,
	@_CultureId	int,

	@Skip int,
	@Take int
)
as begin

--declare 	@Skip int = 0, 	@Take int = 50
	SELECT r.ResId, r.ResName, r.Args,
	(
		select c.CultureId, c.CultureKey,ResValue = ISNULL(rv.ResValue, '')
		from Ep.tCulture c
		left outer join EpRes.tResValue rv
			on   rv.ResId = r.ResId AND rv.CultureId = c.CultureId
		order by c.CultureId
		FOR JSON PATH  
	) as 'Values'
	from EpRes.tRes r		
	ORDER BY r.ResId DESC OFFSET @Skip ROWS 
	FETCH NEXT IIF(@Take = 0, 50, @Take) ROWS ONLY
	FOR JSON PATH, INCLUDE_NULL_VALUES    

END