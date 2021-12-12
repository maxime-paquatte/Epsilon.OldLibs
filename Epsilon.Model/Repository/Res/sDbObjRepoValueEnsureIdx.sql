-- Version = 4.7.31, Package = Ep.DbObjRepoValueHome, Requires={ }

ALTER procedure Ep.sDbObjRepoValueEnsureIdx(
	@DbObjId int = null
) 
AS BEGIN

	with idxs as
	(
		select ROW_NUMBER() OVER(PARTITION BY [DbObjId], rv.RepoId ORDER BY Idx, drv.RepoValueId) AS Row, 
			drv.RepoValueId, DbObjId from [Ep].[tDbObjRepoValue] drv
			inner join Ep.tRepoValue rv on rv.RepoValueId = drv.RepoValueId
			where @DbObjId is null OR [DbObjId] = @DbObjId
	)
	update sc set Idx = idxs.Row from idxs 
	inner join [Ep].[tDbObjRepoValue] sc
		on sc.RepoValueId = idxs.RepoValueId and sc.DbObjId = idxs.DbObjId;

END