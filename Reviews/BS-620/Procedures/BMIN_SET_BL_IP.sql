IF OBJECT_ID ('dbo.BMIN_SET_BL_IP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN_SET_BL_IP;
GO

CREATE PROCEDURE dbo.BMIN_SET_BL_IP
    @ip int,
    @allCl smallint
AS
BEGIN
    SET NOCOUNT ON;
	
	IF (@allCl = 1)
	BEGIN
		DECLARE @count INT, @today date = CAST(SYSDATETIME() AS DATE);
		DECLARE @outerTranCount int;

		BEGIN TRY
			SET @outerTranCount = @@TRANCOUNT;
			IF @outerTranCount = 0
				BEGIN TRANSACTION;
		
			SELECT @count = COUNT(*)
				FROM log_cli
				WHERE who = 101 AND dt > DATEADD(day, -900, @today) AND tag1 = @ip;

			MERGE ip_black trg
				USING (SELECT @ip) AS src(ip) ON trg.ip = src.ip
				WHEN NOT MATCHED THEN
					INSERT (ip, cnt) VALUES (@ip, @count)
				WHEN MATCHED THEN
					UPDATE SET cnt = cnt + @count;

			UPDATE client SET
					fl1 = 1,
					fl2 = 1,
					maxsum = CASE valuta
						WHEN  1 THEN 1000
						WHEN  2 THEN 30000
						WHEN  3 THEN 2000000
						WHEN  4 THEN 5000
						WHEN  5 THEN 1000
						WHEN  6 THEN 150000
						WHEN  7 THEN 10000
						WHEN  8 THEN 40000
						WHEN  9 THEN 10000
						WHEN 10 THEN 400000
						WHEN 11 THEN 1000
						ELSE 10000 END
				WHERE id IN (SELECT client FROM log_cli WHERE who = 101 AND dt > DATEADD(day, -900, @today) AND tag1 = @ip);

			IF @outerTranCount = 0
				COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
			IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
				ROLLBACK TRANSACTION;
			THROW;
		END CATCH;
	END
	ELSE
	BEGIN
		MERGE ip_black trg
			USING (SELECT @ip) AS src(ip) ON trg.ip = src.ip
			WHEN NOT MATCHED THEN
				INSERT (ip) VALUES (@ip);
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN_SET_BL_IP",  conn))
--        {
--            cmd.AddIntParam("@ip", request.ip);
--            cmd.AddSmallIntParam("@allCl", request.allCl);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}