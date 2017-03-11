IF OBJECT_ID ('dbo.BMIN12_VR_ZAPR', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_VR_ZAPR;
GO

CREATE PROCEDURE dbo.BMIN12_VR_ZAPR
    @idzap int,
    @tag2 smallint OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE i_kzap SET @tag2 = tag2 = 1 - tag2 WHERE id = @idzap;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN12_VR_ZAPR",  conn))
--        {
--            cmd.AddIntParam("@idzap", request.idzap);

--            SqlParameter tag2Param = cmd.AddSmallIntParam("@tag2").Output();

--            await cmd.ExecuteNonQueryAsync();

--            short tag2 = tag2Param.GetInt16OrDefault();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}
