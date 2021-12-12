-- Version = 3.09.09, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.svRepoForDbObjType
(
	
	@_ActorId			int,
	@_CultureId			int,

	@DbObjTypeName		varchar(96) 
)
as begin

	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select
		"@json:Array" = 'true',
		r.RepoId,
		r.Name,
		r.SystemKey
		
	from Ep.tRepo r
	inner join Ep.tRepoDbObjType rdt on r.RepoId = rdt.RepoId
	inner join Ep.tDbObjType dt on rdt.DbObjTypeId = dt.DbObjTypeId
	where dt.Name = @DbObjTypeName

	FOR XML PATH('data'), root('data'),  ELEMENTS, TYPE

end
