CREATE TABLE Ep.tCulture
(
	CultureId		int not null,
	CultureKey		varchar(96),
	DisplayName		varchar(256) ,

	constraint PK_tCulture primary key clustered ( CultureId ) 
)
