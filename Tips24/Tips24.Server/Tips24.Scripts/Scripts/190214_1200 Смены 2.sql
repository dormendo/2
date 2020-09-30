SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [telegram].[Turn_ExitAll]
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @BeginDateTime datetime2(7) = sysdatetime();
		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(ns, -100, @beginDateTime);

		UPDATE dbo.EmployeeTurnHistory SET EndDateTime = @PrevEndDateTime
			WHERE @BeginDateTime BETWEEN BeginDateTime AND EndDateTime;

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



ALTER PROCEDURE [payment].[LoadShareData]
	@PlaceId int,
	@EmployeeId int,
	@PaymentDateTime datetime2(7),
	@IsTimeSpecified bit,
	@PaymentLimit decimal(18,2) output,
	@SystemCommission decimal(4,2) output,
	@PlaceDisplayName nvarchar(100) output,
	@IsPlaceActive int output,
	@ShareSchemeHistoryId int output,
	@PersonalShare tinyint output,
	@EmployeeFirstName nvarchar(50) output,
	@EmployeeLastName nvarchar(50) output,
	@EmployeeIsFired bit output
AS
BEGIN
	SET NOCOUNT ON;

	SET @SystemCommission = 10;
	SET @PaymentLimit = 10000;

	IF (@EmployeeId IS NOT NULL)
	BEGIN
		SELECT @EmployeeFirstName = FirstName, @EmployeeLastName = LastName
			FROM dbo.EmployeePersonalDataHistory
			WHERE EmployeeId = @EmployeeId AND @PaymentDateTime BETWEEN BeginDateTime AND EndDateTime;
		
		SELECT @EmployeeIsFired = IsFired
			FROM dbo.EmployeeMembershipHistory
			WHERE EmployeeId = @EmployeeId AND @PaymentDateTime BETWEEN BeginDateTime AND EndDateTime;
		IF @@ROWCOUNT = 0
		BEGIN
			SET @EmployeeIsFired = 1;
		END;
	END;
	
	SELECT @ShareSchemeHistoryId = ssh.ShareSchemeHistoryId, @PlaceDisplayName = p.DisplayName, @PersonalShare = ssh.PersonalShare,
			@IsPlaceActive = IIF(pah.BeginDateTime IS NOT NULL, 1, 0)
		FROM dbo.Place p
		INNER JOIN dbo.ShareSchemeHistory ssh ON p.PlaceID = ssh.PlaceId
		LEFT JOIN dbo.PlaceActivityHistory pah ON pah.PlaceId = p.PlaceID AND @PaymentDateTime BETWEEN pah.BeginDateTime AND pah.EndDateTime
		WHERE ssh.PlaceId = @PlaceId AND @PaymentDateTime BETWEEN ssh.BeginDateTime AND ssh.EndDateTime;

	SELECT g.Name, sh.GroupId, sh.GroupWeight
		FROM dbo.SsGroupShareHistory sh
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = sh.GroupId
		WHERE sh.ShareSchemeHistoryId = @ShareSchemeHistoryId
		ORDER BY sh.GroupId;

	DECLARE @BeginOfDay datetime2(7) = CAST(CAST(@PaymentDateTime as date) as datetime2(7));
	DECLARE @BeginOfNextDay datetime2(7) = DATEADD(day, 1, @BeginOfDay);
	DECLARE @EndOfDay datetime2(7) = DATEADD(ns, -100, @BeginOfNextDay);

	IF (@IsTimeSpecified = 1)
	BEGIN
		SELECT EmployeeId, GroupId, IsManager, IsOwner,
				IIF(BeginDateTime < @BeginOfDay, @BeginOfDay, BeginDateTime) BeginDateTime,
				IIF(EndDateTime > @EndOfDay, @EndOfDay, EndDateTime) EndDateTime
			FROM
			(
				SELECT m.EmployeeId, m.GroupId, m.IsManager, m.IsOwner,
						IIF(m.BeginDateTime < t.BeginDateTime, t.BeginDateTime, m.BeginDateTime) BeginDateTime,
						IIF(m.EndDateTime > t.EndDateTime, t.EndDateTime, m.EndDateTime) EndDateTime
					FROM dbo.EmployeeMembershipHistory m
					INNER JOIN dbo.EmployeeTurnHistory t ON t.EmployeeId = m.EmployeeId AND @PaymentDateTime BETWEEN t.BeginDateTime AND t.EndDateTime
					WHERE m.PlaceId = @PlaceId AND @PaymentDateTime BETWEEN m.BeginDateTime AND m.EndDateTime AND m.IsFired = 0
			) a
			ORDER BY EmployeeId, BeginDateTime;
	END
	ELSE
	BEGIN
		SELECT EmployeeId, GroupId, IsManager, IsOwner,
				IIF(BeginDateTime < @BeginOfDay, @BeginOfDay, BeginDateTime) BeginDateTime,
				IIF(EndDateTime > @EndOfDay, @EndOfDay, EndDateTime) EndDateTime
			FROM
			(
				SELECT m.EmployeeId, m.GroupId, m.IsManager, m.IsOwner,
						IIF(m.BeginDateTime < t.BeginDateTime, t.BeginDateTime, m.BeginDateTime) BeginDateTime,
						IIF(m.EndDateTime > t.EndDateTime, t.EndDateTime, m.EndDateTime) EndDateTime
					FROM dbo.EmployeeMembershipHistory m
					INNER JOIN dbo.EmployeeTurnHistory t ON t.EmployeeId = m.EmployeeId AND
						(t.BeginDateTime BETWEEN m.BeginDateTime AND m.EndDateTime OR t.EndDateTime BETWEEN m.BeginDateTime AND m.EndDateTime)
					WHERE m.PlaceId = @PlaceId AND m.BeginDateTime <= @EndOfDay AND m.EndDateTime >= @BeginOfDay AND m.IsFired = 0
						AND t.BeginDateTime <= @EndOfDay AND t.EndDateTime >= @BeginOfDay
			) a
			ORDER BY EmployeeId, BeginDateTime;
	END;
END;
GO



CREATE NONCLUSTERED INDEX [IX_EmployeeMembershipHistory_LoadShare] ON [dbo].[EmployeeMembershipHistory]
(
	[PlaceId] ASC,
	[EndDateTime] DESC
)
INCLUDE
(
	[GroupId],
	[IsFired],
	[IsManager],
	[IsOwner],
	[BeginDateTime]
);
GO

CREATE NONCLUSTERED INDEX [IX_EmployeePersonalDataHistory_GetData] ON [dbo].[EmployeePersonalDataHistory]
(
	[EmployeeId] ASC,
	[EndDateTime] DESC
)
INCLUDE
(
	[BeginDateTime],
	[FirstName],
	[LastName]
);
GO



CREATE NONCLUSTERED INDEX [IX_LoadShare] ON [dbo].[EmployeeTurnHistory]
(
	[EmployeeId] ASC,
	[EndDateTime] DESC
)
INCLUDE
(
	[BeginDateTime]
);
GO

CREATE NONCLUSTERED INDEX [IX_ShareSchemeHistory_LoadShare] ON [dbo].[ShareSchemeHistory]
(
	[PlaceId] ASC,
	[EndDateTime] DESC
)
INCLUDE
(
	[PersonalShare],
	[BeginDateTime]
);
GO



SET XACT_ABORT ON
BEGIN TRANSACTION
GO
ALTER TABLE payment.Payment
	DROP CONSTRAINT DF_Payment_PaymentId
GO
ALTER TABLE payment.Payment
	DROP CONSTRAINT DF_Payment_Status
GO
ALTER TABLE payment.Payment
	DROP CONSTRAINT DF_Payment_IsPaymentTimeSpecified
GO
CREATE TABLE payment.Tmp_Payment
(
	PaymentId bigint NOT NULL,
	PlaceId int NOT NULL,
	EmployeeId int NULL,
	ShareSchemeHistoryId int NOT NULL,
	DataSourceId tinyint NOT NULL,
	ProviderId tinyint NOT NULL,
	Status tinyint NOT NULL,
	ReasonToReturn tinyint NULL,
	OriginalAmount decimal(18, 2) NOT NULL,
	ReceivedAmount decimal(18, 2) NOT NULL,
	BankCommissionAmount decimal(18, 2) NOT NULL,
	AgentCommissionAmount decimal(18, 2) NOT NULL,
	IncomeAmount decimal(18, 2) NOT NULL,
	PayoutAmount decimal(18, 2) NOT NULL,
	PaymentDateTime datetime2(7) NOT NULL,
	IsPaymentTimeSpecified bit NOT NULL,
	ArrivalDateTime datetime2(7) NOT NULL,
	ReturnDateTime datetime2(7) NULL
)
GO
ALTER TABLE payment.Tmp_Payment SET (LOCK_ESCALATION = TABLE)
GO
DECLARE @v sql_variant 
SET @v = N'Синтетический ключ'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'PaymentId'
GO
DECLARE @v sql_variant 
SET @v = N'Идентификатор заведения'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'PlaceId'
GO
DECLARE @v sql_variant 
SET @v = N'Идентификатор сотрудника для персонального платежа'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'EmployeeId'
GO
DECLARE @v sql_variant 
SET @v = N'Тип платежа (0 - Сбербанк.Онлайн)'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'DataSourceId'
GO
DECLARE @v sql_variant 
SET @v = N'Исходная сумма платежа'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'OriginalAmount'
GO
DECLARE @v sql_variant 
SET @v = N'Сумма, полученая нами'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'ReceivedAmount'
GO
DECLARE @v sql_variant 
SET @v = N'Сумма комиссии платёжной системы'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'BankCommissionAmount'
GO
DECLARE @v sql_variant 
SET @v = N'Сумма предполагаемого дохода до уплаты комиссии на вывод'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'IncomeAmount'
GO
DECLARE @v sql_variant 
SET @v = N'Сумма к выплате сотрудникам заведения'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'PayoutAmount'
GO
DECLARE @v sql_variant 
SET @v = N'ВНО. Дата и время совершения платежа'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'PaymentDateTime'
GO
DECLARE @v sql_variant 
SET @v = N'ВС. Дата и время внесения данных в систему'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'payment', N'TABLE', N'Tmp_Payment', N'COLUMN', N'ArrivalDateTime'
GO
ALTER TABLE payment.Tmp_Payment ADD CONSTRAINT
	DF_Payment_PaymentId DEFAULT (NEXT VALUE FOR [payment].[Seq_Payment]) FOR PaymentId
GO
ALTER TABLE payment.Tmp_Payment ADD CONSTRAINT
	DF_Payment_Status DEFAULT ((0)) FOR Status
GO
ALTER TABLE payment.Tmp_Payment ADD CONSTRAINT
	DF_Payment_IsPaymentTimeSpecified DEFAULT ((0)) FOR IsPaymentTimeSpecified
GO
IF EXISTS(SELECT * FROM payment.Payment)
	 EXEC('INSERT INTO payment.Tmp_Payment (PaymentId, PlaceId, EmployeeId, ShareSchemeHistoryId, DataSourceId, ProviderId, Status, OriginalAmount, ReceivedAmount, BankCommissionAmount, AgentCommissionAmount, IncomeAmount, PayoutAmount, PaymentDateTime, IsPaymentTimeSpecified, ArrivalDateTime, ReturnDateTime)
		SELECT PaymentId, PlaceId, EmployeeId, ShareSchemeHistoryId, DataSourceId, ProviderId, Status, OriginalAmount, ReceivedAmount, BankCommissionAmount, AgentCommissionAmount, IncomeAmount, PayoutAmount, PaymentDateTime, IsPaymentTimeSpecified, ArrivalDateTime, ReturnDateTime FROM payment.Payment WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE payment.Payment
GO
EXECUTE sp_rename N'payment.Tmp_Payment', N'Payment', 'OBJECT' 
GO
ALTER TABLE payment.Payment ADD CONSTRAINT PK_Payment PRIMARY KEY CLUSTERED 
(
	PaymentId
);
GO

CREATE NONCLUSTERED INDEX IX_Payment_EmployeeId ON payment.Payment
(
	EmployeeId,
	PaymentDateTime
) WHERE ([EmployeeId] IS NOT NULL);
GO

CREATE NONCLUSTERED INDEX IX_Payment_PlaceId ON payment.Payment
(
	PlaceId,
	PaymentDateTime
)
GO;
COMMIT
GO


UPDATE payment.Payment SET ReasonToReturn = 0 WHERE Status = 2;
GO




ALTER PROCEDURE [payment].[SavePayment]
	@PlaceId int,
	@EmployeeId int,
	@DataSource char(6),
	@Provider char(6),
	@ShareSchemeHistoryId int,
	@Status tinyint,
	@ReasonToReturn tinyint,
	@OriginalAmount decimal(18,2),
	@ReceivedAmount decimal(18,2),
	@BankCommissionAmount decimal(18,2),
	@AgentCommissionAmount decimal(18,2),
	@IncomeAmount decimal(18,2),
	@PayoutAmount decimal(18,2),
	@PaymentDateTime datetime2(7),
	@IsTimeSpecified bit,
	@ArrivalDateTime datetime2(7),
	@DocumentName varchar(100),
	@DocumentNumber varchar(40),
	@DocumentDate date,
	@DocumentId int output,
	@ExternalId varchar(50),
	@Fio nvarchar(100),
	@Address nvarchar(150),
	@Purpose nvarchar(150),
	@RawData nvarchar(max),
	@Amounts payment.PaymentShare READONLY,
	@PaymentId bigint output,
	@FinalStatus tinyint output
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @DataSourceId tinyint;
	DECLARE @ProviderId tinyint;

	BEGIN TRANSACTION;
	BEGIN TRY
		SELECT @DataSourceId = Id FROM payment.DataSource WHERE Code = @DataSource;
		SELECT @ProviderId = Id FROM payment.Provider WHERE Code = @Provider;

		IF (@DocumentName IS NOT NULL)
		BEGIN
			SELECT @DocumentId = DocumentId
				FROM payment.Document with (xlock, rowlock, serializable)
				WHERE DataSourceId = @DataSourceId AND DocumentName = @DocumentName;

			IF (@@ROWCOUNT = 0)
			BEGIN
				SET @DocumentId = NEXT VALUE FOR payment.Seq_Document;
				INSERT INTO payment.Document (DocumentId, DataSourceId, ProviderId, DocumentName, DocumentNumber, DocumentDate)
					VALUES (@DocumentId, @DataSourceId, @ProviderId, @DocumentName, @DocumentNumber, @DocumentDate);
			END;
		END;

		SELECT @PaymentId = NEXT VALUE FOR payment.Seq_Payment;
		INSERT INTO payment.Payment (PaymentId, PlaceId, EmployeeId, ShareSchemeHistoryId, DataSourceId, ProviderId, Status, ReasonToReturn, OriginalAmount, ReceivedAmount,
				BankCommissionAmount, AgentCommissionAmount, IncomeAmount, PayoutAmount, PaymentDateTime, IsPaymentTimeSpecified, ArrivalDateTime)
			VALUES (@PaymentId, @PlaceId, @EmployeeId, @ShareSchemeHistoryId, @DataSourceId, @ProviderId, IIF(@Status = 3, 3, 0), @ReasonToReturn, @OriginalAmount, @ReceivedAmount,
				@BankCommissionAmount, @AgentCommissionAmount, @IncomeAmount, @PayoutAmount, @PaymentDateTime, @IsTimeSpecified, @ArrivalDateTime);

		INSERT INTO payment.PaymentExternalData (PaymentId, DocumentId, ExternalId, DataSourceId, ProviderId, Fio, Address, Purpose)
			VALUES (@PaymentId, @DocumentId, @ExternalId, @DataSourceId, @ProviderId, @Fio, @Address, @Purpose)

		INSERT INTO payment.PaymentShare (PaymentId, EmployeeId, Amount)
			SELECT @PaymentId, EmployeeId, Amount FROM @Amounts;

		IF (@RawData IS NOT NULL)
		BEGIN
			INSERT INTO payment.RawData (PaymentId, RawData) VALUES (@PaymentId, @RawData);
		END;

		IF (@Status = 1)
		BEGIN
			EXECUTE payment.ApprovePayment @PaymentId = @PaymentId;
		END;

		SELECT @FinalStatus = Status FROM payment.Payment WHERE PaymentId = @PaymentId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO



ALTER PROCEDURE [payment].[ReturnPayment]
	@PaymentId int
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @OldStatus tinyint;

		SELECT @OldStatus = Status FROM payment.Payment with(updlock) WHERE PaymentId = @PaymentId AND Status IN (0, 1, 3);
		IF (@@ROWCOUNT = 1)
		BEGIN
			UPDATE payment.Payment SET Status = 2, ReturnDateTime = sysdatetime(), ReasonToReturn = ISNULL(ReasonToReturn, 3) WHERE PaymentId = @PaymentId;

			IF (@OldStatus = 1)
			BEGIN
				DECLARE @list payment.EmployeeBalanceLogList;

				INSERT INTO @list (EmployeeId, OperationType, Amount, PaymentId)
				SELECT EmployeeId, 3, -Amount, @PaymentId
					FROM payment.PaymentShare with (updlock, rowlock)
					WHERE PaymentId = @PaymentId;

				EXECUTE payment.InsertEmployeeBalanceLog @list = @list;
			END;
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



CREATE TABLE [dbo].[PlaceActivityHistory](
	[PlaceId] [int] NOT NULL,
	[SeqNum] [int] NOT NULL,
	[BeginDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NOT NULL,
	CONSTRAINT [PK_PlaceActivityHistory] PRIMARY KEY CLUSTERED 
	(
		[PlaceId] ASC,
		[SeqNum] ASC
	)
);
GO




CREATE SEQUENCE [dbo].[Seq_PlaceActivityHistory_SeqNum] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE 1
 MAXVALUE 2147483647
 CACHE 
GO



ALTER TABLE [dbo].[PlaceActivityHistory] ADD  CONSTRAINT [DF_PlaceActivityHistory_SeqNum]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_PlaceActivityHistory_SeqNum]) FOR [SeqNum]
GO




ALTER PROCEDURE [admin].[CreatePlace]
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
	@BeginDateTime datetime2(7) = null,
	@PlaceId int output
AS
BEGIN
	SET NOCOUNT ON;

	IF @BeginDateTime IS NULL
	BEGIN
		SET @BeginDateTime = CAST(CAST(sysdatetime() as date) as datetime2(7));
	END;

	SET @PlaceId = NEXT VALUE FOR dbo.Seq_Place;
	DECLARE @InfinityDateTime datetime2(7) = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999', 120);

	BEGIN TRANSACTION;
	BEGIN TRY
		INSERT INTO dbo.Place (PlaceID, RegisterDateTime, DisplayName, Address, City, Phone, Email, TimeZoneId, Info)
			VALUES (@PlaceId, @BeginDateTime, @DisplayName, @Address, @City, @Phone, @Email, @TimeZoneId, @Info);

		INSERT INTO dbo.PlaceActivityHistory (PlaceId, BeginDateTime, EndDateTime)
			VALUES (@PlaceId, @BeginDateTime, @InfinityDateTime);

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



ALTER PROCEDURE [telegram].[GetUserRecordByUserId]
	@UserId bigint
AS
BEGIN
	DECLARE @curDt datetime2(7) = SYSDATETIME();

	SELECT e.EmployeeId, e.Phone, pd.FirstName, pd.LastName, e.Balance, em.PlaceId, p.DisplayName PlaceName, p.Address PlaceAddress, p.City PlaceCity,
			em.GroupId, g.Name GroupName, em.IsFired, em.IsManager, em.IsOwner, t.QrCodeFileId, t.QrCodeStringHash, t.QrCodeDateTime,
			IIF (pah.BeginDateTime IS NULL, 0, 1) IsPlaceActive,
			ds.SessionType DialogSessionType, ds.Step DialogStep, ds.Data DialogData,
			th.SeqNum TurnSeqNum, th.BeginDateTime TurnBeginDateTime, th.EndDateTime TurnEndDateTime
		FROM telegram.BotUser t
		INNER JOIN dbo.Employee e ON e.Phone = t.Phone
		INNER JOIN dbo.EmployeeMembershipHistory em ON e.EmployeeId = em.EmployeeId AND @curDt BETWEEN em.BeginDateTime AND em.EndDateTime
		INNER JOIN dbo.Place p ON p.PlaceId = em.PlaceId
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
		INNER JOIN dbo.EmployeePersonalDataHistory pd ON pd.EmployeeId = e.EmployeeId AND @curDt BETWEEN pd.BeginDateTime AND pd.EndDateTime
		LEFT JOIN dbo.EmployeeTurnHistory th ON th.EmployeeId = e.EmployeeId AND th.PlaceId = p.PlaceID AND @curDt BETWEEN th.BeginDateTime AND th.EndDateTime
		LEFT JOIN dbo.PlaceActivityHistory pah ON pah.PlaceId = p.PlaceID AND @curDt BETWEEN th.BeginDateTime AND pah.EndDateTime
		LEFT JOIN telegram.DialogSession ds ON ds.TelegramId = t.TelegramId
		WHERE t.TelegramId = @UserId;
END;
GO



