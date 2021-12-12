-- Version = *, Requires={ Neva.RemoveNonASCII }

alter procedure Ep.sRepoValueCreate
(
	@RepoId					int,
	@CultureId		int,
	@RepositoryValueName	nvarchar(1024),
	@value nvarchar(128),
	@systemKey nvarchar(255)='',
	@RepoValueId int out
)
as begin


	set @RepositoryValueName = Neva.RemoveNonASCII(@RepositoryValueName);

	IF NOT EXISTS (select 1 from [Ep].[tRepoValue] where RepoId = @RepoId AND Name = @RepositoryValueName)
	BEGIN
	
		insert into [Ep].[tRepoValue] (RepoId, Name, SystemKey)
		VALUES (@RepoId, @RepositoryValueName, ISNULL(@systemKey,''))

		set @RepoValueId = SCOPE_IDENTITY();

		insert into Ep.tRepoValueRes (RepoValueId, CultureId, Value)
		values (@RepoValueId, @CultureId, @value)


	END

end
