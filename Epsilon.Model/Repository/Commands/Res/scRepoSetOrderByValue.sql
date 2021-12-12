-- Version = 4.11.18, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.scRepoSetOrderByValue
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepositoryId		int,
	@OrderByValue			bit
)
as begin



	update Ep.tRepo
	set OrderByValue = @OrderByValue
	where RepoId = @RepositoryId


	IF @@ROWCOUNT > 0
	BEGIN
		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.Changed', RepositoryId = @RepositoryId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		
	END


end
