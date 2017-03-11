IF OBJECT_ID ('dbo.BMI_GET_LIM', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMI_GET_LIM;
GO

CREATE PROCEDURE dbo.BMI_GET_LIM
    @kl int,
    @val2 int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @v1Kurs FLOAT;
	DECLARE @v2Kurs FLOAT;
	DECLARE @sid INT;
	DECLARE @bLim BIGINT;

	SELECT @v1Kurs = v.kurs
		FROM valuta v
		INNER JOIN client c ON c.valuta = v.id
		WHERE c.id = @kl;

	SELECT @v2Kurs = v.kurs FROM valuta v WHERE v.id = @val2;
	
	WITH src AS
	(
		SELECT l.id, l.sport sid, IIF(l.sport = 0, 'Он-лайн', s.rus) sport, CAST(l.summa AS FLOAT) / 100.0 lim
			FROM cl_limit l
			LEFT JOIN sport s ON s.id = l.sport
			WHERE l.cl = @kl
	)
	SELECT id, sport, IIF(@val2 > 0, @v2Kurs / @v1Kurs * lim, lim) lim
		FROM src
		ORDER BY sid;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMI_GET_LIM",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);
--            cmd.AddIntParam("@val2", request.val2);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int id = dr.GetInt32OrDefault("ID");
--                    string sport = dr.GetStringOrDefault("SPORT");
--                    double lim = dr.GetDoubleOrDefault("LIM");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}