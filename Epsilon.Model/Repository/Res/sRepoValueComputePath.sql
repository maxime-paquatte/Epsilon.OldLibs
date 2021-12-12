-- Version = 6.6.20, Requires={ Ep.vRepoValuePath }

ALTER procedure Ep.sRepoValueComputePath
(
	@RepoId int = 0
)
as begin


	update rvr set 
		Path = ISNULL(p.path, pd.path)
	from [Ep].tRepoValueRes rvr
	inner join Ep.tRepoValue rv on rv.RepoValueId = rvr.RepoValueId
	inner join Ep.vRepoValuePath p 
		on rvr.RepoValueId = p.RepoValueId AND rvr.CultureId = p.CultureId
	inner join Ep.vRepoValuePath pd 
		on rvr.RepoValueId = p.RepoValueId AND pd.CultureId = 9
	where @RepoId = 0 OR rv.RepoId =  @RepoId



end
