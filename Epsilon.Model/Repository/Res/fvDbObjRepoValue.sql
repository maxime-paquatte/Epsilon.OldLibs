-- Version = 6.6.16, Requires={ }
ALTER FUNCTION Ep.fvDbObjRepoValue(@DbObjId int, @SystemKey varchar(96), @CultureId int ) returns xml
AS
BEGIN
	declare @xml xml;

	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select @xml =
	(
		select "@json:Array" = 'true', r.RepoId, RepoName = r.Name, rv.RepoValueId, rv.Name, Value, Path, Content,DateAdded
		from Ep.tDbObjRepoValue drv
		inner join Ep.vRepoValue rv on drv.RepoValueId = rv.RepoValueId
			AND rv.CultureId = @CultureId
		inner join Ep.tRepo r on rv.RepoId = r.RepoId
		where drv.DbObjId = @DbObjId AND r.SystemKey = @SystemKey
		order by rv.Name
		FOR XML PATH ('data'),  ELEMENTS, TYPE
	)
	return @xml;
END