SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE SEQUENCE [admin].[Seq_DiagMessage] AS [bigint] START WITH 1 INCREMENT BY 1 MINVALUE 1;
GO


CREATE TABLE admin.DiagMessage
(
	MessageId bigint NOT NULL CONSTRAINT PK_admin_DiagMessage PRIMARY KEY CLUSTERED CONSTRAINT DF__admin_DiagMessage__MessageId DEFAULT NEXT VALUE FOR admin.Seq_DiagMessage,
	CreateDateTime datetime2(7) NOT NULL CONSTRAINT DF__admin_DiagMessage__CreateDateTime DEFAULT sysdatetime(),
	Options int NOT NULL,
	Message nvarchar(max) NOT NULL
);
GO

ALTER PROCEDURE [admin].[DiagMessage_Save]
	@Options int,
	@Message nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO admin.DiagMessage (Options, Message) VALUES (@Options, @Message);
END;
GO

CREATE TABLE telegram.DiagBotUser
(
	TelegramId bigint NOT NULL PRIMARY KEY CLUSTERED,
	Options int NOT NULL
);
GO

CREATE PROCEDURE telegram.Diag_Notifications_Proceed
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #t1
	(
		Id bigint NOT NULL PRIMARY KEY CLUSTERED
	);
	
	INSERT INTO #t1 (Id)
	SELECT MessageId FROM admin.DiagMessage ORDER BY MessageId;

	IF @@ROWCOUNT > 0
	BEGIN
		SELECT TelegramId, Options FROM telegram.DiagBotUser WHERE Options > 0;
		SELECT MessageId, CreateDateTime, Options, Message FROM admin.DiagMessage WHERE MessageId IN (SELECT Id FROM #t1);
		DELETE FROM admin.DiagMessage WHERE MessageId IN (SELECT Id FROM #t1);
	END;
END;
GO


ALTER PROCEDURE [telegram].[Notifications_Proceed]
AS
BEGIN
	CREATE TABLE #t1
	(
		Id bigint NOT NULL PRIMARY KEY CLUSTERED
	);
	
	INSERT INTO #t1 (Id)
	SELECT TOP(20) NotificationId
		FROM telegram.Notification
		WHERE TelegramId NOT IN (SELECT TelegramId FROM telegram.DialogSession)
		ORDER BY NotificationId;

	SELECT TelegramId, CreateDateTime, Message FROM telegram.Notification WHERE NotificationId IN (SELECT Id FROM #t1);

	DELETE FROM telegram.Notification WHERE NotificationId IN (SELECT Id FROM #t1);
END;
GO


ALTER TABLE telegram.BotSettings ADD
	DiagBotOffset int NOT NULL CONSTRAINT DF_BotSettings_DiagBotOffset DEFAULT 0
GO

CREATE PROCEDURE [telegram].[Diag_LoadSettings]
AS
BEGIN
	SELECT DiagBotOffset FROM telegram.BotSettings;
END;
GO

CREATE PROCEDURE [telegram].[Diag_UpdateBotOffset]
	@DiagBotOffset int
AS
BEGIN
	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE telegram.BotSettings SET DiagBotOffset = @DiagBotOffset;
		IF (@@ROWCOUNT = 0)
		BEGIN
			INSERT INTO telegram.BotSettings (DiagBotOffset) VALUES (@DiagBotOffset);
		END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO

INSERT INTO telegram.DiagBotUser(TelegramId, Options) VALUES (260481660, 1 | 2);
INSERT INTO telegram.DiagBotUser(TelegramId, Options) VALUES (20087980, 1);
GO


ALTER PROCEDURE [acquiring].[SucceedRequest]
	@RequestId int,
	@Type tinyint output,
	@PlaceId int output,
	@EmployeeId int output,
	@Amount decimal(18,2) output
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @Status tinyint;
		SELECT @Status = Status, @Type = Type, @PlaceId = PlaceId, @EmployeeId = EmployeeId, @Amount = Amount
			FROM acquiring.Request with (updlock, rowlock)
			WHERE RequestId = @RequestId;

		IF @@ROWCOUNT = 0
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -1;
		END;


		IF @Status NOT IN (1, 2)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN 0;
		END;
		
		UPDATE acquiring.Request SET Status = 3 WHERE RequestId = @RequestId;
		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 3);

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




CREATE PROCEDURE [payment].[SbAcquiring_FailedToCompleteRequest]
	@RequestId int,
	@OrderId varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE acquiring.Request SET Status = 5 WHERE RequestId = @RequestId AND OrderId = @OrderId;
		IF @@ROWCOUNT = 0
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN 1;
		END;

		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 5);

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