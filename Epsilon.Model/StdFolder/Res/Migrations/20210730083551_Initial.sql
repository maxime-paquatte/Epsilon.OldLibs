

CREATE TABLE Ep.tStdFolder
(
	StdFolderId			int not null identity(1,1),
	ParentId			int null,
	
	FolderType			nvarchar(96) not null,
	StdFolderName		nvarchar(128) not null,

	OwnerId				int not null constraint DF_tStdFolder_OwnerId default( 0 ),	
	CreationDate		datetime2 not null constraint DF_tStdFolder_CreationDate default( GETUTCDATE() ),	
	Claims				nvarchar(128) null,	

	constraint PK_tStdFolder primary key clustered ( StdFolderId ) on [PRIMARY],	
	constraint FK_tStdFolder_OwnerId foreign key ( OwnerId ) references Ep.tUser ( UserId )
);


go


SET IDENTITY_INSERT Ep.tStdFolder ON;
INSERT INTO Ep.tStdFolder (StdFolderId, FolderType, StdFolderName) VALUES(0, '','');
SET IDENTITY_INSERT Ep.tStdFolder OFF;

