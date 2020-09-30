SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [tips]
GO


DROP INDEX [IX_EmployeeRegistrationLink_PlaceId] ON [dbo].[EmployeeRegistrationLink]
GO

CREATE NONCLUSTERED INDEX [IX_EmployeeRegistrationLink_PlaceId] ON [dbo].[EmployeeRegistrationLink]
(
	[PlaceId] ASC,
	[CreateDateTime] ASC
)
GO





ALTER PROCEDURE [dbo].[Employee_CheckVerificationCode]
	@VerificationId uniqueidentifier,
	@Code char(6)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@VerificationId IS NULL OR @Code IS NULL)
		RETURN -1;

	DECLARE @GenerationDateTime datetime;
	DECLARE @StoredCode char(6);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @GenerationDateTime = GenerationDateTime, @StoredCode = Code FROM dbo.VerificationCode with(xlock) WHERE VerificationId = @VerificationId;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (DATEDIFF(minute, @GenerationDateTime, GETDATE()) > 60)
		BEGIN
			DELETE FROM dbo.VerificationCode WHERE VerificationId = @VerificationId;
			COMMIT TRANSACTION;
			RETURN -3;
		END;

		IF (@StoredCode != @Code)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
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



SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[Employee_Register]
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Phone char(10),
	@PinCode char(4),
	@LinkParameter uniqueidentifier,
	@PlaceId int,
	@PermanentKey char(10)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PlaceId IS NULL OR @FirstName IS NULL OR @LastName IS NULL OR @Phone IS NULL OR LEN(@Phone) != 10 OR @PinCode IS NULL OR LEN(@PinCode) != 4)
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





ALTER   PROCEDURE [dbo].[Employee_JoinPlace]
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

		IF (NOT EXISTS(SELECT * FROM dbo.Place WHERE PlaceId = @PlaceId))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END

		SELECT @EmployeeId = e.EmployeeId, @OldPlaceId = e.PlaceId
			FROM dbo.EmployeeAuth a
			INNER JOIN dbo.Employee e with(updlock) ON e.EmployeeId = e.EmployeeId
			WHERE a.PermanentKey = @PermanentKey;
		
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
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




ALTER   PROCEDURE [dbo].[Employee_Logout]
	@PermanentKey char(10)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @EmployeeId int;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId FROM dbo.EmployeeAuth with(updlock) WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -1;
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




CREATE OR ALTER PROCEDURE [dbo].[Employee_EnterSecuredSession]
	@PermanentKey char(10),
	@SecuredKey char(10),
	@Phone char(10),
	@PinCode char(4)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@Phone IS NULL OR LEN(@Phone) != 10 OR @PinCode IS NULL OR LEN(@PinCode) != 4 OR
		@PermanentKey IS NULL OR LEN(@PermanentKey) != 10 OR @SecuredKey IS NULL OR LEN(@SecuredKey) != 10)
	BEGIN
		RETURN -1;
	END;

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





CREATE OR ALTER PROCEDURE [dbo].[Employee_KeepSecuredSessionAlive]
	@PermanentKey char(10),
	@SecuredKey char(10)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL OR LEN(@PermanentKey) != 10 OR @SecuredKey IS NULL OR LEN(@SecuredKey) != 10)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredSecuredKey int;
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






CREATE OR ALTER PROCEDURE [dbo].[Employee_ExitSecuredSession]
	@PermanentKey char(10),
	@SecuredKey char(10)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL OR LEN(@PermanentKey) != 10 OR @SecuredKey IS NULL OR LEN(@SecuredKey) != 10)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredSecuredKey int;
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

