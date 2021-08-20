IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Ep].[tActor]') AND type in (N'U'))
BEGIN

CREATE TABLE Ep.tActor
(
	ActorId			int not null IDENTITY(0,1)
	constraint PK_tActor primary key clustered ( ActorId )
)

CREATE TABLE Ep.tUser
(
	UserId			int not null,
	constraint PK_tUser primary key clustered ( UserId ),

	constraint FK_tUser_UserId foreign key( UserId ) 
		references Ep.tActor( ActorId ),
)

CREATE TABLE Ep.tGroup
(
	GroupId			int not null,
	GroupName		nvarchar( 128 ) not null ,
	GroupKey			nvarchar( 128 ) null ,
	IsShared	bit not null constraint DF_tGroup_IsShared default( 1 ),	

	OwnerId		int not null,	
			
	
	CreationDate	datetime2(0) not null constraint DF_tGroup_CreationDate default( GETUTCDATE() ),	
	constraint PK_tGroup primary key clustered ( GroupId ),
	constraint FK_tGroup_GroupId foreign key( GroupId ) 
		references Ep.tActor( ActorId ),
	constraint FK_tGroup_OwnerId foreign key( OwnerId ) 
		references Ep.tUser( UserId )
)


CREATE TABLE Ep.tActorProfile
(
	ActorId			int not null ,
	GroupId			int not null ,
	constraint PK_tActorProfile primary key clustered ( ActorId, GroupId ),
	constraint FK_tActorProfile_UserId foreign key( ActorId ) 
		references Ep.tActor( ActorId ),
	constraint FK_tActorProfile_GroupId foreign key( GroupId ) 
		references Ep.tGroup( GroupId )
)

SET IDENTITY_INSERT Ep.tActor ON;
insert into Ep.tActor (ActorId) values(0)
insert into Ep.tActor (ActorId) values(1)
SET IDENTITY_INSERT Ep.tActor OFF;

INSERT INTO Ep.tUser (UserId) VALUES(0),(1);
INSERT INTO Ep.tGroup (GroupId, GroupName, GroupKey, OwnerId) VALUES(0, 'anonymous', 'anonymous', 1),(1, 'system', 'system', 1);

INSERT INTO Ep.tActorProfile (ActorId, GroupId) VALUES(0, 0),(1, 0), (1, 1);

END