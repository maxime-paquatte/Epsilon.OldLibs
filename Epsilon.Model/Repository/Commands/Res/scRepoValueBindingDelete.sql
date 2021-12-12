-- Version = *, Requires={  }
ALTER procedure Ep.scRepoValueBindingDelete
(
    @_Events nvarchar(MAX)  out,
    @_ActorId		    int,
    @_CultureId		int,
    @_CommandId		nvarchar(128),

    
    @RepoValueSourceId int,    
    @RepoValueTargetId int
    
)
as begin


	delete from Ep.tRepoValueBinding where RepoValueSourceId = @RepoValueSourceId  AND RepoValueTargetId = @RepoValueTargetId     
		OR RepoValueTargetId  = @RepoValueSourceId  AND RepoValueSourceId= @RepoValueTargetId     

	
    IF @@ROWCOUNT > 0
    BEGIN
        DECLARE @event nvarchar(MAX) = (select EventName = 'Ep.Basic.Message.Repo.Events.BindingDeleted'         
            ,RepoValueSourceId = @RepoValueSourceId        
            ,RepoValueTargetId = @RepoValueTargetId      
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
        
    END


end