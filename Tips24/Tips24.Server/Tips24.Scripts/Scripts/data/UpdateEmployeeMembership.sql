DECLARE @RC int
DECLARE @EmployeeId int
DECLARE @BeginDateTime datetime2(7)
DECLARE @PlaceId int
DECLARE @GroupId int
DECLARE @IsFired bit
DECLARE @IsManager bit
DECLARE @IsOwner bit

SET @EmployeeId = 1001
SET @BeginDateTime = sysdatetime();
SELECT @PlaceId = PlaceId, @GroupId = GroupId, @IsFired = IsFired, @IsManager = IsManager, @IsOwner = IsOwner
	FROM dbo.EmployeeMembershipHistory
	WHERE EmployeeId = @EmployeeId;

SET @IsFired = 0;

EXECUTE @RC = [admin].[UpdateEmployeeMembership] 
   @EmployeeId
  ,@BeginDateTime
  ,@PlaceId
  ,@GroupId
  ,@IsFired
  ,@IsManager
  ,@IsOwner

SELECT * FROM dbo.EmployeeMembershipHistory WHERE EmployeeId = @EmployeeId;
GO


