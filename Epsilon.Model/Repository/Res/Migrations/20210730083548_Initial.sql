

CREATE TABLE Ep.tRepo
(
	RepoId			int not null identity( 1,1 ),
	SystemKey		varchar(96)	not null constraint DF_tRepo_SystemKey default '',
	Name			varchar(96)	not null constraint DF_tRepo_Name default '',
	ShowDate		bit	not null constraint DF_tRepo_ShowDate default 0,

	Locked			bit	not null constraint DF_tRepo_Locked default 0,
	Required		bit	not null constraint DF_tRepo_Required default 0,
	OrderByValue	bit	not null constraint DF_tRepo_OrderByValue default 0,
	Sortable	bit	not null constraint DF_tRepo_Sortable default 0,

	AlphaId	int null,

	constraint PK_tRepo primary key clustered ( RepoId ) 

)
go
CREATE TABLE Ep.tRepoValue
(
	RepoValueId		int not null identity( 1,1 ),
	RepoId			int not null,

	Name			varchar(255) not null constraint DF_tRepoValue_Name default '',

	SystemKey		varchar(255) not null constraint DF_tRepoValue_SystemKey default '',
	OldPlatformId	int null,
	AlphaId	int null,

	DoNotUseIt		bit not null constraint DF_tRepoValue_DoNotUseIt default 0,
		
	constraint PK_tRepoValue primary key clustered ( RepoValueId ) ,
	constraint FK_tRepoValue_RepoId foreign key( RepoId ) 
		references Ep.tRepo( RepoId ),

	constraint UK_tRepoValue_Name UNIQUE (RepoId, Name)

)



--Usefull for Ep.sRepoValueComputeSystemKey
CREATE INDEX IX_tRepoValue_Name ON Ep.tRepoValue (Name)
	INCLUDE (RepoValueId, SystemKey);

go

CREATE TABLE Ep.tRepoValueRes
(
	RepoValueId		int not null,
	CultureId	int not null,

	Content			varchar(MAX) not null constraint DF_tRepoValueRes_Content default '',
	Value			varchar(255) not null constraint DF_tRepoValueRes_Value default '',
	Path			varchar(MAX) not null constraint DF_tRepoValueRes_Path default '',

	constraint PK_tRepoValueRes primary key clustered ( RepoValueId, CultureId ) ,
	constraint FK_tRepoValueRes_RepoValueId foreign key( RepoValueId ) 
		references Ep.tRepoValue( RepoValueId ),
	constraint FK_tRepoValueRes_CultureId foreign key( CultureId ) 
		references Ep.tCulture( CultureId )
)

go

CREATE TABLE Ep.tRepoValueComputedSystemKey
(
	RepoValueId		int not null,
	SystemKey		varchar(255) not null constraint DF_tRepoValueComputedSystemKey_SystemKey default '',
		
	constraint PK_tRepoValueComputedSystemKey primary key clustered ( RepoValueId ) ,
	constraint FK_tRepoValueComputedSystemKey_RepoValueId foreign key( RepoValueId ) 
		references Ep.tRepoValue( RepoValueId )
)


go

CREATE TABLE Ep.tRepoValueBinding
(
	RepoValueSourceId		int not null,
	RepoValueTargetId		int not null,
	
	BothWay					bit not null
		constraint DF_tRepoValueBinding_BothWay default 1,
	Weight					int not null
		constraint DF_tRepoValueBinding_Weight default 1,
		
		
	constraint PK_tRepoValueBinding primary key clustered ( RepoValueSourceId, RepoValueTargetId ) ,
	constraint FK_tRepoValueBinding_RepoValueSourceId foreign key( RepoValueSourceId ) 
		references Ep.tRepoValue( RepoValueId ),
	constraint FK_tRepoValueBinding_RepoValueTargetId foreign key( RepoValueTargetId ) 
		references Ep.tRepoValue( RepoValueId )
)


go

CREATE TABLE Ep.tDbObjRepoValue
(
	DbObjId			int not null,
	RepoValueId		int not null,
	DateAdded		datetime2 not null constraint DF_tDbObjRepoValue_DateAdded default(GETUTCDATE()),
	Idx				tinyint not null constraint DF_tDbObjRepoValue_Idx default(0),

	timestamp

	constraint PK_tDbObjRepoValue primary key clustered ( DbObjId, RepoValueId ) ,
	constraint FK_tDbObjRepoValue_DbObjId foreign key( DbObjId ) 
		references Ep.tDbObj( DbObjId ),
	constraint FK_tDbObjRepoValue_RepoValueId foreign key( RepoValueId ) 
		references Ep.tRepoValue( RepoValueId )
)
go

CREATE TABLE Ep.tRepoDbObjType
(
	RepoId			int not null,
	DbObjTypeId		int not null

	constraint PK_tRepoDbObjType primary key clustered ( RepoId, DbObjTypeId ) ,
	constraint FK_tRepoDbObjType_RepoId foreign key( RepoId ) 
		references Ep.tRepo( RepoId ),
	constraint FK_tRepoDbObjType_DbObjTypeId foreign key( DbObjTypeId ) 
		references Ep.tDbObjType( DbObjTypeId )
)

