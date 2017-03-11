IF OBJECT_ID ('dbo.BMIN12_CB_QIWI', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_CB_QIWI;
GO

CREATE PROCEDURE dbo.BMIN12_CB_QIWI
    @id int,
    @flQiwi int
AS
BEGIN
    SET NOCOUNT ON;
    IF (@flQiwi = 1)
		MERGE INTO uni_cli trg
			USING (SELECT 3, @id) AS src(who, client) ON trg.who = src.who AND trg.client = src.client
			WHEN NOT MATCHED THEN
				INSERT (who, client, fl) VALUES (src.who, src.client, 1)
			WHEN MATCHED THEN
				UPDATE SET fl = 1;
    ELSE
		UPDATE uni_cli SET fl = 0, d2 = SYSDATETIME() WHERE who = 3 AND client = @id;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN12_CB_QIWI",  conn))
--        {
--            cmd.AddIntParam("@id", request.id);
--            cmd.AddIntParam("@flQiwi", request.flQiwi);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}