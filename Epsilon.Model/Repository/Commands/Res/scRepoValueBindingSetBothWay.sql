-- Version = *, Requires={  }
ALTER procedure Ep.scRepoValueBindingSetBothWay
(
    @_Events nvarchar(MAX)  out,
    @_ActorId		    int,
    @_CultureId		int,
    @_CommandId		nvarchar(128),

    
    @RepoValueSourceId int,    
    @RepoValueTargetId int,    
    @BothWay bit
    
)
as begin



	update Ep.tRepoValueBinding set BothWay = @BothWay where RepoValueSourceId = @RepoValueSourceId  AND RepoValueTargetId = @RepoValueTargetId     

	IF @@ROWCOUNT > 0
    BEGIN
        DECLARE @event nvarchar(MAX) = (select EventName = 'Ep.Basic.Message.Repo.Events.BindingChanged'         
            ,RepoValueSourceId = @RepoValueSourceId        
            ,RepoValueTargetId = @RepoValueTargetId      
        
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
        
    END


end
