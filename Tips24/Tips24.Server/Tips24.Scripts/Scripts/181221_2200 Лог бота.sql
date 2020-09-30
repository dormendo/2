/****** Object:  StoredProcedure [admin].[CreateEmployee]    Script Date: 18.12.2018 19:43:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE telegram.MessageLog
(
	Id bigint not null identity(1,1) primary key clustered,
	InputMessage bit not null,
	TelegramId bigint not null,
	Phone char(10) null,
	EmployeeId int null,
	MessageDateTime datetime2(7) not null,
	MessageData nvarchar(max) not null
) WITH (DATA_COMPRESSION = PAGE);
GO


CREATE OR ALTER PROCEDURE telegram.WriteMessageLog
	@InputMessage bit,
	@TelegramId bigint,
	@Phone char(10),
	@EmployeeId int,
	@MessageData nvarchar(max)
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO telegram.MessageLog (InputMessage, TelegramId, Phone, EmployeeId, MessageDateTime, MessageData)
		VALUES (@InputMessage, @TelegramId, @Phone, @EmployeeId, sysdatetime(), @MessageData);
END;
GO

UPDATE telegram.BotUser SET QrCodeFileId = NULL;
GO

CREATE TABLE telegram.Tmp_BotUser
(
	TelegramId bigint NOT NULL,
	Phone char(10) NOT NULL,
	QrCodeFileId varchar(64) NULL,
	QrCodeStringHash binary(40) NULL,
	QrCodeDateTime datetime2(7) NULL,
	LastBalanceLogId bigint NULL
);
GO
DECLARE @v sql_variant 
SET @v = N'Id пользователя в телеграм'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'telegram', N'TABLE', N'Tmp_BotUser', N'COLUMN', N'TelegramId'
GO
DECLARE @v sql_variant 
SET @v = N'Телефон пользователя'
EXECUTE sp_addextendedproperty N'MS_Description', @v, N'SCHEMA', N'telegram', N'TABLE', N'Tmp_BotUser', N'COLUMN', N'Phone'
GO
IF EXISTS(SELECT * FROM telegram.BotUser)
	 EXEC('INSERT INTO telegram.Tmp_BotUser (TelegramId, Phone, QrCodeFileId, LastBalanceLogId)
		SELECT TelegramId, Phone, QrCodeFileId, LastBalanceLogId FROM telegram.BotUser WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE telegram.BotUser
GO
EXECUTE sp_rename N'telegram.Tmp_BotUser', N'BotUser', 'OBJECT' 
GO
ALTER TABLE telegram.BotUser ADD CONSTRAINT PK_TelegramUser PRIMARY KEY CLUSTERED 
(
	TelegramId
);
GO

ALTER PROCEDURE [telegram].[UpdateQrCodeFileId]
	@UserId bigint,
	@QrCodeFileId varchar(64),
	@QrCodeStringHash binary(40)
AS
BEGIN
	UPDATE telegram.BotUser SET QrCodeFileId = @QrCodeFileId, QrCodeStringHash = @QrCodeStringHash, QrCodeDateTime = sysdatetime() WHERE TelegramId = @UserId;
END;
GO

ALTER PROCEDURE [telegram].[GetUserRecordByUserId]
	@UserId bigint
AS
BEGIN
	DECLARE @curDt datetime2(7) = SYSDATETIME();

	SELECT e.EmployeeId, e.Phone, pd.FirstName, pd.LastName, e.Balance, em.PlaceId, p.DisplayName PlaceName, p.Address PlaceAddress, p.City PlaceCity,
			em.GroupId, g.Name GroupName, em.IsFired, em.IsManager, em.IsOwner, t.QrCodeFileId, t.QrCodeStringHash, t.QrCodeDateTime,
			yks.Step YksStep, yks.UserLogin YksUserLogin, yks.Amount YksAmount, yks.ProviderId YksProviderId
		FROM telegram.BotUser t
		INNER JOIN dbo.Employee e ON e.Phone = t.Phone
		INNER JOIN dbo.EmployeeMembershipHistory em ON e.EmployeeId = em.EmployeeId AND @curDt BETWEEN em.BeginDateTime AND em.EndDateTime
		INNER JOIN dbo.Place p ON p.PlaceId = em.PlaceId
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
		INNER JOIN dbo.EmployeePersonalDataHistory pd ON pd.EmployeeId = e.EmployeeId AND @curDt BETWEEN pd.BeginDateTime AND pd.EndDateTime
		LEFT JOIN telegram.YandexKassaSession yks ON yks.TelegramId = t.TelegramId
		WHERE t.TelegramId = @UserId;
END;
GO