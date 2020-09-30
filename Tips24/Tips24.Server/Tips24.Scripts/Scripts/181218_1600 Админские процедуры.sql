/****** Object:  StoredProcedure [admin].[CreateEmployee]    Script Date: 18.12.2018 19:43:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [admin].[UpdateEmployeeMembership]
	@EmployeeId int,
	@BeginDateTime datetime2(7) = null,
	@PlaceId int,
	@GroupId int,
	@IsFired bit,
	@IsManager bit,
	@IsOwner bit
AS
BEGIN
	SET NOCOUNT ON;

	IF (@BeginDateTime IS NULL)
	BEGIN
		SET @BeginDateTime = sysdatetime();
	END;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @InfinityDateTime datetime2(7) = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999', 120);
		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(nanosecond, -100, @BeginDateTime);
		
		DECLARE @SeqNum int, @NewSeqNum int;
		SELECT @SeqNum = MAX(SeqNum) FROM dbo.EmployeeMembershipHistory with(xlock, serializable) WHERE EmployeeId = @EmployeeId;
		IF (@SeqNum IS NOT NULL)
		BEGIN
			UPDATE dbo.EmployeeMembershipHistory SET EndDateTime = @PrevEndDateTime WHERE EmployeeId = @EmployeeId AND SeqNum = @SeqNum;
		END;

		SET @NewSeqNum = NEXT VALUE FOR dbo.Seq_EmployeeMembershipHistory_SeqNum;
		INSERT dbo.EmployeeMembershipHistory (EmployeeId, SeqNum, PlaceId, GroupId, BeginDateTime, EndDateTime, IsFired, IsManager, IsOwner)
			VALUES (@EmployeeId, @NewSeqNum, @PlaceId, @GroupId, @BeginDateTime, @InfinityDateTime, @IsFired, @IsManager, @IsOwner);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE [admin].[UpdateEmployeePersonalData]
	@EmployeeId int,
	@BeginDateTime datetime2(7) = null,
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Email varchar(50),
	@PassportNum char(10),
	@CardNumber varchar(20) = null
AS
BEGIN
	SET NOCOUNT ON;

	IF (@BeginDateTime IS NULL)
	BEGIN
		SET @BeginDateTime = sysdatetime();
	END;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @InfinityDateTime datetime2(7) = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999', 120);
		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(nanosecond, -100, @BeginDateTime);
		
		DECLARE @SeqNum int, @NewSeqNum int;
		SELECT @SeqNum = MAX(SeqNum) FROM dbo.EmployeePersonalDataHistory with(xlock, serializable) WHERE EmployeeId = @EmployeeId;
		IF (@SeqNum IS NOT NULL)
		BEGIN
			UPDATE dbo.EmployeePersonalDataHistory SET EndDateTime = @PrevEndDateTime WHERE EmployeeId = @EmployeeId AND SeqNum = @SeqNum;
		END;

		SET @NewSeqNum = NEXT VALUE FOR dbo.Seq_EmployeePersonalDataHistory_SeqNum;
		INSERT dbo.EmployeePersonalDataHistory (EmployeeId, SeqNum, BeginDateTime, EndDateTime, FirstName, LastName, Email, PassportNum, CardNumber)
			VALUES (@EmployeeId, @NewSeqNum, @BeginDateTime, @InfinityDateTime, @FirstName, @LastName, @Email, @PassportNum, @CardNumber);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE [admin].[UpdateShareScheme]
	@PlaceId int,
	@BeginDateTime datetime2(7) = null,
	@PersonalShare tinyint,
	@Weight1 tinyint,
	@Weight2 tinyint,
	@Weight3 tinyint,
	@Weight4 tinyint
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		IF (@BeginDateTime IS NULL)
		BEGIN
			SET @BeginDateTime = sysdatetime();
		END;

		DECLARE @InfinityDateTime datetime2(7) = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999', 120);
		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(nanosecond, -100, @BeginDateTime);
		UPDATE dbo.ShareSchemeHistory with (xlock, serializable) SET EndDateTime = @PrevEndDateTime WHERE PlaceId = @PlaceId AND EndDateTime = @InfinityDateTime;

		DECLARE @ShareSchemeHistoryId int = NEXT VALUE FOR dbo.Seq_ShareSchemeHistory;
		INSERT INTO dbo.ShareSchemeHistory (ShareSchemeHistoryId, PlaceId, PersonalShare, BeginDateTime, EndDateTime)
			VALUES (@ShareSchemeHistoryId, @PlaceId, @PersonalShare, @BeginDateTime, @InfinityDateTime);

		INSERT INTO dbo.SsGroupShareHistory (ShareSchemeHistoryId, GroupId, GroupWeight) VALUES (@ShareSchemeHistoryId, 1, @Weight1);
		INSERT INTO dbo.SsGroupShareHistory (ShareSchemeHistoryId, GroupId, GroupWeight) VALUES (@ShareSchemeHistoryId, 2, @Weight2);
		INSERT INTO dbo.SsGroupShareHistory (ShareSchemeHistoryId, GroupId, GroupWeight) VALUES (@ShareSchemeHistoryId, 3, @Weight3);
		INSERT INTO dbo.SsGroupShareHistory (ShareSchemeHistoryId, GroupId, GroupWeight) VALUES (@ShareSchemeHistoryId, 4, @Weight4);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE [admin].[CreateEmployee]
	@Phone char(10),
	@PlaceId int,
	@GroupId int,
	@IsManager bit,
	@IsOwner bit,
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Email varchar(50),
	@PassportNum char(10),
	@CardNumber varchar(20) = null,
	@EmployeeId int output
AS
BEGIN
	SET NOCOUNT ON;

	SET @EmployeeId = NEXT VALUE FOR dbo.Seq_Employee;
	DECLARE @RegisterDateTime datetime2(7) = sysdatetime();

	BEGIN TRANSACTION;
	BEGIN TRY
		INSERT INTO dbo.Employee (EmployeeId, Phone, RegisterDateTime, Balance) VALUES (@EmployeeId, @Phone, @RegisterDateTime, 0);
		INSERT INTO dbo.EmployeeAuth (EmployeeId, PinCode) VALUES (@EmployeeId, '1111');

		EXECUTE admin.UpdateEmployeeMembership @EmployeeId = @EmployeeId, @BeginDateTime = @RegisterDateTime,
			@PlaceId = @PlaceId, @GroupId = @GroupId, @IsFired = 0, @IsManager = @IsManager, @IsOwner = @IsOwner;

		EXECUTE admin.UpdateEmployeePersonalData @EmployeeId = @EmployeeId, @BeginDateTime = @RegisterDateTime,
			@FirstName = @FirstName, @LastName = @LastName, @Email = @Email, @PassportNum = @PassportNum, @CardNumber = @CardNumber;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		SET @EmployeeId = NULL;
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE [admin].[CreatePlace]
	@DisplayName nvarchar(100),
	@Address nvarchar(100),
	@City nvarchar(40),
	@Phone char(10),
	@Inn varchar(20),
	@Email varchar(50),
	@TimeZoneId varchar(32) = 'RTZ2',
	@Info nvarchar(max),
	@PersonalShare tinyint,
	@Weight1 tinyint,
	@Weight2 tinyint,
	@Weight3 tinyint,
	@Weight4 tinyint,
	@PlaceId int output
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BeginDateTime datetime2(7) = sysdatetime();
	SET @PlaceId = NEXT VALUE FOR dbo.Seq_Place;

	BEGIN TRANSACTION;
	BEGIN TRY
		INSERT INTO dbo.Place (PlaceID, RegisterDateTime, DisplayName, Address, City, Phone, Email, TimeZoneId, Info)
			VALUES (@PlaceId, @BeginDateTime, @DisplayName, @Address, @City, @Phone, @Email, @TimeZoneId, @Info);

		EXECUTE admin.UpdateShareScheme @PlaceId = @PlaceId, @BeginDateTime = @BeginDateTime, @PersonalShare = @PersonalShare,
			@Weight1 = @Weight1, @Weight2 = @Weight2, @Weight3 = @Weight3, @Weight4 = @Weight4;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
