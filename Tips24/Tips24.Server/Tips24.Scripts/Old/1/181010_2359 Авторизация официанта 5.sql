SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

DROP TABLE dbo.EmployeeInGroup;
GO

ALTER TABLE dbo.Employee DROP CONSTRAINT DF__Employee__EmployeeId;
GO
ALTER TABLE dbo.Employee DROP CONSTRAINT DF__Employee__RegisterDate;
GO
ALTER TABLE dbo.Employee DROP CONSTRAINT DF__Employee__IsDisabled;
GO
CREATE TABLE dbo.Tmp_Employee
(
	EmployeeId int NOT NULL,
	PlaceId int NULL,
	GroupId int NULL,
	RegisterDateTime datetime NOT NULL,
	IsDisabled bit NOT NULL
);
GO
ALTER TABLE dbo.Tmp_Employee ADD CONSTRAINT DF__Employee__EmployeeId DEFAULT (NEXT VALUE FOR [dbo].[Seq_Employee]) FOR EmployeeId;
GO
ALTER TABLE dbo.Tmp_Employee ADD CONSTRAINT DF__Employee__RegisterDate DEFAULT (getdate()) FOR RegisterDateTime;
GO
ALTER TABLE dbo.Tmp_Employee ADD CONSTRAINT DF__Employee__IsDisabled DEFAULT ((0)) FOR IsDisabled;
GO
IF EXISTS(SELECT * FROM dbo.Employee)
	 EXEC('INSERT INTO dbo.Tmp_Employee (EmployeeId, PlaceId, RegisterDateTime, IsDisabled)
		SELECT EmployeeId, PlaceId, RegisterDateTime, IsDisabled FROM dbo.Employee WITH (HOLDLOCK TABLOCKX)');
GO
DROP TABLE dbo.Employee;
GO
EXECUTE sp_rename N'dbo.Tmp_Employee', N'Employee', 'OBJECT';
GO
ALTER TABLE dbo.Employee ADD CONSTRAINT PK_Employee PRIMARY KEY CLUSTERED 
(
	EmployeeId
);
GO
CREATE NONCLUSTERED INDEX IX_Employee_PlaceId ON dbo.Employee
(
	PlaceId,
	IsDisabled
);
GO







CREATE OR ALTER PROCEDURE dbo.Employee_Login
	@Phone char(10),
	@PinCode char(4),
	@PermanentKey char(10) output
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @StoredPinCode char(4);
	DECLARE @EmployeeId int;

	IF (@Phone IS NULL OR LEN(@Phone) != 10 OR @PinCode IS NULL OR LEN(@PinCode) != 4)
		RETURN -1;

	SELECT @EmployeeId = EmployeeId, @StoredPinCode = PinCode FROM tips_personal.dbo.EmployeePersonalData WHERE Phone = @Phone;
	IF (@@ROWCOUNT = 0)
		RETURN -2;

	IF (@PinCode != @StoredPinCode)
		RETURN -3;

	SELECT @PermanentKey = PermanentKey FROM dbo.EmployeeAuth WHERE EmployeeId = @EmployeeId;
	IF (@@ROWCOUNT = 0)
		RETURN -4;

	RETURN 0;
END;
GO


CREATE OR ALTER PROCEDURE dbo.Employee_JoinPlace
	@PermanentKey char(10),
	@LinkParameter uniqueidentifier,
	@PlaceId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @StoredPlaceId int;
	DECLARE @EmployeeId int;
	DECLARE @OldPlaceId int;

    BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @StoredPlaceId = PlaceId FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -1;
		END;

		IF (@StoredPlaceId != @PlaceId)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		SELECT @EmployeeId = e.EmployeeId, @OldPlaceId = e.PlaceId
			FROM dbo.EmployeeAuth a
			INNER JOIN dbo.Employee e with(updlock) ON e.EmployeeId = e.EmployeeId
			WHERE a.PermanentKey = @PermanentKey;
		
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		UPDATE dbo.Employee SET PlaceId = @PlaceId, GroupId = NULL, IsDisabled = 0 WHERE EmployeeId = @EmployeeId;
		COMMIT TRANSACTION;

		RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO





CREATE OR ALTER PROCEDURE dbo.Employee_CheckEmployeeStatus
	@PermanentKey char(10),
	@PlaceId int,
	@EmployeeId int output,
	@EmployeeFirstName nvarchar(50) output,
	@EmployeeLastName nvarchar(50) output,
	@EmployeeIsDisabled bit output,
	@PlaceGroupId int output,
	@PlaceGroupName nvarchar(50) output
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @StoredPlaceId int;

	SELECT @EmployeeId = e.EmployeeId, @StoredPlaceId = e.PlaceId, @EmployeeIsDisabled = e.IsDisabled, @PlaceGroupId = e.GroupId, @PlaceGroupName = g.Name
		FROM dbo.EmployeeAuth a
		INNER JOIN dbo.Employee e ON e.EmployeeId = e.EmployeeId
		LEFT JOIN dbo.PlaceGroup g ON e.GroupId = g.GroupId
		WHERE a.PermanentKey = @PermanentKey;
		
	IF (@@ROWCOUNT = 0)
		RETURN -1;

	IF (@StoredPlaceId != @PlaceId)
		RETURN -2;

	SELECT @EmployeeFirstName = FirstName, @EmployeeLastName = LastName FROM tips_personal.dbo.EmployeePersonalData WHERE EmployeeId = @EmployeeId;
	IF (@@ROWCOUNT = 0)
		RETURN -3;
END;
GO
