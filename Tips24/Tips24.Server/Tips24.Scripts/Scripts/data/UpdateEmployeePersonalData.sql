DECLARE @RC int
DECLARE @EmployeeId int
DECLARE @BeginDateTime datetime2(7)
DECLARE @FirstName nvarchar(50)
DECLARE @LastName nvarchar(50)
DECLARE @Email varchar(50)
DECLARE @PassportNum char(10)
DECLARE @CardNumber varchar(20)

SET @EmployeeId = 1001
SET @BeginDateTime = sysdatetime();
SELECT @FirstName = FirstName, @LastName = LastName, @Email = Email, @PassportNum = PassportNum, @CardNumber = CardNumber
	FROM dbo.EmployeePersonalDataHistory
	WHERE EmployeeId = @EmployeeId;

SET @CardNumber = '0004000300020001'

EXECUTE @RC = [admin].[UpdateEmployeePersonalData] 
   @EmployeeId
  ,@BeginDateTime
  ,@FirstName
  ,@LastName
  ,@Email
  ,@PassportNum
  ,@CardNumber

SELECT * FROM dbo.EmployeePersonalDataHistory WHERE EmployeeId = @EmployeeId;
GO


