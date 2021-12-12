-- Version = 4.11.18, Package = Ep.DbObjRepoValueHome, Requires={  }

ALTER procedure Ep.scDbObjRepoValueImport
(
	@_Events nvarchar(MAX)  out,
	@_ActorId		int,
	@_CultureId		int,
	@_CommandId		nvarchar(128),

	@DbObjId		int,
	@Data			xml
)
as begin


	DECLARE @t TABLE (DbObjId int, RepoValueId  int) 
	INSERT INTO @t (DbObjId, RepoValueId)
	select  
		@DbObjId,
		RepoValueId = rv.RepoValueId
	from @Data.nodes('RepoValues[1]/RepoValue') as t(c)
	inner join [Ep].tRepo r on r.SystemKey = t.c.value('Repo[1]','varchar(96)')
	inner join [Ep].tRepoValue rv on r.RepoId = rv.RepoId
		AND rv.Name like t.c.value('Value[1]','varchar(255)')


	merge Ep.tDbObjRepoValue as target
	using ( 
		select l.DbObjId, l.RepoValueId
		from @t l ) as source
	on 
	(
		 target.DbObjId = source.DbObjId and 
		 target.RepoValueId = source.RepoValueId
	)
	when not matched by target then
		insert (DbObjId, RepoValueId) 
		values (source.DbObjId, source.RepoValueId)	;

	IF @@ROWCOUNT > 0
	BEGIN
		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.DbObjRepoValueChanged', DbObjId = @DbObjId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		
	END


end
