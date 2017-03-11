IF OBJECT_ID ('dbo.BMIN_CANCEL_DEP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN_CANCEL_DEP;
GO

CREATE PROCEDURE dbo.BMIN_CANCEL_DEP
    @kassa int,
    @client int,
    @sum2 int,
    @st varchar(48) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @bal BIGINT;
	DECLARE @kassirId INTEGER;
	DECLARE @cnt INTEGER;
	DECLARE @id INTEGER;
	DECLARE @today DATE = CAST(SYSDATETIME() AS DATE);
	DECLARE @remm AS VARCHAR(250);
	DECLARE @outerTranCount int;

	SET @st = 'Не найдено';

	-- Изменение в DATEADD с 10 дней на 7 согласовано с Виктором
	SELECT @cnt = count(*)
		FROM kass_log l
		WHERE l.kassa = @kassa AND l.tag1 = @client AND l.dt > DATEADD(day, -7, @today) AND l.summa = @sum2 AND l.who = 5;

	IF (@cnt > 0)
	BEGIN
		IF (@cnt > 1)
			SET @st = 'Найдено: ' + CAST(@cnt AS VARCHAR(10)) + ' , отменен 1 последний депозит';
		ELSE
			SET @st = 'Отменен';

		SET @remm = 'Отмена депозита по клиенту: ' + CAST(@client AS VARCHAR(10)) +
			', касса: ' + CAST(@kassa AS VARCHAR(10)) +
			', сумма: ' + CAST(CAST(@sum2 / 100.00 AS DECIMAL(15, 2)) AS VARCHAR);

		BEGIN TRY
			SET @outerTranCount = @@TRANCOUNT;
			IF @outerTranCount = 0
				BEGIN TRANSACTION;

			INSERT INTO log_oper (who, remm) VALUES (2, @remm);

			WITH src AS
			(
				SELECT TOP(1) id
					FROM kvit
					WHERE kassa = @kassa AND client = @client AND kdate > DATEADD(day, -7, @today) AND summa = @sum2 AND fl = 1
					ORDER BY id DESC
			)
			DELETE FROM kvit
				WHERE id IN (SELECT id FROM src);

			WITH src AS
			(
				SELECT TOP(1) id
					FROM kass_log
					WHERE kassa = @kassa AND tag1 = @client AND dt > DATEADD(day, -7, @today) AND summa = @sum2 AND who = 5
					ORDER BY id DESC
			)
			UPDATE kass_log SET who = 1021, @kassirId = kassir
				WHERE id IN (SELECT id FROM src);

			UPDATE client SET balans = balans - @sum2
				WHERE id = @client;

			UPDATE kassa SET @bal = balans = balans - @sum2
				WHERE id = @kassa;

			WITH src AS
			(
				SELECT TOP(1) id
					FROM log_cli
					WHERE client = @client AND who = 503 AND summa = @sum2 AND dt > DATEADD(day, -7, @today)
					ORDER BY dt DESC
			)
			DELETE FROM log_cli
				WHERE id IN (SELECT id FROM src);

			INSERT INTO kass_log (kassa, who, summa, bal, tag1, tag2, kassir)
				VALUES (@kassa, 1020, -@sum2, @bal, 0, @client, @kassirId);

			IF @outerTranCount = 0
				COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
			IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
				ROLLBACK TRANSACTION;
			THROW;
		END CATCH;
	END;
END;
GO



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN_CANCEL_DEP",  conn))
--        {
--            cmd.AddIntParam("@kassa", request.kassa);
--            cmd.AddIntParam("@client", request.client);
--            cmd.AddIntParam("@sum2", request.sum2);

--            SqlParameter stParam = cmd.AddVarCharParam("@st", 48).Output();

--            await cmd.ExecuteNonQueryAsync();

--            string st = stParam.Value.ToString();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}