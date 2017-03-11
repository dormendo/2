IF OBJECT_ID ('dbo.BMIN15_NORM_KLIENTS', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN15_NORM_KLIENTS;
GO

CREATE PROCEDURE dbo.BMIN15_NORM_KLIENTS
    @val smallint,
    @maxmin int
AS
BEGIN
    SET NOCOUNT ON;

	WITH src AS
	(
		SELECT
				c.id,
				c.name1,
				c.name2,
				0.01 * c.balans balans,
				0.01 * c.maxsum maxsum,
				IIF(c.fl1 = 1, 'Да', '') fl1,
				IIF(c.fl2 = 1, 'Да', '') fl2,
				c.city,
				ISNULL(s1.sum_dep, 0) sum_dep,
				ISNULL(s2.sum_vyp, 0) sum_vyp,
				ISNULL(s3.sum_p, 0) sum_p,
				s3.cnt_p,
				ISNULL(s4.sum_v, 0) sum_v,
				s4.cnt_v

			FROM CLIENT c
			OUTER APPLY (SELECT 0.01 * SUM(cc.summa) sum_dep FROM client_cnt cc WHERE cc.client = c.id AND cc.who BETWEEN 100 AND 190) s1
			OUTER APPLY (SELECT 0.01 * SUM(cc.summa) sum_vyp FROM client_cnt cc WHERE cc.client = c.id AND cc.who BETWEEN 200 AND 299) s2
			OUTER APPLY (SELECT TOP(1) 0.01 * cc.summa sum_p, ISNULL(cc.cnt, 0) cnt_p FROM client_cnt cc WHERE cc.client = c.id AND cc.who = 20) s3
			OUTER APPLY (SELECT TOP(1) 0.01 * cc.summa sum_v, ISNULL(cc.cnt, 0) cnt_v FROM client_cnt cc WHERE cc.client = c.id AND cc.who = 21) s4
			WHERE c.valuta = @val AND c.maxsum < @maxmin
	)
	SELECT id, balans, maxsum, fl1, fl2, sum_dep, sum_vyp, sum_p, sum_v, sum_p - sum_v sum_r,
			IIF(sum_p = 0, 0, 100.0000000 * (sum_p - sum_v) / sum_p) prc,
			cnt_p, cnt_v, city, name1 + ' ' + name2 fio
		FROM src
		WHERE sum_p - sum_v > 0
		ORDER BY name1, name2;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN15_NORM_KLIENTS",  conn))
--        {
--            cmd.AddSmallIntParam("@val", request.val);
--            cmd.AddIntParam("@maxmin", request.maxmin);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int id = dr.GetInt32OrDefault("ID");
--                    decimal balans = dr.GetDecimalOrDefault("BALANS");
--                    decimal maxsum = dr.GetDecimalOrDefault("MAXSUM");
--                    string fl1 = dr.GetStringOrDefault("FL1");
--                    string fl2 = dr.GetStringOrDefault("FL2");
--                    decimal sumDep = dr.GetDecimalOrDefault("SUM_DEP");
--                    decimal sumVyp = dr.GetDecimalOrDefault("SUM_VYP");
--                    decimal sumP = dr.GetDecimalOrDefault("SUM_P");
--                    decimal sumV = dr.GetDecimalOrDefault("SUM_V");
--                    decimal sumR = dr.GetDecimalOrDefault("SUM_R");
--                    decimal prc = dr.GetDecimalOrDefault("PRC");
--                    int cntP = dr.GetInt32OrDefault("CNT_P");
--                    int cntV = dr.GetInt32OrDefault("CNT_V");
--                    string city = dr.GetStringOrDefault("CITY");
--                    string fio = dr.GetStringOrDefault("FIO");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}