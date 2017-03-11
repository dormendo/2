IF OBJECT_ID ('dbo.BMIN12_BAN_CLIENT', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_BAN_CLIENT;
GO

CREATE PROCEDURE dbo.BMIN12_BAN_CLIENT
    @client int,
    @fl3 int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @ip VARCHAR(128) = CAST(CONNECTIONPROPERTY('client_net_address') AS VARCHAR(128));
	DECLARE @appName VARCHAR(128) = CAST(APP_NAME() AS VARCHAR(128));
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		IF (@fl3 = 1)
			MERGE uni_cli trg
				USING (SELECT 2, @client) AS src(who, client) ON trg.who = src.who AND trg.client = src.client
				WHEN NOT MATCHED THEN
					INSERT (who, client, fl) VALUES (src.who, src.client, 1)
				WHEN MATCHED THEN
					UPDATE SET fl = 1;
		ELSE
			UPDATE uni_cli SET fl = 0, d2 = SYSDATETIME()
			WHERE who = 2 AND client = @client;

		INSERT INTO log_uni (tb, who, prog, ip, tag1, tag2)
			VALUES (609, 609, @appName, @ip, @client, @fl3);
		
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN12_BAN_CLIENT",  conn))
--        {
--            cmd.AddIntParam("@client", request.client);
--            cmd.AddIntParam("@fl3", request.fl3);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}