-- Version = 21.03.02, Package = Ep.RepoValueHome, Requires={  }

ALTER procedure Ep.scRepoValueDelete
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepoValueId		int
)
as begin


	IF @_ActorId > 1 AND exists (select 1 from Ep.tRepoValue rv inner join Ep.tRepo r on r.RepoId = rv.RepoId AND r.AlphaId is not null where rv.RepoValueId = @RepoValueId)
		RAISERROR('Unable to edit Alpha Repo', 18, 1)

	SET XACT_ABORT ON;
		BEGIN TRANSACTION;	

		declare @parentId int
		select top 1 @parentId = p.RepoValueId
		from Ep.tRepoValue v left outer join Ep.tRepoValue p
			on p.RepoId = v.RepoId AND v.Name like p.Name + '.%'
		where v.RepoValueId = @RepoValueId order by LEN(p.Name) DESC

		update a set RepoValueId = @parentId 
		from [Ep].[tDbObjRepoValue] a
		left outer join [Ep].[tDbObjRepoValue]  b
			on a.DbObjId = b.DbObjId AND b.RepoValueId = @parentId
		where a.RepoValueId = @RepoValueId AND b.DbObjId is null

		update a set RepoValueId = @parentId 
		from Emat.tEmatObjFieldRepoValue a
		left outer join Emat.tEmatObjFieldRepoValue  b
			on a.EmatObjFieldId = b.EmatObjFieldId AND b.RepoValueId = @parentId
		where a.RepoValueId = @RepoValueId AND b.EmatObjFieldId is null

		
		delete from [Ep].[tDbObjRepoValue] where RepoValueId = @RepoValueId
		delete from Emat.tEmatObjFieldRepoValue where RepoValueId = @RepoValueId
		delete from [Ep].[tRepoValueComputedSystemKey] where RepoValueId = @RepoValueId
		delete from [Ep].tRepoValueRes where RepoValueId = @RepoValueId
		delete from [Ep].tRepoValue where RepoValueId = @RepoValueId

			
		IF @@ROWCOUNT > 0
		BEGIN
			DECLARE @event nvarchar(MAX) = (
			select EventName = 'Ep.Basic.Message.Repo.Events.ValueDeleted', RepoValueId = @RepoValueId
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
			
		END

		COMMIT TRANSACTION;

end
