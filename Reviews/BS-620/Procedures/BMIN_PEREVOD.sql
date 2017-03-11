IF OBJECT_ID ('dbo.BMIN_PEREVOD', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN_PEREVOD;
GO

CREATE PROCEDURE dbo.BMIN_PEREVOD
    @kl int
AS
BEGIN
    SET NOCOUNT ON;

	SELECT
			who,
			CASE who WHEN 502 THEN 'Из Webmoney' WHEN 503 THEN 'Депозит с кассы' WHEN 506 THEN 'ОСМП' ELSE '????' END remm,
			SUM(summa) / 100.00 summa,
			COUNT(who) cnt,
			MIN(dt) min_dt,
			MAX(dt) max_dt
		FROM log_cli
		WHERE client = @kl AND who IN (502, 503, 506)
		GROUP BY who
	
	UNION ALL
	SELECT 998, 'Выплата в кассу', SUM(i.summa) / 100.00, COUNT(i.id), MIN(i.dt), MAX(i.dt)
		FROM kass_log i
		WHERE i.tag1 = @kl AND i.who = 25
		HAVING COUNT(i.id) > 0

	UNION ALL
	SELECT 999, 'В WebMoney', SUM(i.summa) / 100.00, COUNT(i.id), MIN(i.zdate), MAX(i.zdate)
		FROM i_kzap i
		WHERE i.client = @kl AND i.paymethod = 2 AND i.flag = 2
		HAVING COUNT(i.id) > 0;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN_PEREVOD",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int who = dr.GetInt32OrDefault("WHO");
--                    string remm = dr.GetStringOrDefault("REMM");
--                    double summa = dr.GetDoubleOrDefault("SUMMA");
--                    int cnt = dr.GetInt32OrDefault("CNT");
--                    DateTime minDt = dr.GetDateTimeOrNull("MIN_DT");
--                    DateTime maxDt = dr.GetDateTimeOrNull("MAX_DT");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}