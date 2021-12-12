-- Version = 21.03.02, Requires={  }

ALTER procedure Ep.scRepoValueChange
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepoValueId		int,
	@CultureId		int,
	@Value				nvarchar(128)
)
as begin


	IF @_ActorId > 1 AND exists (select 1 from Ep.tRepoValue rv inner join Ep.tRepo r on r.RepoId = rv.RepoId AND r.AlphaId is not null where rv.RepoValueId = @RepoValueId)
		RAISERROR('Unable to edit Alpha Repo', 18, 1)

	update [Ep].[tRepoValueRes] set [Value] = @Value where RepoValueId = @RepoValueId AND CultureId = @CultureId
	IF @@ROWCOUNT = 0 insert into Ep.tRepoValueRes (RepoValueId, CultureId, Value) VALUES( @RepoValueId, @CultureId, @Value )

	DECLARE @event nvarchar(MAX) = (
	select EventName = 'Ep.Basic.Message.Repo.Events.ValueChanged', RepoValueId = @RepoValueId
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
	




end
