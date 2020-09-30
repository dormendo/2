SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE SCHEMA [acquiring];
GO

CREATE PROCEDURE acquiring.GetReceiverData
	@PlaceId int,
	@EmployeeId int
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @curdt datetime2(7) = sysdatetime();
	
	BEGIN TRANSACTION;
	BEGIN TRY
		IF (NOT EXISTS(SELECT * FROM dbo.Place WHERE PlaceId = @PlaceId))
		BEGIN
			IF @@TRANCOUNT > 0
				ROLLBACK TRANSACTION;
			RETURN -1;
		END;

		IF (NOT EXISTS(SELECT * FROM dbo.PlaceActivityHistory WHERE PlaceId = @PlaceId AND @curdt BETWEEN BeginDateTime AND EndDateTime))
		BEGIN
			IF @@TRANCOUNT > 0
				ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@EmployeeId IS NOT NULL)
		BEGIN
			IF (NOT EXISTS(SELECT * FROM dbo.Employee WHERE EmployeeId = @EmployeeId))
			BEGIN
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
				RETURN -3;
			END;

			IF (NOT EXISTS(SELECT * FROM dbo.EmployeeMembershipHistory WHERE EmployeeId = @EmployeeId AND IsFired = 0 AND @curdt BETWEEN BeginDateTime AND EndDateTime))
			BEGIN
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
				RETURN -4;
			END;

			IF (NOT EXISTS(SELECT * FROM dbo.EmployeePersonalDataHistory WHERE EmployeeId = @EmployeeId AND @curdt BETWEEN BeginDateTime AND EndDateTime))
			BEGIN
				IF @@TRANCOUNT > 0
					ROLLBACK TRANSACTION;
				RETURN -5;
			END;
		END;

		SELECT DisplayName, Address, City FROM dbo.Place WHERE PlaceId = @PlaceId;

		IF (@EmployeeId IS NOT NULL)
		BEGIN
			SELECT FirstName, LastName FROM dbo.EmployeePersonalDataHistory WHERE EmployeeId = @EmployeeId AND @curdt BETWEEN BeginDateTime AND EndDateTime;
		END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO




CREATE SEQUENCE [acquiring].[Seq_Request] AS [int] START WITH 1 INCREMENT BY 1 MINVALUE 1;
GO



CREATE TABLE acquiring.Request
(
	RequestId int NOT NULL CONSTRAINT DF__acquiring_Request__RequestId DEFAULT NEXT VALUE FOR acquiring.Seq_Request,
	Status tinyint NOT NULL,
	PlaceId int NOT NULL,
	EmployeeId int NULL,
	Amount decimal(18,2) NOT NULL,
	IpAddress varchar(15) NULL,
	CONSTRAINT PK_acquiring_Request PRIMARY KEY CLUSTERED (RequestId)
);
GO


CREATE PROCEDURE acquiring.CreateRequest
	@PlaceId int,
	@EmployeeId int,
	@Amount decimal(18,2),
	@IpAddress varchar(15),
	@RequestId int output
AS
BEGIN
	SET @RequestId = NEXT VALUE FOR acquiring.Seq_Request;
	INSERT INTO acquiring.Request(RequestId, Status, PlaceId, EmployeeId, Amount, IpAddress)
		VALUES (@RequestId, 0, @PlaceId, @EmployeeId, @Amount, @IpAddress);
END;
GO
