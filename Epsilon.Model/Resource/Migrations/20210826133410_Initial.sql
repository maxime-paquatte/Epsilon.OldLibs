
EXEC('CREATE SCHEMA [EpRes] AUTHORIZATION [db_owner]')
go

CREATE TABLE EpRes.tRes
(
	ResId		int not null identity( 1,1 ),
	ResName		varchar(512) not null UNIQUE,
	Args		varchar(512) ,

	constraint PK_tRes primary key clustered ( ResId ) 
)

CREATE TABLE EpRes.tResValue
(
	ResId		int not null,
	CultureId	int not null,
	ResValue	varchar(Max) not null,

	constraint PK_tResValue primary key clustered ( ResId, CultureId ) ,
	constraint FK_tResValue_ResId foreign key( ResId ) 
		references EpRes.tRes( ResId ),
	constraint FK_tResValue_CultureId foreign key( CultureId ) 
		references Ep.tCulture( CultureId )
)
