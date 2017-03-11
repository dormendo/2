IF OBJECT_ID ('dbo.BMIN12_LOGS_PAYS', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_LOGS_PAYS;
GO

CREATE PROCEDURE dbo.BMIN12_LOGS_PAYS
    @kli int,
    @whos dbo.IntId READONLY
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @cdate DATETIME2(7);
    DECLARE @rem VARCHAR(1000);
    DECLARE @summa DECIMAL(15,2);

	DECLARE @soc VARCHAR(10);

	SELECT @soc = v.soc
		FROM valuta v
		INNER JOIN client c ON v.id = c.valuta
		WHERE c.id = @kli;

	WITH src AS
	(
		SELECT l.id lid, l.dt cdate, l.who who, ISNULL(l.summa, 0) tSum, ISNULL(l.tag1, 0) tag1,
				CAST(ISNULL(l.tag1, 0) AS binary(4)) tag1_bin, ISNULL(l.tag2, 0) tag2, l.tag3 i, l.tag4 wmr, d.rus rem
			FROM log_cli l
			INNER JOIN dict_mini d ON d.who = 201 AND d.fid = l.who
			WHERE L.client = @kli AND l.who IN (SELECT Id FROM @whos)
	),
	src2 AS
	(
		SELECT lid, cdate, who, tSum, 0.01 * tSum summa, tag1, tag2, i, wmr, rem,
				CASE
					WHEN who IN (702, 703, 707)
						THEN CONCAT(
							CAST(SUBSTRING(tag1_bin, 1, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 2, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 3, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 4, 1) AS tinyint)
							)
					ELSE NULL END ip
			FROM src
	),
	src3 AS
	(
		SELECT s.lid, s.cdate, s.who, s.summa, s.tag1, s.tag2, s.i, s.wmr, 
				CASE
					WHEN s.who IN (530, 999) THEN u.v
					WHEN s.who = 550 THEN s.wmr
					WHEN s.who IN (702, 703, 707) AND -s.tSum - s.i != 0
						THEN ' . Комиссия за перевод ' + CAST(CAST(0.01 * (-s.tSum - s.i) AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + @soc
					WHEN s.who = 503 THEN ISNULL('№' + CAST(k.num AS VARCHAR) + ' (' + k.adres + ')', '(Пункт закрыт)')
					WHEN s.who = 309 THEN CAST(c.tag1 AS VARCHAR) 
					WHEN s.who IN (520, 720, 220) THEN ISNULL(dm2.rus, '?????')
					ELSE '' END s,
				CASE
					WHEN s.who = 700 THEN ISNULL(ik1.summa, 0)
					ELSE tSum END tSum,
				CASE
					WHEN s.who IN (702, 703, 707) THEN ik2.flag
					ELSE 0 END flzp,
				CASE
					WHEN s.who IN (702, 703, 707)
						THEN REPLACE(REPLACE(s.rem,
							'%wmid%', CAST(s.tag2 AS VARCHAR)),
							'%sd%', CAST(CAST(0.01 * s.i AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + @soc)
					WHEN s.who IN (101, 120, 130) THEN REPLACE(s.rem, '%ip%', s.ip)
					WHEN s.who = 207 AND s.wmr IS NOT NULL THEN REPLACE(s.rem, '%4%', s.wmr)
					WHEN s.who = 517 THEN REPLACE(s.rem, '%d%', ISNULL(dm1.rus, '???'))
					WHEN s.who IN (520, 720, 220)
						THEN REPLACE(REPLACE(@rem, '%4%', ISNULL(s.wmr, '?????')), '%D3%', ISNULL(dm2.rus, '?????'))
					ELSE s.rem END rem
			FROM src2 s
			LEFT JOIN uni_d u ON u.id = s.tag1 AND s.who IN (530, 999)
			LEFT JOIN i_kzap ik1 ON ik1.id = s.tag2 AND s.who = 700
			LEFT JOIN i_kzap ik2 ON ik2.id = s.tag1 AND s.who IN (702, 703, 707)
			LEFT JOIN kassa k ON k.id = s.tag1 AND s.who = 503
			LEFT JOIN card2 c ON c.id = s.tag1 AND s.who = 309
			LEFT JOIN uni_dict ud ON ud.id = s.tag2 AND s.who = 517
			LEFT JOIN dict_mini dm1 ON dm1.fid = CAST(ud.ori AS INT) AND dm1.who = 700
			LEFT JOIN dict_mini dm2 ON dm2.fid = s.i AND dm2.who = 400 AND s.who IN (520,720,220)
	),
	src4 AS
	(
		SELECT lid, cdate, who, summa, tag1, tag2, i, wmr, s, tSum, flzp,
				CASE
					WHEN who IN (702, 703, 707) AND wmr IS NOT NULL
						THEN REPLACE(REPLACE(rem, '%K%', s), '%wmr%', wmr)
					WHEN who IN (702, 703, 707) THEN REPLACE(rem, '%K%', s)
					ELSE rem END rem
			FROM src3
			WHERE flzp != 3
	),
	src5 AS
	(
		SELECT lid, cdate, who, summa, tag1, tag2, i, wmr, s, tSum,
				CASE WHEN tag1 IS NOT NULL THEN REPLACE(rem, '%1%', CAST(tag1 AS VARCHAR)) ELSE rem END rem
			FROM src4
	),
	src6 AS
	(
		SELECT lid, cdate, who, summa, tag1, tag2, i, wmr, s, tSum,
				CASE WHEN tag2 IS NOT NULL
					THEN REPLACE(REPLACE(rem,
						'%2%', CAST(tag2 AS VARCHAR)),
						'%2s%', CAST(CAST(0.01 * ABS(tag2) AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + @soc) ELSE rem END rem
			FROM src5
	),
	src7 AS
	(
		SELECT lid, cdate, who, summa, tag1, tag2, i, wmr, s, tSum,
				CASE WHEN s IS NOT NULL
					THEN REPLACE(rem, '%d%', s) ELSE rem END rem
			FROM src6
	)
	SELECT
			cdate,
			CASE WHEN s IS NOT NULL
				THEN REPLACE(rem, '%s%', CAST(CAST(0.01 * ABS(tSum) AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + @soc) ELSE rem END rem,
			summa
		FROM src7
		ORDER BY lid DESC
		OPTION (RECOMPILE);
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN12_LOGS_PAYS",  conn))
--        {
--            cmd.AddIntParam("@kli", request.kli);
--            cmd.AddVarCharParam("@whos", 250, request.whos);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    DateTime cdate = dr.GetDateTimeOrNull("CDATE");
--                    string rem = dr.GetStringOrDefault("REM");
--                    decimal summa = dr.GetDecimalOrDefault("SUMMA");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}