-- Version = 4.11.18, Package = Ep.DbObjRepoValueHome, Requires={  }

ALTER procedure Ep.scSetDbObjRepoValue
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@DbObjId		int = 0,
	@RepoId			int = 0,
	@RepoValueIds		nvarchar(MAX)= ''
)
as begin


	SET XACT_ABORT ON;
	BEGIN TRANSACTION;

	declare @changeCount int = 0;

	declare @values table(id int);
	insert into @values
	select CAST(ids.value as int) 
	from STRING_SPLIT(@RepoValueIds, ',') ids;



	delete from v
	from Ep.tDbObjRepoValue v
	inner join Ep.tRepoValue rv on rv.RepoValueId = v.RepoValueId
	inner join Ep.tRepo r on rv.RepoId = r.RepoId
	left outer join @values nv on nv.id = v.RepoValueId
	where nv.id is null
	AND v.DbObjId = @DbObjId AND r.RepoId = @RepoId

	set @changeCount = @changeCount + @@ROWCOUNT;

	insert into Ep.tDbObjRepoValue([DbObjId], [RepoValueId])
	select @DbObjId , rv.RepoValueId
	from @values nv
	inner join Ep.tRepoValue rv on rv.RepoValueId = nv.id
	inner join Ep.tRepo r on rv.RepoId = r.RepoId
	left outer join Ep.tDbObjRepoValue v
		on v.RepoValueId = nv.id and v.DbObjId = @DbObjId
	where v.RepoValueId is null
		AND r.RepoId = @RepoId

	set @changeCount = @changeCount + @@ROWCOUNT;

	IF @changeCount > 0
	BEGIN
		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.DbObjRepoValueChanged', DbObjId = @DbObjId , RepoId = @RepoId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		
	END

	COMMIT TRANSACTION;

end
