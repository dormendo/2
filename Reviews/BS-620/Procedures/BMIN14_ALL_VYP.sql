IF OBJECT_ID ('dbo.BMIN14_ALL_VYP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN14_ALL_VYP;
GO

CREATE PROCEDURE dbo.BMIN14_ALL_VYP
    @pm int,
    @dt date,
    @sumv int
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE i_kzap SET flag = 1
		FROM i_kzap z
		INNER JOIN client c ON c.id = z.client
		INNER JOIN valuta v ON v.id = c.valuta
		WHERE z.paymethod = @pm AND z.flag = 0 AND z.zdate < @dt AND z.summa / v.kurs < @sumv AND
			NOT EXISTS (SELECT * FROM uni_cli u2 WHERE u2.who = 3 AND u2.client = z.id AND u2.fl = 1);
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN14_ALL_VYP",  conn))
--        {
--            cmd.AddIntParam("@pm", request.pm);
--            cmd.AddDateParam("@dt", request.dt);
--            cmd.AddIntParam("@sumv", request.sumv);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}