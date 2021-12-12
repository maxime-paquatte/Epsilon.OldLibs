-- Version = 21.03.02, Package = Ep.RepoValueHome, Requires={  }

ALTER procedure Ep.scRepoValueRename
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepoValueId			int,
	@RepositoryValueName	nvarchar(128)
)
as begin


	IF @_ActorId > 1 AND exists (select 1 from Ep.tRepoValue rv inner join Ep.tRepo r on r.RepoId = rv.RepoId AND r.AlphaId is not null where rv.RepoValueId = @RepoValueId)
		RAISERROR('Unable to edit Alpha Repo', 18, 1)

	declare @OldName nvarchar(128), @repoId int
	select @OldName = Name, @repoId = RepoId from  [Ep].[tRepoValue] where RepoValueId = @RepoValueId


	update [Ep].[tRepoValue] 
	set Name = STUFF(Name, CHARINDEX(@OldName, Name), LEN(@OldName), @RepositoryValueName)
	where Name like @OldName + '%'
	

	IF @@ROWCOUNT > 0
	BEGIN
		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.ValueChanged', RepoValueId = @RepoValueId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		
	END


end
