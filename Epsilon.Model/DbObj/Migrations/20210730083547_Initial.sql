IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Ep].[tDbObj]') AND type in (N'U'))
BEGIN

CREATE TABLE Ep.tDbObjType
(
	DbObjTypeId		int not null identity( 1,1 ),
	Name			varchar(96),
	DisplayName		varchar(256) constraint DF_tDbObjType_DisplayName default(''),

	constraint PK_tDbObjType primary key clustered ( DbObjTypeId ) 
)



SET IDENTITY_INSERT Ep.tDbObjType ON;
INSERT INTO Ep.tDbObjType (DbObjTypeId, Name) VALUES(0, '');
SET IDENTITY_INSERT Ep.tDbObjType OFF;


CREATE TABLE Ep.tDbObj
(
	DbObjId			int not null identity( 1,1 ),
	DbObjTypeId		int not null,
	CreationDate	datetime2 not null constraint DF_tDbObj_CreationDate default GETUTCDATE(),	
	CreatorId		int not null constraint DF_tDbObj_CreatorId default 0,	
	timestamp

	constraint PK_tDbObj primary key clustered ( DbObjId ) ,
	constraint FK_tDbObj_DbObjTypeId foreign key( DbObjTypeId ) 
		references Ep.tDbObjType( DbObjTypeId ),
	constraint FK_tDbObj_CreatorId foreign key( CreatorId ) 
		references  Ep.tUser( UserId )
)

--drop index IX_tDbObj_CreatorId  ON [Ep].[tDbObj]
CREATE NONCLUSTERED INDEX IX_tDbObj_CreatorId
ON [Ep].[tDbObj] ([CreatorId])
INCLUDE ([DbObjId], DbObjTypeId, CreationDate)



SET IDENTITY_INSERT Ep.tDbObj ON;
INSERT INTO Ep.tDbObj (DbObjId, DbObjTypeId) VALUES(0, 0);
SET IDENTITY_INSERT Ep.tDbObj OFF;



CREATE TABLE Ep.tDbObjMerged
(
	OldDbObjId			int not null,
	NewDbObjId			int not null,
	MergeDate			datetime2 not null constraint DF_tDbObjMerged_MergeDate default(GETUTCDATE()),

	constraint PK_tDbObjMerged primary key clustered ( OldDbObjId ) ,

	constraint FK_tDbObjMerged_NewDbObjId foreign key( NewDbObjId ) 
		references Ep.tDbObj( DbObjId )
)



--CREATE TABLE Ep.tDbObjManualSource
--(
--	DbObjId			int not null,
--	UserId			int not null,

--	CreatorId		int not null,
--	CreationDate	datetime2 not null constraint DF_tDbObjManualSource_CreationDate default(GETUTCDATE()),

--	constraint PK_tDbObjManualSource primary key clustered ( DbObjId, UserId ) ,

--	constraint FK_tDbObjManualSource_DbObjId foreign key( DbObjId ) 
--		references Ep.tDbObj( DbObjId ),

--	constraint FK_tDbObjManualSource_UserId foreign key( UserId ) 
--		references Ep.tUser( UserId ),
--	constraint FK_tDbObjManualSource_CreatorId foreign key( CreatorId ) 
--		references Ep.tUser( UserId )
--)

END