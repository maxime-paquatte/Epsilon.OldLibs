-- Version = 9.09.17, Package = Ep.RepoHome, Requires={  }

ALTER procedure Ep.scRepoCreate
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepositoryName		nvarchar(255)
)
as begin


	SET XACT_ABORT ON;
	BEGIN TRANSACTION;	

	insert into [Ep].tRepo ( Name)
	VALUES( @RepositoryName);
	
	declare @RepositoryId int = SCOPE_IDENTITY();

	DECLARE @event nvarchar(MAX) = (
	select EventName = 'Ep.Basic.Message.Repo.Events.Created', RepositoryId = @RepositoryId
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
	

	COMMIT TRANSACTION;

end
