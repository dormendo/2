SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [tips_personal];
GO

DROP TABLE dbo.EmployeePersonalData
GO
CREATE TABLE dbo.EmployeePersonalData
(
	EmployeeId int NOT NULL,
	Phone char(10) NOT NULL,
	PinCode char(4) NOT NULL,
	FirstName nvarchar(50) NOT NULL,
	LastName nvarchar(50) NOT NULL
)
GO
ALTER TABLE dbo.EmployeePersonalData ADD CONSTRAINT PK_EmployeePersonalData PRIMARY KEY CLUSTERED 
(
	EmployeeId
)
GO








USE [tips]
GO

DROP TABLE dbo.VerificationCode
GO

CREATE TABLE dbo.VerificationCode
(
	VerificationId uniqueidentifier NOT NULL,
	Code char(6) NOT NULL,
	GenerationDateTime datetime NOT NULL
);
GO
ALTER TABLE dbo.VerificationCode ADD CONSTRAINT
	DF__VerificationCode__GenerationDateTime DEFAULT (getdate()) FOR GenerationDateTime
GO
ALTER TABLE dbo.VerificationCode ADD CONSTRAINT PK_VerificationCode PRIMARY KEY CLUSTERED 
(
	VerificationId
);
GO





DROP INDEX IX_Employee_Phone ON dbo.Employee;
GO
ALTER TABLE dbo.Employee DROP COLUMN Phone, IsVerified;
GO





ALTER TABLE dbo.EmployeeAuth DROP CONSTRAINT PK_EmployeeAuth;
GO
ALTER TABLE dbo.EmployeeAuth ADD CONSTRAINT PK_EmployeeAuth PRIMARY KEY CLUSTERED 
(
	EmployeeId
);
GO
ALTER TABLE dbo.EmployeeAuth DROP COLUMN Phone, PinCode;
GO




CREATE OR ALTER PROCEDURE dbo.Employee_RegisterVerificationCode
	@VerificationId uniqueidentifier,
	@Code char(6)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.VerificationCode (VerificationId, Code, GenerationDateTime) VALUES (@VerificationId, @Code, GETDATE());
	RETURN 0;
END;
GO


CREATE OR ALTER PROCEDURE dbo.Employee_CheckVerificationCode
	@VerificationId uniqueidentifier,
	@Code char(6)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @GenerationDateTime datetime;
	DECLARE @StoredCode char(6);

    BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @GenerationDateTime = GenerationDateTime, @StoredCode = Code FROM dbo.VerificationCode with(xlock) WHERE VerificationId = @VerificationId;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -1;
		END;

		IF (DATEDIFF(minute, @GenerationDateTime, GETDATE()) > 60)
		BEGIN
			DELETE FROM dbo.VerificationCode WHERE VerificationId = @VerificationId;
			COMMIT TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredCode != @Code)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		DELETE FROM dbo.VerificationCode WHERE VerificationId = @VerificationId;
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




CREATE OR ALTER PROCEDURE dbo.Employee_Register
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Phone char(10),
	@PinCode char(4),
	@PlaceId int,
	@PermanentKey char(10)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PlaceId IS NULL OR @FirstName IS NULL OR @LastName IS NULL OR @Phone IS NULL OR LEN(@Phone) != 10 OR @PinCode IS NULL OR LEN(@PinCode) != 4)
		RETURN -1;

	DECLARE @EmployeeId int;

    BEGIN TRY
		BEGIN TRANSACTION;

		IF (EXISTS(SELECT * FROM tips_personal.dbo.EmployeePersonalData with(xlock, serializable) WHERE Phone = @Phone))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		SET @EmployeeId = NEXT VALUE FOR dbo.Seq_Employee;
		INSERT INTO dbo.Employee (EmployeeId, PlaceId, RegisterDateTime, IsDisabled) VALUES (@EmployeeId, @PlaceId, GETDATE(), 0);
		INSERT INTO dbo.EmployeeAuth (EmployeeId, PermanentKey) VALUES (@EmployeeId, @PermanentKey);
		INSERT INTO tips_personal.dbo.EmployeePersonalData (EmployeeId, Phone, PinCode, FirstName, LastName) VALUES (@EmployeeId, @Phone, @PinCode, @FirstName, @LastName);

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