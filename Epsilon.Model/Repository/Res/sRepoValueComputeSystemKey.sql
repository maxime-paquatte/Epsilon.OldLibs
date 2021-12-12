-- Version = 4.8.1, Package = Ep.RepoValueHome, Requires={ Neva.fCharCount }

ALTER procedure Ep.sRepoValueComputeSystemKey
as begin


	truncate table Ep.tRepoValueComputedSystemKey;

	WITH ValueSystemKey ( RepoId, RepoValueId, Name,  SystemKey, lvl )
	AS
	(
	-- Anchor member definition
		select RepoId, RepoValueId, Name, SystemKey, 0 from Ep.tRepoValue
		where  [Neva].[fCharCount](Name, '.') = 0
	UNION ALL
	-- Recursive member definition
		select r.RepoId, r.RepoValueId, r.Name, IIF(r.SystemKey = '', p.SystemKey, r.SystemKey) , p.lvl +1
		from Ep.tRepoValue r
		inner join ValueSystemKey p 
			on r.RepoId = p.RepoId
			AND r.Name like p.Name + '.%' 
			AND [Neva].[fCharCount](r.Name, '.') = p.lvl +1
	)
	-- Statement using the CTE
	insert into Ep.tRepoValueComputedSystemKey ( RepoValueId, SystemKey)
	SELECT sk.RepoValueId, sk.SystemKey
	FROM ValueSystemKey sk
	

end
