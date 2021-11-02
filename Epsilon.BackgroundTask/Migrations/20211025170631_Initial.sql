

CREATE TABLE Ep.tBackgroundTask
(
	TaskKey		varchar(512) not null UNIQUE,
	TaskType	varchar(1024) not null,
	JsonConfig  varchar(MAX) ,

	NextRun	datetime2(0) null,
	LastRun datetime2(0) null,
	
	NbRun int not null
		constraint DF_tBackgroundTask_NbRun default(0),

	State nvarchar(128) not null
		constraint DF_tBackgroundTask_State default('waiting'),

	ShouldRun as
	(
		CAST(IIF(State = 'waiting' AND (NextRun is null OR NextRun < GETUTCDATE()), 1, 0) as bit)
	),	

	constraint PK_tBackgroundTask primary key clustered ( TaskKey ) 
)
