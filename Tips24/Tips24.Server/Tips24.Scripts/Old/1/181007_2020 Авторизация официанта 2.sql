SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_EmployeeAuth_PermanentKey ON dbo.EmployeeAuth (PermanentKey) WHERE PermanentKey IS NOT NULL;
GO

CREATE OR ALTER PROCEDURE dbo.Employee_Logout
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

		UPDATE dbo.EmployeeAuth SET PermanentKey = NULL WHERE PermanentKey = @PermanentKey;

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




CREATE OR ALTER PROCEDURE dbo.Employee_CheckBeforeRegistration
	@PermanentKey char(10),
	@PlaceId int output,
	@IsDisabled bit output
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @EmployeeId int;

    BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId FROM dbo.EmployeeAuth WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -1;
		END;

		SELECT @PlaceId = PlaceId, @IsDisabled = IsDisabled FROM dbo.Employee WHERE EmployeeId = @EmployeeId;

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
