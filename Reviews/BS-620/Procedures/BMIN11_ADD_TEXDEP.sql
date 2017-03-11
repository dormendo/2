IF OBJECT_ID ('dbo.BMIN11_ADD_TEXDEP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN11_ADD_TEXDEP;
GO

CREATE PROCEDURE dbo.BMIN11_ADD_TEXDEP
    @kl int,
    @remm varchar(250),
    @summa int,
    @login varchar(64)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @bal BIGINT;
	DECLARE @idd INT;
	DECLARE @logId INT;

	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		UPDATE client
			SET @bal = balans = balans + @summa
			WHERE id = @kl;

		SELECT TOP(1) @idd = id FROM uni_d with(serializable) WHERE who = 200 AND v = @remm;

		IF (@idd IS NULL)
		BEGIN
			INSERT INTO uni_d (who, v) VALUES (200, @remm);
			SET @idd = SCOPE_IDENTITY();
		END;

		INSERT INTO log_cli (who, client, summa, bal, tag1)
			VALUES (999, @kl, @summa, @bal, @idd);
		SET @logId = SCOPE_IDENTITY();

		MERGE client_cnt trg
			USING (SELECT @kl, 199) AS src(client, who) ON trg.client = src.client AND trg.who = src.who
			WHEN NOT MATCHED THEN
				INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, 1, @summa)
			WHEN MATCHED THEN
				UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt + 1, summa = summa + @summa;

		DECLARE @ip VARCHAR(24) = CAST(CONNECTIONPROPERTY('client_net_address') AS VARCHAR(24));
		DECLARE @appName VARCHAR(128) = CAST(APP_NAME() AS VARCHAR(128));

		INSERT INTO log_uni (tb, who, prog, ip, tag1, tag2)
			VALUES (55, 55, '(' + @login + ') ' + @appName, @ip, @logId, @kl);
		
		IF @outerTranCount = 0
			COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN11_ADD_TEXDEP",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);
--            cmd.AddVarCharParam("@remm", 250, request.remm);
--            cmd.AddIntParam("@summa", request.summa);
--            cmd.AddVarCharParam("@login", 64, request.login);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}