SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE NONCLUSTERED INDEX [IX_AllOpenedTurns] ON [dbo].[EmployeeTurnHistory]
(
	[EndDateTime] DESC
)
INCLUDE
(
	[BeginDateTime]
);
GO




CREATE PROCEDURE [telegram].[Turn_ExitAll]
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @BeginDateTime datetime2(7) = sysdatetime();
		DECLARE @PrevEndDateTime datetime2(7) = DATEADD(ns, -100, @beginDateTime);

		CREATE TABLE #InTurn
		(
			EmployeeId int not null primary key clustered,
			SeqNum int not null
		);

		INSERT INTO #InTurn (EmployeeId, SeqNum)
		SELECT EmployeeId, SeqNum
			FROM dbo.EmployeeTurnHistory with (tablockx)
			WHERE @BeginDateTime BETWEEN BeginDateTime AND EndDateTime;

		UPDATE th SET EndDateTime = @PrevEndDateTime
			FROM dbo.EmployeeTurnHistory th
			INNER JOIN #InTurn it ON th.EmployeeId = it.EmployeeId AND th.SeqNum = it.SeqNum;

		INSERT INTO telegram.Notification (TelegramId, CreateDateTime, Message)
		SELECT u.TelegramId, @BeginDateTime, N'Произведён автоматический выход из смены'
			FROM dbo.Employee e
			INNER JOIN #InTurn it ON e.EmployeeId = it.EmployeeId
			INNER JOIN telegram.BotUser u ON u.Phone = e.Phone;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO
