
EXEC('CREATE SCHEMA [EpTest] AUTHORIZATION [db_owner]')

go
Create procedure EpTest.sModelSp
as begin
	print 'test'
END