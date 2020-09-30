/****** Object:  Table [dbo].[EmployeeMembershipHistory]    Script Date: 06.02.2019 22:42:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EmployeeTurnHistory](
	[EmployeeId] [int] NOT NULL,
	[SeqNum] [int] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[BeginDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NOT NULL,
	[OpenedBy] [int] NULL,
	[ClosedBy] [int] NULL,
	CONSTRAINT [PK_EmployeeTurnHistory] PRIMARY KEY CLUSTERED 
	(
		[EmployeeId] ASC,
		[SeqNum] ASC
	)
);
GO

/****** Object:  Sequence [dbo].[Seq_EmployeeMembershipHistory_SeqNum]    Script Date: 06.02.2019 23:04:30 ******/
CREATE SEQUENCE [dbo].[Seq_EmployeeTurnHistory_SeqNum] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE 1
 MAXVALUE 2147483647
 CACHE 
GO



ALTER TABLE [dbo].[EmployeeTurnHistory] ADD  CONSTRAINT [DF_EmployeeTurnHistory_SeqNum]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_EmployeeTurnHistory_SeqNum]) FOR [SeqNum]
GO

ALTER TABLE [dbo].[EmployeeTurnHistory] ADD  CONSTRAINT [DF_EmployeeTurnHistory_EndDateTime]  DEFAULT (CONVERT([datetime2](7),'9999-12-31 23:59:59.9999999')) FOR [EndDateTime]
GO





CREATE TABLE [telegram].[DialogSession](
	[TelegramId] [bigint] NOT NULL,
	[SessionType] [tinyint] NOT NULL,
	[Step] [tinyint] NOT NULL,
	[Data] [nvarchar](200) NULL,
	CONSTRAINT [PK_telegram_DialogSession] PRIMARY KEY CLUSTERED 
	(
		[TelegramId] ASC
	)
);
GO



DROP TABLE telegram.YandexKassaSession;
GO



ALTER PROCEDURE [telegram].[GetUserRecordByUserId]
	@UserId bigint
AS
BEGIN
	DECLARE @curDt datetime2(7) = SYSDATETIME();

	SELECT e.EmployeeId, e.Phone, pd.FirstName, pd.LastName, e.Balance, em.PlaceId, p.DisplayName PlaceName, p.Address PlaceAddress, p.City PlaceCity,
			em.GroupId, g.Name GroupName, em.IsFired, em.IsManager, em.IsOwner, t.QrCodeFileId, t.QrCodeStringHash, t.QrCodeDateTime,
			ds.SessionType DialogSessionType, ds.Step DialogStep, ds.Data DialogData,
			th.SeqNum TurnSeqNum, th.BeginDateTime TurnBeginDateTime, th.EndDateTime TurnEndDateTime
		FROM telegram.BotUser t
		INNER JOIN dbo.Employee e ON e.Phone = t.Phone
		INNER JOIN dbo.EmployeeMembershipHistory em ON e.EmployeeId = em.EmployeeId AND @curDt BETWEEN em.BeginDateTime AND em.EndDateTime
		INNER JOIN dbo.Place p ON p.PlaceId = em.PlaceId
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
		INNER JOIN dbo.EmployeePersonalDataHistory pd ON pd.EmployeeId = e.EmployeeId AND @curDt BETWEEN pd.BeginDateTime AND pd.EndDateTime
		LEFT JOIN dbo.EmployeeTurnHistory th ON th.EmployeeId = e.EmployeeId AND th.PlaceId = p.PlaceID AND @curDt BETWEEN th.BeginDateTime AND th.EndDateTime
		LEFT JOIN telegram.DialogSession ds ON ds.TelegramId = t.TelegramId
		WHERE t.TelegramId = @UserId;
END;
GO


DROP PROCEDURE [telegram].[CancelYandexKassaSession];
GO
DROP PROCEDURE [telegram].[CompleteYandexKassaSession];
GO
DROP PROCEDURE [telegram].[StartYandexKassaSession];
GO
DROP PROCEDURE [telegram].[UpdateYandexKassaSession];
GO



CREATE PROCEDURE [telegram].[DialogSession_Cancel]
	@TelegramId bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM telegram.DialogSession WHERE TelegramId = @TelegramId;
END
GO


CREATE PROCEDURE [telegram].[DialogSession_YandexKassa_Complete]
	@TelegramId bigint,
	@PlaceId int,
	@EmployeeId int,
	@UserLogin varchar(50),
	@Amount decimal(18,2),
	@ProviderId int
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRANSACTION;
	BEGIN TRY
		
		DECLARE @curDt datetime2(7) = sysdatetime();
		DECLARE @RequestId int = NEXT VALUE FOR payment.Seq_YandexKassaRequest;

		INSERT INTO payment.YandexKassaRequest (RequestId, Status, PlaceId, EmployeeId, CreateDateTime, UserLogin, Amount, ProviderId)
			VALUES (@RequestId, 0, @PlaceId, @EmployeeId, @curDt, @UserLogin, @Amount, @ProviderId);
		
		INSERT INTO payment.YandexKassaRequestLog (RequestId, LogDateTime, Operation) VALUES (@RequestId, @curDt, 0);

		DELETE FROM telegram.DialogSession WHERE TelegramId = @TelegramId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO


CREATE PROCEDURE [telegram].[DialogSession_Start]
	@TelegramId bigint,
	@SessionType tinyint,
	@Step tinyint,
	@Data nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DELETE FROM telegram.DialogSession WHERE TelegramId = @TelegramId;
		INSERT INTO telegram.DialogSession (TelegramId, SessionType, Step, Data)
			VALUES (@TelegramId, @SessionType, @Step, @Data);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO


CREATE PROCEDURE [telegram].[DialogSession_Update]
	@TelegramId bigint,
	@Step tinyint,
	@Data nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE telegram.DialogSession SET Step = @Step, Data = @Data WHERE TelegramId = @TelegramId;
	IF (@@ROWCOUNT = 0)
		RETURN -1;
	
	RETURN 0;
END
GO


CREATE PROCEDURE [telegram].[DialogSession_Turn_Enter]
	@TelegramId bigint,
	@EmployeeId int,
	@PlaceId int,
	@Message nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @BeginDateTime datetime2(7) = sysdatetime();
		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(ns, -100, @beginDateTime);
		DECLARE @InfinityDateTime datetime2(7) = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999', 120);

		UPDATE dbo.EmployeeTurnHistory SET EndDateTime = @PrevEndDateTime
			WHERE EmployeeId = @EmployeeId AND @BeginDateTime BETWEEN BeginDateTime AND EndDateTime;

		INSERT INTO dbo.EmployeeTurnHistory (EmployeeId, PlaceId, BeginDateTime, EndDateTime)
			VALUES (@EmployeeId, @PlaceId, @BeginDateTime, @InfinityDateTime);

		INSERT INTO telegram.Notification (TelegramId, CreateDateTime, Message)
		SELECT u.TelegramId, @BeginDateTime, @Message
			FROM dbo.EmployeeMembershipHistory m
			INNER JOIN dbo.Employee e ON m.EmployeeId = e.EmployeeId
			INNER JOIN telegram.BotUser u ON u.Phone = e.Phone
			WHERE m.PlaceId = @PlaceId AND m.IsManager = 1 AND m.IsFired = 0 AND @BeginDateTime BETWEEN m.BeginDateTime AND m.EndDateTime;
		
		DELETE FROM telegram.DialogSession WHERE TelegramId = @TelegramId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO



CREATE PROCEDURE [telegram].[DialogSession_Turn_Exit]
	@TelegramId bigint,
	@EmployeeId int,
	@PlaceId int,
	@Message nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @BeginDateTime datetime2(7) = sysdatetime();
		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(ns, -100, @beginDateTime);

		UPDATE dbo.EmployeeTurnHistory SET EndDateTime = @PrevEndDateTime
			WHERE EmployeeId = @EmployeeId AND @BeginDateTime BETWEEN BeginDateTime AND EndDateTime;

		INSERT INTO telegram.Notification (TelegramId, CreateDateTime, Message)
		SELECT u.TelegramId, @BeginDateTime, @Message
			FROM dbo.EmployeeMembershipHistory m
			INNER JOIN dbo.Employee e ON m.EmployeeId = e.EmployeeId
			INNER JOIN telegram.BotUser u ON u.Phone = e.Phone
			WHERE m.PlaceId = @PlaceId AND m.IsManager = 1 AND m.IsFired = 0 AND @BeginDateTime BETWEEN m.BeginDateTime AND m.EndDateTime;
		
		DELETE FROM telegram.DialogSession WHERE TelegramId = @TelegramId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO



ALTER PROCEDURE [telegram].[GetUsersToNotify]
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #t1
	(
		EmployeeId int NOT NULL PRIMARY KEY CLUSTERED,
		TelegramId bigint NOT NULL,
		LastNotifiedLogId bigint NOT NULL
	);

	INSERT INTO #t1 (EmployeeId, TelegramId, LastNotifiedLogId)
	SELECT e.EmployeeId, u.TelegramId, ISNULL(u.LastBalanceLogId, 0) LastNotifiedLogId
		FROM telegram.BotUser u
		INNER JOIN dbo.Employee e ON u.Phone = e.Phone AND e.LastBalanceLogId IS NOT NULL AND ISNULL(u.LastBalanceLogId, 0) < e.LastBalanceLogId;

	SELECT t.EmployeeId, t.TelegramId, l.EmployeeBalanceLogId, l.LogDateTime, l.OperationType, l.Amount, l.Balance, l.PaymentId, p.OriginalAmount
		FROM #t1 t
		INNER JOIN dbo.EmployeeBalanceLog l ON t.EmployeeId = l.EmployeeId AND l.EmployeeBalanceLogId > t.LastNotifiedLogId
		LEFT JOIN payment.Payment p ON l.PaymentId = p.PaymentId
		ORDER BY l.EmployeeId, l.EmployeeBalanceLogId;

	DROP TABLE #t1;
END;
GO



CREATE SEQUENCE [telegram].[Seq_Notification] 
 START WITH 1
 INCREMENT BY 1
 MINVALUE 1
GO



CREATE TABLE telegram.Notification
(
	NotificationId bigint NOT NULL CONSTRAINT DF__telegram_Notification__NotificationId DEFAULT NEXT VALUE FOR telegram.Seq_Notification PRIMARY KEY CLUSTERED,
	TelegramId bigint NOT NULL,
	CreateDateTime datetime2(7) NOT NULL,
	Message nvarchar(200) NOT NULL
);
GO



ALTER PROCEDURE [telegram].[UpdateLastBalanceLogId]
	@UserId bigint,
	@LastBalanceLogId bigint,
	@Message nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE telegram.BotUser SET LastBalanceLogId = @LastBalanceLogId WHERE TelegramId = @UserId;

		INSERT INTO telegram.Notification (TelegramId, CreateDateTime, Message) VALUES (@UserId, sysdatetime(), @Message);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO



CREATE PROCEDURE telegram.Notifications_Proceed
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

	SELECT TelegramId, CreateDateTime, Message FROM telegram.Notification WHERE NotificationId IN (SELECT NotificationId FROM #t1);

	DELETE FROM telegram.Notification WHERE NotificationId IN (SELECT NotificationId FROM #t1);
END;
GO



CREATE PROCEDURE [telegram].[Turn_Employees]
	@PlaceId int,
	@EmployeeId int
AS
BEGIN
	DECLARE @curDt datetime2(7) = SYSDATETIME();

	SELECT *
		FROM
		(
			SELECT em.EmployeeId, epd.FirstName, epd.LastName, em.GroupId, g.Name GroupName,
					CASE
						WHEN EXISTS(SELECT * FROM dbo.EmployeeTurnHistory th WHERE th.EmployeeId = em.EmployeeId AND @curDt BETWEEN th.BeginDateTime AND th.EndDateTime) THEN 1
						ELSE 0 END IsInTurn
				FROM dbo.EmployeeMembershipHistory em
				INNER JOIN dbo.EmployeePersonalDataHistory epd ON epd.EmployeeId = em.EmployeeId AND @curDt BETWEEN epd.BeginDateTime AND epd.EndDateTime
				INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
				WHERE @curDt BETWEEN em.BeginDateTime AND em.EndDateTime AND em.PlaceId = @PlaceId AND em.IsFired = 0 AND em.EmployeeId != @EmployeeId
		) t
		ORDER BY IsInTurn, CONCAT(FirstName, ' ', LastName);
END;
GO



CREATE PROCEDURE [telegram].[DialogSession_Turn_EnterExitEmployee]
	@ManagerId int,
	@ManagerTelegramId bigint,
	@ManagerName nvarchar(101),
	@EmployeeId int,
	@PlaceId int,
	@IsEnter bit
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @CurrentDateTime datetime2(7) = sysdatetime();

		IF NOT EXISTS(SELECT *
			FROM dbo.EmployeeMembershipHistory with(xlock, rowlock)
			WHERE EmployeeId = @ManagerId AND @CurrentDateTime BETWEEN BeginDateTime AND EndDateTime AND IsManager = 1 AND IsFired = 0)
		BEGIN
			IF @@TRANCOUNT > 0
				ROLLBACK TRANSACTION;
			RETURN -1;
		END;

		IF NOT EXISTS(SELECT *
			FROM dbo.EmployeeMembershipHistory with(updlock, rowlock)
			WHERE EmployeeId = @EmployeeId AND PlaceId = @PlaceId AND @CurrentDateTime BETWEEN BeginDateTime AND EndDateTime AND IsFired = 0)
		BEGIN
			IF @@TRANCOUNT > 0
				ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		DECLARE @EmployeeName nvarchar(101);
		SELECT @EmployeeName = CONCAT(FirstName, N' ', LastName)
			FROM dbo.EmployeePersonalDataHistory
			WHERE EmployeeId = @EmployeeId AND @CurrentDateTime BETWEEN BeginDateTime AND EndDateTime;

		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(ns, -100, @CurrentDateTime);
		DECLARE @InfinityDateTime datetime2(7) = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999', 120);

		DECLARE @IsInTurn bit = 0;
		IF EXISTS(SELECT * FROM dbo.EmployeeTurnHistory with (updlock, rowlock) WHERE EmployeeId = @EmployeeId AND @CurrentDateTime BETWEEN BeginDateTime AND EndDateTime)
		BEGIN
			SET @IsInTurn = 1;
		END;

		DECLARE @ReturnValue int = 0
		IF (@IsEnter = 1 AND @IsInTurn = 1)
		BEGIN
			SET @ReturnValue = 1;
		END;

		IF (@IsEnter = 0 AND @IsInTurn = 0)
		BEGIN
			SET @ReturnValue = 2;
		END;


		DECLARE @ManagerMessage nvarchar(200);
		DECLARE @EmployeeMessage nvarchar(200);

		IF (@IsEnter = 0)
		BEGIN
			UPDATE dbo.EmployeeTurnHistory SET EndDateTime = @PrevEndDateTime, ClosedBy = @ManagerId
				WHERE EmployeeId = @EmployeeId AND @CurrentDateTime BETWEEN BeginDateTime AND EndDateTime;
			SET @ManagerMessage = @ManagerName + N' закрыл смену ' + @EmployeeName;
			SET @EmployeeMessage = @ManagerName + N' закрыл Вашу смену ';
		END;

		IF (@IsEnter = 1)
		BEGIN
			INSERT INTO dbo.EmployeeTurnHistory (EmployeeId, PlaceId, BeginDateTime, EndDateTime, OpenedBy)
				VALUES (@EmployeeId, @PlaceId, @CurrentDateTime, @InfinityDateTime, @ManagerId);
			SET @ManagerMessage = @ManagerName + N' открыл смену ' + @EmployeeName;
			SET @EmployeeMessage = @ManagerName + N' открыл Вашу смену ';
		END;

		IF (@ReturnValue = 0)
		BEGIN
			INSERT INTO telegram.Notification (TelegramId, CreateDateTime, Message)
			SELECT u.TelegramId, @CurrentDateTime, @ManagerMessage
				FROM dbo.EmployeeMembershipHistory m
				INNER JOIN dbo.Employee e ON m.EmployeeId = e.EmployeeId
				INNER JOIN telegram.BotUser u ON u.Phone = e.Phone
				WHERE m.PlaceId = @PlaceId AND m.IsManager = 1 AND m.IsFired = 0 AND @CurrentDateTime BETWEEN m.BeginDateTime AND m.EndDateTime;

			INSERT INTO telegram.Notification (TelegramId, CreateDateTime, Message)
			SELECT u.TelegramId, @CurrentDateTime, @EmployeeMessage
				FROM dbo.Employee e
				INNER JOIN telegram.BotUser u ON u.Phone = e.Phone
				WHERE e.EmployeeId = @EmployeeId;
		END;
				
		DELETE FROM telegram.DialogSession WHERE TelegramId = @ManagerTelegramId;

		COMMIT TRANSACTION;

		RETURN @ReturnValue;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;

	RETURN 0;
END
GO

