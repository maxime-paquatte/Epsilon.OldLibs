
CREATE TABLE Ep.tSharedDic
(
	[Key] nvarchar(255) not null,
	Value nvarchar(MAX) not null,
	CreationDate datetime2(0) not null
		constraint DF_tSharedDic_CreationDate default( GETUTCDATE()),
	LastWriteDate datetime2(0) not null
		constraint DF_tSharedDic_LastWriteDate default( GETUTCDATE()),
	LastAccessDate datetime2(0) not null
		constraint DF_tSharedDic_LastAccessDate default( GETUTCDATE()),
	
	DateRef smallint not null, --0:creation, 1:write, 2:read, else Infinit
	RelativeExpirationSecond int not null,
	
	Expired as
	(
		CASE 
		WHEN DateRef = 0 AND GETUTCDATE() > DATEADD(second, RelativeExpirationSecond, CreationDate) THEN 1
		WHEN DateRef = 1 AND GETUTCDATE() > DATEADD(second, RelativeExpirationSecond, LastWriteDate) THEN 1
		WHEN DateRef = 2 AND GETUTCDATE() > DATEADD(second, RelativeExpirationSecond, LastAccessDate) THEN 1
		ELSE 0 END
	),	
		
	CONSTRAINT PK_tSharedDic PRIMARY KEY ([Key])
)

go

CREATE PROCEDURE Ep.sSharedDicGet 
	-- Add the parameters for the stored procedure here
	@Key nvarchar(255),
	@Value nvarchar(MAX) output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
    delete from Ep.tSharedDic where Expired = 1
    
	update Ep.tSharedDic
	set LastAccessDate = GETUTCDATE()
	where [Key] = @Key and Expired = 0
    
	SELECT top 1 @Value = Value 
	from Ep.tSharedDic 
	where [Key] = @Key and Expired = 0
END

go

CREATE PROCEDURE Ep.sSharedDicCreateOrUpdate
	-- Add the parameters for the stored procedure here
	@Key nvarchar(255),
	@Value nvarchar(MAX) ,
	@DateRef smallint,
	@RelativeExpirationSecond int
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    
    IF  EXISTS ( select * from Ep.tSharedDic where [Key] = @Key)
		update Ep.tSharedDic 
		set Value = @Value,LastWriteDate = GETUTCDATE(), LastAccessDate = GETUTCDATE()
		where [Key] = @Key
	else
		insert into Ep.tSharedDic ([Key], Value, DateRef, RelativeExpirationSecond)
		values (@Key, @Value, @DateRef, @RelativeExpirationSecond)	
		
		delete from Ep.tSharedDic where Expired = 1	
	
END

go
CREATE PROCEDURE Ep.sSharedDicCreate
	-- Add the parameters for the stored procedure here
	@Key nvarchar(255),
	@Value nvarchar(MAX),
	@DateRef smallint,
	@RelativeExpirationSecond int
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    
	insert into Ep.tSharedDic ([Key], Value, DateRef, RelativeExpirationSecond)
	values (@Key, @Value, @DateRef, @RelativeExpirationSecond)	
	
	delete from tSharedDic where Expired = 1
END

go
CREATE PROCEDURE Ep.sSharedDicUpdate
	-- Add the parameters for the stored procedure here
	@Key nvarchar(255),
	@Value nvarchar(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    
	update Ep.tSharedDic
	set 
	Value = @Value,
	LastWriteDate = GETUTCDATE(),
	LastAccessDate = GETUTCDATE()
	where [Key] = @Key and Expired = 0
	
	delete from Ep.tSharedDic where Expired = 1
END

go
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[Neva].[tSharedDic]') AND type in (N'U'))
BEGIN

	insert into Ep.tSharedDic ([Key], Value, CreationDate, LastWriteDate, LastAccessDate, DateRef, RelativeExpirationSecond)
	select [Key], Value, CreationDate, LastWriteDate, LastAccessDate, DateRef, RelativeExpirationSecond from Neva.tSharedDic

	drop procedure [Neva].sSharedDicGet 
	drop procedure [Neva].sSharedDicCreateOrUpdate 
	drop procedure [Neva].sSharedDicCreate 
	drop procedure [Neva].sSharedDicUpdate 
	drop table [Neva].[tSharedDic]

END