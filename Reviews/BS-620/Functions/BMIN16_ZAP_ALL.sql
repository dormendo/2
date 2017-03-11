IF OBJECT_ID ('dbo.BMIN16_ZAP_ALL', 'IF') IS NOT NULL
    DROP FUNCTION dbo.BMIN16_ZAP_ALL;
GO


CREATE FUNCTION dbo.BMIN16_ZAP_ALL
(	
    @vid smallint,
    @md smallint,
    @pms dbo.SmallintId READONLY,
    @ds int,
    @aps dbo.SmallintId READONLY
)
RETURNS TABLE 
AS
RETURN 
(
	-- 03.03.2015  Добавлено AccentPay
	-- 17.04.2013
	-- 30.04.2013  BMIN13_ZAP_ALL потом удалить, добавлено для деньги онлайн
	-- 27.08.2013


	WITH src AS
	(
		SELECT z.id, z.client, z.kassa, z.zdate, z.edate, 0.01 * z.summa summa_orig, z.flag, z.remm, z.tag2,
				ISNULL(z.otkaz, '') o, ISNULL(u1.fl, 0) fl3, ISNULL(u2.fl, 0) fl_qiwi,
				z.paymethod pm, ISNULL(d1.rus, '????') pmstr, ISNULL(d2.rus, '????') sfl, c.valuta cl_vid,
				(SELECT SUM(0.01 * c1.summa) FROM client_cnt c1 WHERE c1.who BETWEEN 100 AND 199 AND c1.client = z.client) sum_dep,
				(SELECT SUM(0.01 * c2.summa) FROM client_cnt c2 WHERE c2.who BETWEEN 200 AND 299 AND c2.who <> 218 AND c2.client = z.client) sum_vyp,
				z.tag1, ISNULL(u3.v, 0) osb_client, ISNULL(u4.v, 0) osb_kassa, d3.rus type_ap, d3.prio group_ap, d3.tag1 val_ap,
				v.kurs kurs1, v2.kurs kurs2, CHARINDEX('на сумму ', z.remm, 1) i
			FROM I_KZAP z
			INNER JOIN client c ON c.id = z.client
			LEFT JOIN valuta v ON v.id = @vid
			LEFT JOIN valuta v2 ON c.valuta != 2 AND v.kurs IS NOT NULL AND v2.id = c.valuta
			LEFT JOIN uni_cli u1 ON u1.who = 2 AND u1.client = z.client
			LEFT JOIN uni_cli u2 ON u2.who = 3 AND u2.client = z.id
			LEFT JOIN uni_int u3 ON u3.who = 7001 AND u3.fid = z.client
			LEFT JOIN uni_int u4 ON u4.who = 7000 AND u4.fid = z.kassa
			LEFT JOIN DICT_MINI d1 ON d1.who = 4000 AND d1.fid = z.paymethod
			LEFT JOIN DICT_MINI d2 ON d2.who = 4100 AND d2.fid = z.flag
			LEFT JOIN DICT_MINI d3 ON d3.who = 400 AND d3.fid = z.tag2
			WHERE z.zdate > DATEADD(day, -@ds, CAST(SYSDATETIME() AS DATE)) AND
				z.paymethod IN (SELECT Id FROM @pms) AND
				(
					@md NOT IN (0, 1) OR @md IS NULL
					OR
					@md = 0 AND z.flag = 0
					OR
					@md = 1 AND z.flag != 3
				)
				AND (NOT EXISTS(SELECT * FROM @aps) OR d3.prio IN (SELECT Id FROM @aps))
	),
	src2 AS
	(
		SELECT id, client, kassa, zdate, edate, flag, remm, tag2, fl3, fl_qiwi, pm, pmstr, cl_vid, sum_dep, sum_vyp,
				tag1, osb_client, osb_kassa, type_ap, group_ap, val_ap, kurs1, kurs2,
				REPLACE(sfl, '%1', o) sfl,
				CASE
					WHEN i > 5 THEN CAST(REPLACE(SUBSTRING(remm, i + 9, CHARINDEX(' ', remm, i + 9 + 1) - i - 9), ',', '.') AS DECIMAL(15,2))
					ELSE summa_orig END summa_orig,
				IIF(kurs1 IS NOT NULL AND cl_vid != 2, 1, 0) can_calculate
			FROM src
	),
	src3 AS
	(
		SELECT client, kassa, id, zdate, edate, flag, remm, tag2, fl3, fl_qiwi, pm, pmstr, cl_vid,
				tag1, osb_client, osb_kassa, type_ap, group_ap, val_ap, kurs1, kurs2, sfl, summa_orig,
				IIF(can_calculate = 1, summa_orig * kurs1 / kurs2, summa_orig) summa,
				IIF(can_calculate = 1, sum_dep * kurs1 / kurs2, sum_dep) sum_dep,
				IIF(can_calculate = 1, sum_vyp * kurs1 / kurs2, sum_vyp) sum_vyp
			FROM src2
	)
	SELECT client, kassa, id, zdate, edate, summa, flag, remm, sfl, tag2, fl3, pm, pmstr, sum_dep, sum_vyp, cl_vid, summa_orig, fl_qiwi,
				tag1, osb_kassa, osb_client, type_ap, group_ap, val_ap
			FROM src3
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN16_ZAP_ALL",  conn))
--        {
--            cmd.AddSmallIntParam("@vid", request.vid);
--            cmd.AddSmallIntParam("@md", request.md);
--            cmd.AddVarCharParam("@pms", 255, request.pms);
--            cmd.AddIntParam("@ds", request.ds);
--            cmd.AddVarCharParam("@aps", 200, request.aps);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int client = dr.GetInt32OrDefault("CLIENT");
--                    int kassa = dr.GetInt32OrDefault("KASSA");
--                    int id = dr.GetInt32OrDefault("ID");
--                    DateTime zdate = dr.GetDateTimeOrNull("ZDATE");
--                    DateTime edate = dr.GetDateTimeOrNull("EDATE");
--                    decimal summa = dr.GetDecimalOrDefault("SUMMA");
--                    short flag = dr.GetInt16OrDefault("FLAG");
--                    string remm = dr.GetStringOrDefault("REMM");
--                    string sfl = dr.GetStringOrDefault("SFL");
--                    short tag2 = dr.GetInt16OrDefault("TAG2");
--                    short fl3 = dr.GetInt16OrDefault("FL3");
--                    short pm = dr.GetInt16OrDefault("PM");
--                    string pmstr = dr.GetStringOrDefault("PMSTR");
--                    decimal sumDep = dr.GetDecimalOrDefault("SUM_DEP");
--                    decimal sumVyp = dr.GetDecimalOrDefault("SUM_VYP");
--                    int clVid = dr.GetInt32OrDefault("CL_VID");
--                    decimal summaOrig = dr.GetDecimalOrDefault("SUMMA_ORIG");
--                    short flQiwi = dr.GetInt16OrDefault("FL_QIWI");
--                    int tag1 = dr.GetInt32OrDefault("TAG1");
--                    int osbKassa = dr.GetInt32OrDefault("OSB_KASSA");
--                    int osbClient = dr.GetInt32OrDefault("OSB_CLIENT");
--                    string typeAp = dr.GetStringOrDefault("TYPE_AP");
--                    short groupAp = dr.GetInt16OrDefault("GROUP_AP");
--                    short valAp = dr.GetInt16OrDefault("VAL_AP");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}