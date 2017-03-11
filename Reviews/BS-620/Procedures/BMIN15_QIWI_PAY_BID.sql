IF OBJECT_ID ('dbo.BMIN15_QIWI_PAY_BID', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN15_QIWI_PAY_BID;
GO

CREATE PROCEDURE dbo.BMIN15_QIWI_PAY_BID
    @client int,
    @zid int,
    @summa decimal(15,2),
    @id int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	INSERT INTO bill (client, who, zdate, ex_fl, summa, tag1) VALUES (@client, 708, SYSDATETIME(), 0, @summa, @zid);
	SET @id = SCOPE_IDENTITY();
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN15_QIWI_PAY_BID",  conn))
--        {
--            cmd.AddIntParam("@client", request.client);
--            cmd.AddIntParam("@zid", request.zid);
--            cmd.AddDecimalParam("@summa", 15, 2, request.summa);

--            SqlParameter idParam = cmd.AddIntParam("@id").Output();

--            await cmd.ExecuteNonQueryAsync();

--            int id = idParam.GetInt32OrDefault();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}