IF OBJECT_ID ('dbo.BMI_ALL_VYP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMI_ALL_VYP;
GO

CREATE PROCEDURE dbo.BMI_ALL_VYP
    @pm int,
    @dt date,
    @sumv int
AS
BEGIN
    SET NOCOUNT ON;

	WITH src AS
	(
		SELECT i.id
			FROM i_kzap i
			INNER JOIN client c ON c.id = i.client
			INNER JOIN valuta v ON v.id = c.valuta
			WHERE i.paymethod = @pm AND i.flag = 0 AND i.zdate < @dt AND i.summa / v.kurs < @sumv
	)
	UPDATE i_kzap SET flag=1
		WHERE id IN (SELECT id FROM src);

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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMI_ALL_VYP",  conn))
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