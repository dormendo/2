IF OBJECT_ID ('dbo.BMIN12_QIWI_PAY_OK', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_QIWI_PAY_OK;
GO

CREATE PROCEDURE dbo.BMIN12_QIWI_PAY_OK
    @id int,
    @trid bigint,
    @bl int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @cl INT;
	DECLARE @summa INT;
	DECLARE @r VARCHAR(24);
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		UPDATE i_kzap
			SET flag = 2, @cl = client, @summa = summa, @r = remm
			WHERE id = @id;

		INSERT INTO log_cli (client, who, tag1, tag2, tag3, tag4)
			VALUES (@cl, 207, @bl, @trid, @id, @r);

		MERGE client_cnt trg
			USING (SELECT @cl, 204) AS src(client) ON trg.client = src.client AND trg.who = src.who
			WHEN NOT MATCHED THEN
				INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, 1, @summa)
			WHEN MATCHED THEN
				UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt + 1, summa = summa + @summa;
		
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN12_QIWI_PAY_OK",  conn))
--        {
--            cmd.AddIntParam("@id", request.id);
--            cmd.AddBigIntParam("@trid", request.trid);
--            cmd.AddIntParam("@bl", request.bl);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}