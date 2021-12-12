-- Version = *, Requires={  }
ALTER procedure Ep.scRepoValueBindingAdd
(
    @_Events nvarchar(MAX)  out,
    @_ActorId		    int,
    @_CultureId		int,
    @_CommandId		nvarchar(128),

    
    @RepoValueSourceId int,    
    @RepoValueTargetId int
    
)
as begin


	DECLARE @event nvarchar(MAX);
	IF not exists (select 1 from Ep.tRepoValueBinding where RepoValueSourceId = @RepoValueSourceId AND RepoValueTargetId = @RepoValueTargetId)
	BEGIN
		--if reverse binding not exists -->  create binding
		IF not exists (select 1 from Ep.tRepoValueBinding where RepoValueSourceId = @RepoValueTargetId AND RepoValueTargetId = @RepoValueSourceId)
		BEGIN
			insert into Ep.tRepoValueBinding (RepoValueSourceId, RepoValueTargetId) values (@RepoValueSourceId, @RepoValueTargetId)
		
			set @event = (select EventName = 'Ep.Basic.Message.Repo.Events.BindingCreated'         
				,RepoValueSourceId = @RepoValueSourceId       
				,RepoValueTargetId = @RepoValueTargetId     
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
			
		END
		-- ELSE set BothWay on reverse binding
		ELSE
		BEGIN
			Update Ep.tRepoValueBinding set BothWay = 1 where RepoValueSourceId = @RepoValueTargetId AND RepoValueTargetId = @RepoValueSourceId

			set @event = (select EventName = 'Ep.Basic.Message.Repo.Events.BindingChanged'         
				,RepoValueSourceId = @RepoValueSourceId       
				,RepoValueTargetId = @RepoValueTargetId     
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER )
SET @_Events =JSON_MODIFY(@_Events, 'append $',JSON_QUERY(@event))
			
		END

	END


end