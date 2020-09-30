DECLARE @RC int
DECLARE @Phone char(10)
DECLARE @PlaceId int
DECLARE @GroupId int
DECLARE @IsManager bit
DECLARE @IsOwner bit
DECLARE @FirstName nvarchar(50)
DECLARE @LastName nvarchar(50)
DECLARE @Email varchar(50)
DECLARE @PassportNum char(10)
DECLARE @CardNumber varchar(20)
DECLARE @EmployeeId int

SELECT @Phone = '9010000001',
	@PlaceId = 101,
	@GroupId = 2,
	@IsManager = 1,
	@IsOwner = 1,
	@FirstName = 'Kthulhu',
	@LastName = 'Fhtagn',
	@Email = 'mylittlekthulhu@r.lieh.com',
	@PassportNum = '0001000001',
	@CardNumber = NULL

EXECUTE @RC = [admin].[CreateEmployee] 
   @Phone
  ,@PlaceId
  ,@GroupId
  ,@IsManager
  ,@IsOwner
  ,@FirstName
  ,@LastName
  ,@Email
  ,@PassportNum
  ,@CardNumber
  ,@EmployeeId output

SELECT * FROM dbo.Employee WHERE EmployeeId = @EmployeeId;
SELECT * FROM dbo.EmployeeAuth WHERE EmployeeId = @EmployeeId;
SELECT * FROM dbo.EmployeeMembershipHistory WHERE EmployeeId = @EmployeeId;
SELECT * FROM dbo.EmployeePersonalDataHistory WHERE EmployeeId = @EmployeeId;
SELECT * FROM dbo.EmployeeBalanceLog WHERE EmployeeId = @EmployeeId;
GO


