-- Version = 20.12.21, Requires={  }

ALTER procedure Ep.svRepoValueTypeahead
(
	
	@_ActorId			int,
	@_CultureId			int,

	@RepoId				int ,
	@SystemKey			varchar(255) = '',
	@Pattern			varchar(255) = ''
)
as begin


	if @RepoId is null OR @RepoId = 0
		select @RepoId = RepoId from Ep.tRepo r where r.SystemKey = @SystemKey;

	select
		rv.*
	from Ep.vRepoValue rv
	inner join Ep.tRepo r on rv.RepoId = r.RepoId
	where r.RepoId = @RepoId AND rv.CultureId = @_CultureId
	AND LOWER(rv.Path) like '%' + LOWER(@Pattern) + '%'
	ORDER BY
        CASE WHEN @Pattern != '' OR r.OrderByValue = 0 THEN rv.Name END,
        CASE WHEN  r.OrderByValue = 1 THEN rv.Value END
	  
	FOR JSON PATH, INCLUDE_NULL_VALUES

end
