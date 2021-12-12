

CREATE TABLE Ep.tPostalAddress
(
	PostalAddressId		int not null IDENTITY(1,1), 

	CountryId		int not null,
	CONSTRAINT FK_tPostalAddress_CountryId FOREIGN KEY(CountryId)
		REFERENCES Ep.tRepoValue(RepoValueId),

	City		nvarchar(256) not null Constraint DF_tPostalAddress_City default (''),
	PostalCode	nvarchar(256) not null Constraint DF_tPostalAddress_PostalCode default (''),

	Street1		nvarchar(256) not null Constraint DF_tPostalAddress_Street1 default (''),
	Street2		nvarchar(256) not null Constraint DF_tPostalAddress_Street2 default (''),
	
	constraint PK_tPostalAddress primary key clustered ( PostalAddressId )
);



