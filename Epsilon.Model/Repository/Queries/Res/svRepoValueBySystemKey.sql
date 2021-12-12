-- Version = 6.6.16, Requires={  }

ALTER procedure Ep.svRepoValueBySystemKey
(
	
	@_ActorId			int,
	@_CultureId			int,

	@RepoSystemKey		varchar(255) = '',
	@ValueSystemKey		varchar(255) = ''
)
as begin


	select top 1
		rv.*
	from Ep.vRepoValue rv
	inner join Ep.tRepo r on r.RepoId = rv.RepoId
	where rv.CultureId = @_CultureId AND
	r.SystemKey = @RepoSystemKey AND rv.SystemKey = @ValueSystemKey

	FOR XML PATH('data'),  ELEMENTS, TYPE

end
