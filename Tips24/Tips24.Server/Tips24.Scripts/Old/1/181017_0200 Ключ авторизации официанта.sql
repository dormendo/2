SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [tips]
GO


ALTER TABLE dbo.EmployeeAuth DROP CONSTRAINT DF__EmployeeA__FailedAuthCount;
GO
CREATE TABLE dbo.Tmp_EmployeeAuth
(
	EmployeeId int NOT NULL,
	PermanentKey binary(16) NULL,
	SpecialKey binary(16) NULL,
	SpecialKeyLastAccessDt datetime NULL,
	FailedAuthCount tinyint NOT NULL,
	LastFailedAuthDt datetime NULL
);
GO
ALTER TABLE dbo.Tmp_EmployeeAuth ADD CONSTRAINT DF__EmployeeA__FailedAuthCount DEFAULT ((0)) FOR FailedAuthCount;
GO
IF EXISTS(SELECT * FROM dbo.EmployeeAuth)
	 EXEC('INSERT INTO dbo.Tmp_EmployeeAuth (EmployeeId, PermanentKey, SpecialKey, SpecialKeyLastAccessDt, FailedAuthCount, LastFailedAuthDt)
		SELECT EmployeeId, NULL, NULL, SpecialKeyLastAccessDt, FailedAuthCount, LastFailedAuthDt FROM dbo.EmployeeAuth WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.EmployeeAuth
GO
EXECUTE sp_rename N'dbo.Tmp_EmployeeAuth', N'EmployeeAuth', 'OBJECT' 
GO
ALTER TABLE dbo.EmployeeAuth ADD CONSTRAINT PK_EmployeeAuth PRIMARY KEY CLUSTERED 
(
	EmployeeId
);
GO
CREATE NONCLUSTERED INDEX IX_EmployeeAuth_EmployeeId ON dbo.EmployeeAuth
(
	EmployeeId
);
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_EmployeeAuth_PermanentKey ON dbo.EmployeeAuth
(
	PermanentKey
)
WHERE ([PermanentKey] IS NOT NULL);
GO





ALTER PROCEDURE [dbo].[Employee_CheckEmployeeStatus]
	@PermanentKey binary(16),
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

	IF (@PermanentKey IS NULL)
		RETURN -1;

	DECLARE @StoredPlaceId int;

	SELECT @EmployeeId = e.EmployeeId, @StoredPlaceId = e.PlaceId, @EmployeeIsDisabled = e.IsDisabled, @PlaceGroupId = e.GroupId, @PlaceGroupName = g.Name
		FROM dbo.EmployeeAuth a
		INNER JOIN dbo.Employee e ON e.EmployeeId = e.EmployeeId
		LEFT JOIN dbo.PlaceGroup g ON e.GroupId = g.GroupId
		WHERE a.PermanentKey = @PermanentKey;
		
	IF (@@ROWCOUNT = 0)
		RETURN -2;

	IF (@StoredPlaceId != @PlaceId)
		RETURN -3;

	SELECT @EmployeeFirstName = FirstName, @EmployeeLastName = LastName FROM tips_personal.dbo.EmployeePersonalData WHERE EmployeeId = @EmployeeId;
	IF (@@ROWCOUNT = 0)
		RETURN -4;
END;
GO




ALTER PROCEDURE [dbo].[Employee_EnterSecuredSession]
	@PermanentKey binary(16),
	@SecuredKey binary(16),
	@Phone char(10),
	@PinCode char(4)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@Phone IS NULL OR @PinCode IS NULL OR @PermanentKey IS NULL OR @SecuredKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeIdByPermanentKey int;
	DECLARE @EmployeeIdByPersonalData int;
	DECLARE @StoredPinCode char(4);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeIdByPermanentKey = EmployeeId FROM dbo.EmployeeAuth with(updlock) WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		SELECT @EmployeeIdByPersonalData = EmployeeId, @StoredPinCode = PinCode FROM tips_personal.dbo.EmployeePersonalData WHERE Phone = @Phone;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (@PinCode != @StoredPinCode)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END;

		IF (@EmployeeIdByPermanentKey != @EmployeeIdByPersonalData)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -5;
		END;

		UPDATE dbo.EmployeeAuth SET
				SpecialKey = @SecuredKey,
				SpecialKeyLastAccessDt = GETDATE(),
				FailedAuthCount = 0,
				LastFailedAuthDt = NULL
			WHERE EmployeeId = @EmployeeIdByPermanentKey;

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




ALTER PROCEDURE [dbo].[Employee_ExitSecuredSession]
	@PermanentKey binary(16),
	@SecuredKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL OR @SecuredKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredSecuredKey binary(16);
	DECLARE @SecuredKeyLastAccessDt datetime;
	DECLARE @StoredPinCode char(4);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId, @StoredSecuredKey = SpecialKey
			FROM dbo.EmployeeAuth with(updlock)
			WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredSecuredKey IS NULL OR @StoredSecuredKey != @SecuredKey)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		UPDATE dbo.EmployeeAuth SET
				SpecialKey = NULL,
				SpecialKeyLastAccessDt = NULL,
				FailedAuthCount = 0,
				LastFailedAuthDt = NULL
			WHERE EmployeeId = @EmployeeId;

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




ALTER PROCEDURE [dbo].[Employee_FollowRegistrationLink]
	@LinkParameter uniqueidentifier,
	@PermanentKey binary(16),
	@LinkPlaceId int output,
	@LinkPlaceName nvarchar(100) output,
	@LinkPlaceAddress nvarchar(100) output,
	@LinkPlaceCity nvarchar(40) output,
	@EmployeeId int output,
	@EmployeePlaceId int output,
	@EmployeeIsDisabled bit output
AS
BEGIN
	SET NOCOUNT ON;
	
	IF (@LinkParameter IS NULL OR @PermanentKey IS NULL)
		RETURN -1;

	DECLARE @LinkCreateDateTime datetime;

	SELECT @LinkPlaceId = PlaceId, @LinkCreateDateTime = CreateDateTime FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
	IF (@@ROWCOUNT = 0)
		RETURN -2;

	IF (DATEDIFF(minute, @LinkCreateDateTime, GETDATE()) > 60 * 24)
		RETURN -3;

	IF (@PermanentKey IS NOT NULL)
	BEGIN
		SELECT @EmployeeId = EmployeeId FROM dbo.EmployeeAuth WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
			RETURN -4;
	END;

	SELECT @LinkPlaceName = DisplayName, @LinkPlaceAddress = Address, @LinkPlaceCity = City FROM dbo.Place WHERE PlaceId = @LinkPlaceId;

	IF (@EmployeeId IS NOT NULL)
		SELECT @EmployeePlaceId = PlaceId, @EmployeeIsDisabled = IsDisabled FROM dbo.Employee WHERE EmployeeId = @EmployeeId;

	RETURN 0;
END;
GO




ALTER PROCEDURE [dbo].[Employee_JoinPlace]
	@PermanentKey binary(16),
	@LinkParameter uniqueidentifier,
	@PlaceId int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF (@LinkParameter IS NULL OR @PermanentKey IS NULL)
		RETURN -1;

	DECLARE @StoredPlaceId int;
	DECLARE @EmployeeId int;
	DECLARE @OldPlaceId int;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @StoredPlaceId = PlaceId FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredPlaceId != @PlaceId)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (NOT EXISTS(SELECT * FROM dbo.Place WHERE PlaceId = @PlaceId))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END

		SELECT @EmployeeId = e.EmployeeId, @OldPlaceId = e.PlaceId
			FROM dbo.EmployeeAuth a
			INNER JOIN dbo.Employee e with(updlock) ON e.EmployeeId = e.EmployeeId
			WHERE a.PermanentKey = @PermanentKey;
		
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -5;
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




ALTER PROCEDURE [dbo].[Employee_KeepSecuredSessionAlive]
	@PermanentKey binary(16),
	@SecuredKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL OR @SecuredKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredSecuredKey binary(16);
	DECLARE @SecuredKeyLastAccessDt datetime;
	DECLARE @StoredPinCode char(4);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId, @StoredSecuredKey = SpecialKey, @SecuredKeyLastAccessDt = SpecialKeyLastAccessDt
			FROM dbo.EmployeeAuth with(updlock)
			WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredSecuredKey IS NULL OR @StoredSecuredKey != @SecuredKey)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (DATEDIFF(minute, @SecuredKeyLastAccessDt, GETDATE()) > 10)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END;

		UPDATE dbo.EmployeeAuth SET SpecialKeyLastAccessDt = GETDATE() WHERE EmployeeId = @EmployeeId;

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




ALTER PROCEDURE [dbo].[Employee_Login]
	@Phone char(10),
	@PinCode char(4),
	@PermanentKey binary(16) output
AS
BEGIN
	SET NOCOUNT ON;

	IF (@Phone IS NULL OR @PinCode IS NULL)
		RETURN -1;

	DECLARE @StoredPinCode char(4);
	DECLARE @EmployeeId int;

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




ALTER PROCEDURE [dbo].[Employee_Logout]
	@PermanentKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId FROM dbo.EmployeeAuth with(updlock) WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		UPDATE dbo.EmployeeAuth SET
				PermanentKey = NULL,
				SpecialKey = NULL,
				SpecialKeyLastAccessDt = NULL,
				FailedAuthCount = 0,
				LastFailedAuthDt = NULL
			WHERE EmployeeId = @EmployeeId;

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




ALTER PROCEDURE [dbo].[Employee_Register]
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Phone char(10),
	@PinCode char(4),
	@LinkParameter uniqueidentifier,
	@PlaceId int,
	@PermanentKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PlaceId IS NULL OR @FirstName IS NULL OR @LastName IS NULL OR @Phone IS NULL OR @PinCode IS NULL OR @LinkParameter IS NULL OR @PermanentKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredPlaceId int;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @StoredPlaceId = PlaceId FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredPlaceId != @PlaceId)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (NOT EXISTS(SELECT * FROM dbo.Place WHERE PlaceId = @PlaceId))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END

		IF (EXISTS(SELECT * FROM tips_personal.dbo.EmployeePersonalData with(xlock, serializable) WHERE Phone = @Phone))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -5;
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




ALTER PROCEDURE [dbo].[Employee_RegisterVerificationCode]
	@VerificationId uniqueidentifier,
	@Code char(6)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@VerificationId IS NULL OR @Code IS NULL)
		RETURN -1;

	INSERT INTO dbo.VerificationCode (VerificationId, Code, GenerationDateTime) VALUES (@VerificationId, @Code, GETDATE());
	RETURN 0;
END;
