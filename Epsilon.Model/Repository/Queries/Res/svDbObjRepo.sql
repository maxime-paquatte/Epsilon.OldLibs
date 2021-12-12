-- Version = 6.6.16, Package = Ep.DbObjRepoValueHome, Requires={  }

ALTER procedure Ep.svDbObjRepo
(
	
	@_ActorId			int,
	@_CultureId			int,

	@DbObjId			int ,
	@SystemKeys			varchar(96) = ''
)
as begin


	declare @repoIds table(id int)
	IF @SystemKeys <> ''
	BEGIN
		insert into @repoIds
		select r.RepoId from STRING_SPLIT(@SystemKeys, '|') s
		inner join Ep.tRepo r on r.SystemKey = s.value
	END
	ELSE
	BEGIN
		insert into @repoIds
		select rd.RepoId from Ep.tDbObj d
		inner join Ep.tRepoDbObjType rd on rd.DbObjTypeId = d.DbObjTypeId
		where d.DbObjId = @DbObjId;
	END;


	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select
		"@json:Array" = 'true',
		r.RepoId,
		r.Name,
		r.ShowDate,
		r.SystemKey,
		r.Sortable,
		(
			select "@json:Array" = 'true', drv.*, dr.Idx, dr.DateAdded, DbObjId = @DbObjId
			from Ep.tDbObjRepoValue dr			
			inner join Ep.vRepoValue drv
				on dr.RepoValueId = drv.RepoValueId	AND drv.CultureId = @_CultureId
			where dr.DbObjId = @DbObjId  and drv.RepoId = r.RepoId
			ORDER BY 
			  CASE WHEN r.Sortable = 1 THEN dr.Idx END,
			  CASE WHEN r.OrderByValue = 1 THEN drv.Value END,
			  CASE WHEN r.Sortable = 0 THEN drv.Name END
			FOR XML PATH('Values'),  ELEMENTS, TYPE
		)
	from Ep.tRepo r
	inner join @repoIds ids
		on r.RepoId = ids.id
	FOR XML PATH('data'), root('data'),  ELEMENTS, TYPE

end
