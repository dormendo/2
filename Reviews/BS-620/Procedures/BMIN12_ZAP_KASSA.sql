IF OBJECT_ID ('dbo.BMIN12_ZAP_KASSA', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_ZAP_KASSA;
GO

CREATE PROCEDURE dbo.BMIN12_ZAP_KASSA
    @vid smallint,
    @md smallint,
	@client int = NULL
AS
BEGIN
    SET NOCOUNT ON;

	SELECT z.client, z.kassa, z.id, z.zdate, z.edate,
			-- Логика согласована с Виктором
			CASE
				WHEN v.kurs IS NULL OR c.valuta = 2 THEN 0.01 * z.summa
				ELSE 0.01 * v.kurs * z.summa / v2.kurs END summa,
			z.flag,
			z.remm,
			CASE z.flag 
				WHEN 0 THEN 'в обработке'
				WHEN 1 THEN 'разрешен на выплату'
				WHEN 2 THEN 'выплачен'
				WHEN 3 THEN 'отказ клиента'
				WHEN 4 THEN '????'
				WHEN 5 THEN 'отклонен администрацией из-за ' + z.otkaz
				ELSE 'Error' END sfl,
			z.tag2, ISNULL(u.fl, 0) fl3
		FROM I_KZAP z
		LEFT JOIN uni_cli u on u.who = 2 AND u.client = z.client
		LEFT JOIN valuta v ON v.id = @vid
		LEFT JOIN client c ON c.id = z.client AND v.kurs IS NOT NULL
		LEFT JOIN valuta v2 ON v2.id = c.valuta AND c.valuta != 2
		WHERE z.PAYMETHOD = 1
			AND
			(
				@md NOT IN (0, 1) OR @md IS NULL
				OR
				@md = 0 AND z.flag = 0
				OR
				@md = 1 AND z.flag != 3
			)
			AND (@client IS NULL OR z.client = @client)
		OPTION (MAXDOP 1, RECOMPILE);
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN12_ZAP_KASSA",  conn))
--        {
--            cmd.AddSmallIntParam("@vid", request.vid);
--            cmd.AddSmallIntParam("@md", request.md);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int client = dr.GetInt32OrDefault("CLIENT");
--                    int kassa = dr.GetInt32OrDefault("KASSA");
--                    int id = dr.GetInt32OrDefault("ID");
--                    DateTime zdate = dr.GetDateTimeOrNull("ZDATE");
--                    DateTime edate = dr.GetDateTimeOrNull("EDATE");
--                    double summa = dr.GetDoubleOrDefault("SUMMA");
--                    short flag = dr.GetInt16OrDefault("FLAG");
--                    string remm = dr.GetStringOrDefault("REMM");
--                    string sfl = dr.GetStringOrDefault("SFL");
--                    short tag2 = dr.GetInt16OrDefault("TAG2");
--                    short fl3 = dr.GetInt16OrDefault("FL3");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}