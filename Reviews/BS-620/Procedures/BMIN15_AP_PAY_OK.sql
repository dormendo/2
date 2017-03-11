IF OBJECT_ID ('dbo.BMIN15_AP_PAY_OK', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN15_AP_PAY_OK;
GO

CREATE PROCEDURE dbo.BMIN15_AP_PAY_OK
    @zId int
AS
BEGIN
    SET NOCOUNT ON;
	
	DECLARE @client INT;
	DECLARE @summa INT;
	DECLARE @purse VARCHAR(24);
	DECLARE @idClcnt INT;
	DECLARE @typeId INT;
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;
	
		UPDATE i_kzap SET flag = 2, @client = client, @summa = summa, @purse = remm, @typeId = tag2
		FROM i_kzap
		WHERE id = @zId

		UPDATE bill SET ex_fl = 1, edate = SYSDATETIME()
			WHERE who IN (503, 504) AND tag1 = @zId;

		INSERT INTO log_cli (client, who, tag1, tag2, tag3, tag4)
			VALUES (@client, 220, @zId, @zId, @typeId, @purse);

		MERGE INTO client_cnt trg
			USING (SELECT @client, 220) AS src(client, who) ON trg.client = src.client AND trg.who = src.who
			WHEN NOT MATCHED THEN
				INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, 1, @summa)
			WHEN MATCHED THEN
				UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt + 1, summa = summa + 1;

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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN15_AP_PAY_OK",  conn))
--        {
--            cmd.AddIntParam("@zId", request.zId);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}