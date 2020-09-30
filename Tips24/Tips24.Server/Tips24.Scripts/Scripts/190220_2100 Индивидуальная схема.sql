SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ShareSchemeHistory
	DROP CONSTRAINT DF_ShareSchemeHistory_ShareSchemeHistoryId
GO
ALTER TABLE dbo.ShareSchemeHistory
	DROP CONSTRAINT DF_ShareSchemeHistory_EndDateTime
GO
CREATE TABLE dbo.Tmp_ShareSchemeHistory
(
	ShareSchemeHistoryId int NOT NULL,
	PlaceId int NOT NULL,
	PersonalShare tinyint NOT NULL,
	IsIndividual bit NOT NULL,
	BeginDateTime datetime2(7) NOT NULL,
	EndDateTime datetime2(7) NOT NULL
);
GO
ALTER TABLE dbo.Tmp_ShareSchemeHistory SET (LOCK_ESCALATION = TABLE)
GO
DECLARE @v sql_variant 
SET @v = N'Идентификатор заведения'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'dbo', N'TABLE', N'Tmp_ShareSchemeHistory', N'COLUMN', N'PlaceId'
GO
DECLARE @v sql_variant 
SET @v = N'Индивидуальная доля'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'dbo', N'TABLE', N'Tmp_ShareSchemeHistory', N'COLUMN', N'PersonalShare'
GO
DECLARE @v sql_variant 
SET @v = N'ВЗ. Дата и время начала действия'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'dbo', N'TABLE', N'Tmp_ShareSchemeHistory', N'COLUMN', N'BeginDateTime'
GO
DECLARE @v sql_variant 
SET @v = N'ВЗ. Дата и время окончания действия'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'dbo', N'TABLE', N'Tmp_ShareSchemeHistory', N'COLUMN', N'EndDateTime'
GO
ALTER TABLE dbo.Tmp_ShareSchemeHistory ADD CONSTRAINT
	DF_ShareSchemeHistory_ShareSchemeHistoryId DEFAULT (NEXT VALUE FOR [dbo].[Seq_ShareSchemeHistory]) FOR ShareSchemeHistoryId
GO
ALTER TABLE dbo.Tmp_ShareSchemeHistory ADD CONSTRAINT
	DF_ShareSchemeHistory_IsIndividual DEFAULT 0 FOR IsIndividual
GO
ALTER TABLE dbo.Tmp_ShareSchemeHistory ADD CONSTRAINT
	DF_ShareSchemeHistory_EndDateTime DEFAULT (CONVERT([datetime2](7),'9999-12-31 23:59:59.9999999')) FOR EndDateTime
GO
IF EXISTS(SELECT * FROM dbo.ShareSchemeHistory)
	 EXEC('INSERT INTO dbo.Tmp_ShareSchemeHistory (ShareSchemeHistoryId, PlaceId, PersonalShare, BeginDateTime, EndDateTime)
		SELECT ShareSchemeHistoryId, PlaceId, PersonalShare, BeginDateTime, EndDateTime FROM dbo.ShareSchemeHistory WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.ShareSchemeHistory
GO
EXECUTE sp_rename N'dbo.Tmp_ShareSchemeHistory', N'ShareSchemeHistory', 'OBJECT' 
GO
ALTER TABLE dbo.ShareSchemeHistory ADD CONSTRAINT
	PK_ShareSchemeHistory_1 PRIMARY KEY CLUSTERED 
	(
	ShareSchemeHistoryId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_ShareSchemeHistory_LoadShare ON dbo.ShareSchemeHistory
	(
	PlaceId,
	EndDateTime DESC
	) INCLUDE (PersonalShare, BeginDateTime) 
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
GO



ALTER PROCEDURE [admin].[UpdateShareScheme]
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
		DECLARE @IsIndividual bit = IIF (@Weight1 + @Weight2 + @Weight3 + @Weight4 = 0, 1, 0);
		UPDATE dbo.ShareSchemeHistory with (xlock, serializable) SET EndDateTime = @PrevEndDateTime WHERE PlaceId = @PlaceId AND EndDateTime = @InfinityDateTime;

		DECLARE @ShareSchemeHistoryId int = NEXT VALUE FOR dbo.Seq_ShareSchemeHistory;
		INSERT INTO dbo.ShareSchemeHistory (ShareSchemeHistoryId, PlaceId, PersonalShare, IsIndividual, BeginDateTime, EndDateTime)
			VALUES (@ShareSchemeHistoryId, @PlaceId, @PersonalShare, @IsIndividual, @BeginDateTime, @InfinityDateTime);

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



ALTER PROCEDURE [telegram].[GetUserRecordByUserId]
	@UserId bigint
AS
BEGIN
	DECLARE @curDt datetime2(7) = SYSDATETIME();

	SELECT e.EmployeeId, e.Phone, pd.FirstName, pd.LastName, e.Balance, em.PlaceId, p.DisplayName PlaceName, p.Address PlaceAddress, p.City PlaceCity,
			em.GroupId, g.Name GroupName, em.IsFired, em.IsManager, em.IsOwner, t.QrCodeFileId, t.QrCodeStringHash, t.QrCodeDateTime,
			IIF (pah.BeginDateTime IS NULL, 0, 1) IsPlaceActive, ssh.IsIndividual IsPlaceSchemeIndividual,
			ds.SessionType DialogSessionType, ds.Step DialogStep, ds.Data DialogData,
			th.SeqNum TurnSeqNum, th.BeginDateTime TurnBeginDateTime, th.EndDateTime TurnEndDateTime
		FROM telegram.BotUser t
		INNER JOIN dbo.Employee e ON e.Phone = t.Phone
		INNER JOIN dbo.EmployeeMembershipHistory em ON e.EmployeeId = em.EmployeeId AND @curDt BETWEEN em.BeginDateTime AND em.EndDateTime
		INNER JOIN dbo.Place p ON p.PlaceId = em.PlaceId
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
		INNER JOIN dbo.ShareSchemeHistory ssh ON ssh.PlaceId = p.PlaceID AND @curDt BETWEEN ssh.BeginDateTime AND ssh.EndDateTime
		INNER JOIN dbo.EmployeePersonalDataHistory pd ON pd.EmployeeId = e.EmployeeId AND @curDt BETWEEN pd.BeginDateTime AND pd.EndDateTime
		LEFT JOIN dbo.EmployeeTurnHistory th ON th.EmployeeId = e.EmployeeId AND th.PlaceId = p.PlaceID AND @curDt BETWEEN th.BeginDateTime AND th.EndDateTime
		LEFT JOIN dbo.PlaceActivityHistory pah ON pah.PlaceId = p.PlaceID AND @curDt BETWEEN pah.BeginDateTime AND pah.EndDateTime
		LEFT JOIN telegram.DialogSession ds ON ds.TelegramId = t.TelegramId
		WHERE t.TelegramId = @UserId;
END;
GO


USE [tipsv2dev]

GO

CREATE NONCLUSTERED INDEX [IX_ShareSchemeHistory_LoadShare] ON [dbo].[ShareSchemeHistory]
(
	[PlaceId] ASC,
	[EndDateTime] DESC
)
INCLUDE
(
	[PersonalShare],
	[BeginDateTime],
	[IsIndividual]
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = ON, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
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
	
	SELECT @ShareSchemeHistoryId = ssh.ShareSchemeHistoryId, @PlaceDisplayName = p.DisplayName, @PersonalShare = IIF(ssh.IsIndividual = 1, 100, ssh.PersonalShare),
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