IF OBJECT_ID ('dbo.BMIN16_CLIENTS', 'IF') IS NOT NULL
    DROP FUNCTION dbo.BMIN16_CLIENTS;
GO


CREATE FUNCTION dbo.BMIN16_CLIENTS
(	
    @mode smallint,
    @val2 smallint,
	@ids dbo.IntId READONLY,
    @filCli varchar(1000),
    @name1 varchar(250),
    @name2 varchar(250),
    @d1 date,
    @d2 date
)
RETURNS TABLE 
AS
RETURN 
(
	WITH src AS
	(
		SELECT c.id, ISNULL(0.01 * c.balans, 0) balans, v.soc, 0.01 * c.maxsum maxsum, c.evn env, c.fl1, v.kurs, v2.kurs kurs2, c.tel,
				ISNULL(0.01 * s1.sum_dep, 0) sum_dep, ISNULL(0.01 * s2.sum_vyp, 0) sum_vyp,
				ISNULL(0.01 * s3.sum_p, 0) sum_p, ISNULL(s3.cnt_p, 0) cnt_p,
				ISNULL(0.01 * s4.sum_v, 0) sum_v, ISNULL(s4.cnt_v, 0) cnt_v,
				ISNULL(0.01 * s5.tt_p, 0) tt_p, ISNULL(s5.tt_cnt_p, 0) tt_cnt_p,
				ISNULL(0.01 * s6.tt_v, 0) tt_v, ISNULL(s6.tt_cnt_v, 0) tt_cnt_v
			FROM client c
			INNER JOIN valuta v ON c.valuta = v.id
			LEFT JOIN valuta v2 ON v2.id = @val2
			OUTER APPLY (SELECT SUM(cc.summa) sum_dep FROM client_cnt cc WHERE cc.client = c.id AND cc.who BETWEEN 100 AND 190) s1
			OUTER APPLY (SELECT SUM(cc.summa) sum_vyp FROM client_cnt cc WHERE cc.client = c.id AND cc.who BETWEEN 200 AND 299) s2
			OUTER APPLY (SELECT TOP(1) cc.summa sum_p, cc.cnt cnt_p FROM client_cnt cc WHERE cc.client = c.id AND cc.who = 20) s3
			OUTER APPLY (SELECT TOP(1) cc.summa sum_v, cc.cnt cnt_v FROM client_cnt cc WHERE cc.client = c.id AND cc.who = 21) s4
			OUTER APPLY (SELECT TOP(1) cc.summa tt_p, cc.cnt tt_cnt_p FROM client_cnt cc WHERE cc.client = c.id AND cc.who = 30) s5
			OUTER APPLY (SELECT TOP(1) cc.summa tt_v, cc.cnt tt_cnt_v FROM client_cnt cc WHERE cc.client = c.id AND cc.who = 31) s6
			WHERE c.dtreg BETWEEN @d1 AND @d2
				AND
				(
					@mode NOT IN (1, 2, 3, 4, 5, 6, 7)
					OR
					@mode = 1 AND EXISTS(SELECT * FROM client_cnt cc WHERE cc.client = c.id AND cc.who IN (100,101,104,171,199))
					OR
					@mode = 2 AND c.id IN (SELECT Id FROM @ids)
					OR
					@mode = 3 AND c.email = @filCli
					OR
					@mode = 4 AND c.id IN (SELECT client FROM LOGI.dbo.log_login WHERE who = 3 AND ip IN (SELECT Id FROM @ids))
					OR
					@mode = 5 AND EXISTS(SELECT * FROM log_cli l WHERE l.client = c.id AND l.who = 508 AND l.tag2 = @filCli)
					OR
					@mode = 6 AND c.city = @filCli
					OR
					@mode = 7 AND c.id IN (SELECT fid FROM PSW.dbo.psw WHERE psw = @filCli AND who = 0)
				)
				AND
				(
					@name1 = '' AND @name2 = ''
					OR
					@name1 != '' AND c.name1 LIKE '%' + @name1 + '%'
					OR
					@name2 != '' AND c.name2 LIKE '%' + name2 + '%'
				)
	),
	src2 AS
	(
		SELECT id, soc, env, fl1, tel, cnt_p, cnt_v, tt_p, tt_cnt_p, tt_v, tt_cnt_v,
				IIF(sum_p = 0, 0, 100.0 * (sum_p - sum_v) / sum_p) prc,
				IIF (@val2 > 0, kurs2 * balans / kurs, balans) balans,
				IIF (@val2 > 0, kurs2 * sum_dep / kurs, sum_dep) sum_dep,
				IIF (@val2 > 0, kurs2 * sum_vyp / kurs, sum_vyp) sum_vyp,
				IIF (@val2 > 0, kurs2 * sum_p / kurs, sum_p) sum_p,
				IIF (@val2 > 0, kurs2 * sum_v / kurs, sum_v) sum_v,
				IIF (@val2 > 0, kurs2 * (sum_p - sum_v) / kurs, (sum_p - sum_v)) sum_r,
				IIF (@val2 > 0, kurs2 * maxsum / kurs, maxsum) maxsum
			FROM src
	)
	SELECT id, balans, sum_dep, sum_vyp, 0.0 texdep, maxsum, sum_p, sum_v, sum_r, prc, cnt_p, cnt_v, tt_p, tt_v, tt_cnt_p, tt_cnt_v, soc, env, fl1, tel
		FROM src2
)
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN16_CLIENTS",  conn))
--        {
--            cmd.AddSmallIntParam("@mode", request.mode);
--            cmd.AddSmallIntParam("@val2", request.val2);
--            cmd.AddVarCharParam("@filCli", 1000, request.filCli);
--            cmd.AddVarCharParam("@name1", 250, request.name1);
--            cmd.AddVarCharParam("@name2", 250, request.name2);
--            cmd.AddDateParam("@d1", request.d1);
--            cmd.AddDateParam("@d2", request.d2);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int id = dr.GetInt32OrDefault("ID");
--                    double balans = dr.GetDoubleOrDefault("BALANS");
--                    double sumDep = dr.GetDoubleOrDefault("SUM_DEP");
--                    double sumVyp = dr.GetDoubleOrDefault("SUM_VYP");
--                    double texdep = dr.GetDoubleOrDefault("TEXDEP");
--                    double maxsum = dr.GetDoubleOrDefault("MAXSUM");
--                    double sumP = dr.GetDoubleOrDefault("SUM_P");
--                    double sumV = dr.GetDoubleOrDefault("SUM_V");
--                    double sumR = dr.GetDoubleOrDefault("SUM_R");
--                    double prc = dr.GetDoubleOrDefault("PRC");
--                    int cntP = dr.GetInt32OrDefault("CNT_P");
--                    int cntV = dr.GetInt32OrDefault("CNT_V");
--                    double ttP = dr.GetDoubleOrDefault("TT_P");
--                    double ttV = dr.GetDoubleOrDefault("TT_V");
--                    int ttCntP = dr.GetInt32OrDefault("TT_CNT_P");
--                    int ttCntV = dr.GetInt32OrDefault("TT_CNT_V");
--                    string soc = dr.GetStringOrDefault("SOC");
--                    int env = dr.GetInt32OrDefault("ENV");
--                    int fl1 = dr.GetInt32OrDefault("FL1");
--                    string tel = dr.GetStringOrDefault("TEL");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}