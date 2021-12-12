-- Version = 21.10.12, Requires={  }

ALTER procedure Ep.svRepoValueChildren
(
	
	@_ActorId			int,
	@_CultureId			int,

	@RepoId				int ,
	@SystemKey			varchar(255) = '',
	@Path				varchar(255) = '',
	@AllDepth			bit = 0
)
as begin


	if @RepoId is null OR @RepoId = 0
		select @RepoId = RepoId from Ep.tRepo r where r.SystemKey = @SystemKey;

	declare @rootDepth int = 0
	if @Path <> ''
	set  @rootDepth = LEN(@Path) - LEN(REPLACE(@Path, '.', '')) + 1;

	WITH XMLNAMESPACES ('http://james.newtonking.com/projects/json' as json)
	select
		"@json:Array" = 'true',
		rv.*,
		Depth = LEN(rv.Name) - LEN(REPLACE(rv.Name, '.', '')),
		Descendants = ( select count(*) from Ep.tRepoValue a where a.Name like rv.Name + '.%' and a.RepoId = r.RepoId  )
	from Ep.vRepoValue rv
	inner join Ep.tRepo r on rv.RepoId = r.RepoId
	where r.RepoId = @RepoId AND rv.CultureId = @_CultureId
	AND rv.Name like @Path + '%'
	AND( @AllDepth = 1 OR LEN(rv.Name) - LEN(REPLACE(rv.Name, '.', '')) - @rootDepth = 0)
	ORDER BY 
	  CASE WHEN r.OrderByValue = 1 THEN rv.Value END,
	  CASE WHEN r.OrderByValue = 0 THEN rv.Name END

	FOR JSON PATH

end
