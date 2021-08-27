
ALTER procedure Ep.svCultureAll
(
	@_ActorId	int,
	@_CultureId	int
)
as begin

	SELECT *
	from Ep.tCulture
	order by CultureId
	FOR JSON PATH

END