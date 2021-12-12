-- Version = 6.6.16, Requires={  }

ALTER procedure Ep.scRepoValueContentChange
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepoValueId		int,
	@CultureId		int,
	@Content			nvarchar(MAX)
)
as begin



	update [Ep].[tRepoValueRes] set Content = @Content where RepoValueId = @RepoValueId AND CultureId = @CultureId
	IF @@ROWCOUNT = 0 insert into Ep.tRepoValueRes (RepoValueId, CultureId, Content) VALUES( @RepoValueId, @CultureId, @Content )


	DECLARE @event nvarchar(MAX) = (
	select EventName = 'Ep.Basic.Message.Repo.Events.ValueContentChanged', RepoValueId = @RepoValueId
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
	




end
