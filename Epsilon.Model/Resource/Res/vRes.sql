-- Version = *

ALTER VIEW EpRes.vRes as
(
	select rc.CultureId, rc.CultureKey, r.ResId, r.ResName, Value = IIF(rc.CultureId = -1, r.ResName, rv.ResValue), Fallback = ISNULL(rvD.ResValue, '?' + r.ResName)
	from EpRes.tRes r
	cross join Ep.tCulture rc
	left outer join EpRes.tResValue rvD on rvD.CultureId = 9 AND rvD.ResId = r.ResId
	left outer join EpRes.tResValue rv on rv.CultureId = rc.CultureId AND rv.ResId = r.ResId
	where r.ResId != 0
)