-- Version =  21.03.02, Requires={ Neva.RemoveNonASCII, Ep.sRepoValueCreate }

ALTER procedure Ep.scRepoValueCreate
(
	@_Events nvarchar(MAX)  out,
	@_ActorId			int,
	@_CultureId			int,
	@_CommandId			nvarchar(128),

	@RepoId					int,
	@CultureId		int,
	@RepositoryValueName	nvarchar(1024),
	
	@AlphaId			int = null
)
as begin


	IF @_ActorId > 1 AND exists (select 1 from Ep.tRepo r where r.RepoId = @RepoId AND r.AlphaId is not null)
		RAISERROR('Unable to edit Alpha Repo', 18, 1)

	declare @value nvarchar(128)
	declare @lastDotIdx int;
	declare @length int;

	set @length  = LEN(@RepositoryValueName);
	set @lastDotIdx =  CHARINDEX ('.', REVERSE(@RepositoryValueName));
	set @value = SUBSTRING (@RepositoryValueName, @length - @lastDotIdx +2 ,  @lastDotIdx + 1)
		
	IF @value is null OR @value = '' set @value = @RepositoryValueName;

	declare @RepoValueId int = 0
	exec Ep.sRepoValueCreate @RepoId, @CultureId, @RepositoryValueName, @value,'', @RepoValueId out
	IF @RepoValueId > 0
	BEGIN
		IF @AlphaId is not null
		update Ep.tRepoValue set AlphaId = @AlphaId where RepoValueId = @RepoValueId

		DECLARE @event nvarchar(MAX) = (
		select EventName = 'Ep.Basic.Message.Repo.Events.ValueChanged', RepoValueId = @RepoValueId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
		

	END

end
