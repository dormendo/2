﻿IF OBJECT_ID ('dbo.BMIN15_LOGS', 'TF') IS NOT NULL
    DROP FUNCTION dbo.BMIN15_LOGS;
GO


CREATE FUNCTION dbo.BMIN15_LOGS
(	
    @kli int,
    @isdep bit,
    @isdt bit,
    @val smallint,
    @d1 datetime2,
    @d2 datetime2
)
RETURNS @result TABLE (cdate datetime2 not null, rem varchar(1000) not null, who int not null, summ float, bal float)
AS
BEGIN
	
	DECLARE @t1 TABLE(
		[cdate] [datetime2](7) NOT NULL,
		[who] [smallint] NOT NULL,
		[summ] [real] NULL,
		[bal] [real] NULL,
		[tag1] [int] NULL,
		[tag2] [bigint] NULL,
		[wmr] [varchar](24) NULL,
		[soc] [varchar](16) NULL,
		[s] [varchar](250) NULL,
		[t_sum] [bigint] NULL,
		[rem] [varchar](1000) NULL
	);

	IF (@isdt = 0)
	BEGIN
		SET @d1 = '2000-01-01';
		SET @d2 = DATEADD(day, 100, SYSDATETIME());
	END;
	
	WITH src1 AS
	(
		SELECT L.dt cdate, l.who, l.summa t_sum, l.bal t_bal, l.tag1, l.tag2, l.tag3, l.tag4 wmr, d.rus rem, v2.soc,
				IIF(@val > 0, v2.kurs / (v1.kurs * 100), 0.01) kurs3,
				CAST(ISNULL(l.tag1, 0) AS binary(4)) tag1_bin
			FROM log_cli l
			INNER JOIN dict_mini d ON d.who = 201 AND d.fid = l.who
			LEFT JOIN valuta v1 ON v1.id = @val
			LEFT JOIN client c ON c.id = @kli
			LEFT JOIN valuta v2 ON v2.id = c.valuta
			WHERE l.client = @kli AND l.dt BETWEEN @d1 AND @d2 AND
				(@isdep = 0 OR l.who BETWEEN 500 AND 999)
	),
	src2 AS
	(
		SELECT cdate, who, t_sum, t_sum * kurs3 summ, t_bal * kurs3 bal, tag1, tag2, tag3, wmr, soc,
				CASE
					WHEN who IN (702, 703, 707)
						THEN CONCAT(
							CAST(SUBSTRING(tag1_bin, 1, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 2, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 3, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 4, 1) AS tinyint)
							)
					ELSE NULL END ip,
				CASE	
					WHEN who = 520 AND tag3 IS NOT NULL THEN REPLACE(rem, '%3%', tag3)
					ELSE rem END rem
			FROM src1
	)
	INSERT INTO @t1 (cdate, who, summ, bal, tag1, tag2, wmr, soc, s, t_sum, rem)
		SELECT s.cdate, s.who, s.summ, s.bal, s.tag1, s.tag2, s.wmr, s.soc,
				CASE
					WHEN s.who IN (530, 999) THEN u.v
					WHEN s.who = 550 THEN s.wmr
					WHEN s.who IN (702, 703, 707) AND -s.t_sum - s.tag3 != 0
						THEN ' . Комиссия за перевод ' + CAST(CAST(0.01 * (-s.t_sum - s.tag3) AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + soc
					WHEN s.who = 503 THEN ISNULL('№' + CAST(k.num AS VARCHAR) + ' (' + k.adres + ')', '(Пункт закрыт)')
					WHEN s.who = 309 THEN CAST(c.tag1 AS VARCHAR) 
					WHEN s.who IN (520, 720, 220) THEN ISNULL(dm.rus, '?????')
					ELSE '' END s,
				CASE
					WHEN s.who = 700 THEN ISNULL(ik1.summa, 0)
					ELSE t_sum END t_sum,
				CASE
					WHEN s.who = 9999
						THEN 'Вычет из баланса по ID карточки = ' + CAST(lc.tag1 AS VARCHAR) + ' от ' + CAST(lc.dt AS VARCHAR(19))
					WHEN s.who IN (702, 703, 707)
						THEN REPLACE(s.rem, '%sd%', CAST(CAST(0.01 * s.tag3 AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + s.soc)
					WHEN s.who IN (702, 703, 707) AND s.tag2 IS NOT NULL
						THEN REPLACE(REPLACE(s.rem,
							'%wmid%', CAST(s.tag2 AS VARCHAR)),
							'%sd%', CAST(CAST(0.01 * s.tag3 AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + s.soc)
					WHEN s.who IN (101, 120, 130)
						THEN REPLACE(s.rem, '%ip%', s.ip)
					WHEN s.who IN (207, 517) AND s.wmr IS NOT NULL
						THEN REPLACE(s.rem, '%4%', s.wmr)
					WHEN s.who = 218
						THEN REPLACE(rem, '%sd%', CAST(CAST(0.01 * s.tag3 AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					WHEN s.who IN (520, 720, 220)
						THEN REPLACE(rem, '%4%', ISNULL(s.wmr, '?????'))
					WHEN s.who = 502 AND s.wmr IS NOT NULL THEN s.rem + ' ' + s.wmr
					ELSE s.rem END rem
			FROM src2 s
			LEFT JOIN uni_d u ON u.id = s.tag1 AND s.who IN (530, 999)
			LEFT JOIN log_cli lc ON lc.id = s.tag3 AND s.who = 9999
			LEFT JOIN i_kzap ik1 ON ik1.id = s.tag2 AND s.who = 700
			LEFT JOIN kassa k ON k.id = s.tag1 AND s.who = 503
			LEFT JOIN card2 c ON c.id = s.tag1 AND s.who = 309
			LEFT JOIN dict_mini dm ON dm.fid = s.tag3 AND dm.who = 400 AND s.who IN (520,720,220)
			OPTION(MAXDOP 1, RECOMPILE);



	WITH src3 AS
	(
		SELECT cdate, who, t_sum, summ, bal, tag1, tag2, soc, s,
				CASE
					WHEN who IN (702, 703, 707) AND wmr IS NOT NULL
						THEN REPLACE(REPLACE(rem, '%K%', s), '%wmr%', wmr)
					WHEN who IN (702, 703, 707) THEN REPLACE(rem, '%K%', s)
					WHEN who = 517 THEN REPLACE(REPLACE(rem, '%2%', CAST(tag2 AS VARCHAR)), 'WMID=-1, ', '')
					WHEN who = 218 AND wmr IS NOT NULL THEN REPLACE(rem, '%4%', wmr)
					WHEN who IN (520, 720, 220) THEN REPLACE(rem, '%D3%', s)
					WHEN who IN (200, 201, 341) AND tag2 IS NOT NULL
						THEN REPLACE(rem, '%2s%', CAST(CAST(0.01 * ABS(tag2) AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					ELSE rem END rem
			FROM @t1
	),
	src4 AS
	(
		SELECT cdate, who, t_sum, summ, bal, soc,
				CASE
					WHEN who IN (702, 703, 707) AND tag2 IS NOT NULL THEN REPLACE(rem, '%wmid%', tag2)
					WHEN who IN (200,301,309,331,341,509,512,517,551,552,553,700) AND tag1 IS NOT NULL
						THEN REPLACE(rem, '%1%', CAST(tag1 AS VARCHAR))
					WHEN who IN (207,217,218,220,502,506,507,508,518,520) AND tag2 IS NOT NULL THEN
						REPLACE(rem, '%2%', CAST(tag2 AS VARCHAR))
					WHEN who IN (418,503,530,550,999) AND s IS NOT NULL THEN
						REPLACE(rem, '%d%', s)
					ELSE rem END rem
			FROM src3
	)
	INSERT INTO @result (cdate, who, summ, bal, rem)
		SELECT cdate, who, summ, bal,
				CASE WHEN t_sum IS NOT NULL AND who IN (300,301,341,418,502,503,507,508,509,512,518,520,700,720) THEN
						REPLACE(rem, '%s%', CAST(CAST(0.01 * ABS(t_sum) AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					ELSE rem END rem
			FROM src4
			OPTION(MAXDOP 1);

	RETURN;
END
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN15_LOGS",  conn))
--        {
--            cmd.AddIntParam("@kli", request.kli);
--            cmd.AddSmallIntParam("@isdep", request.isdep);
--            cmd.AddSmallIntParam("@isdt", request.isdt);
--            cmd.AddSmallIntParam("@val", request.val);
--            cmd.AddDateTime2Param("@d1", request.d1);
--            cmd.AddDateTime2Param("@d2", request.d2);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    DateTime cdate = dr.GetDateTimeOrNull("CDATE");
--                    string rem = dr.GetStringOrDefault("REM");
--                    int who = dr.GetInt32OrDefault("WHO");
--                    double summ = dr.GetDoubleOrDefault("SUMM");
--                    double bal = dr.GetDoubleOrDefault("BAL");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}

/*

IF OBJECT_ID ('dbo.BMIN15_LOGS_OLD', 'IF') IS NOT NULL
    DROP FUNCTION dbo.BMIN15_LOGS_OLD;
GO


CREATE FUNCTION dbo.BMIN15_LOGS_OLD
(	
    @kli int,
    @isdep smallint,
    --@isdt smallint,
    @val smallint,
    @d1 datetime2,
    @d2 datetime2
)
RETURNS TABLE 
AS
RETURN 
(
	WITH src1 AS
	(
		SELECT L.dt cdate, l.who, l.summa t_sum, l.bal t_bal, l.tag1, l.tag2, l.tag3, l.tag4 wmr, d.rus rem, v2.soc,
				IIF(@val > 0, v2.kurs / (v1.kurs * 100), 0.01) kurs3,
				CAST(ISNULL(l.tag1, 0) AS binary(4)) tag1_bin
			FROM log_cli l
			INNER JOIN dict_mini d ON d.who = 201 AND d.fid = l.who
			LEFT JOIN valuta v1 ON v1.id = @val
			LEFT JOIN client c ON c.id = @kli
			LEFT JOIN valuta v2 ON v2.id = c.valuta
			WHERE l.client = @kli AND l.dt BETWEEN @d1 AND @d2
	),
	src2 AS
	(
		SELECT cdate, who, t_sum, t_sum * kurs3 summ, t_bal, t_bal * kurs3 bal, tag1, tag2, tag3, wmr, rem, soc,
				CASE
					WHEN who IN (702, 703, 707)
						THEN CONCAT(
							CAST(SUBSTRING(tag1_bin, 1, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 2, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 3, 1) AS tinyint), '.',
							CAST(SUBSTRING(tag1_bin, 4, 1) AS tinyint)
							)
					ELSE NULL END ip
			FROM src1
	),
	src3 AS
	(
		SELECT s.cdate, s.who, s.summ, s.t_bal, s.bal, s.tag1, s.tag2, s.tag3, s.wmr, s.soc,
				CASE
					WHEN s.who IN (530, 999) THEN u.v
					WHEN s.who = 550 THEN s.wmr
					WHEN s.who IN (702, 703, 707) AND -s.t_sum - s.tag3 != 0
						THEN ' . Комиссия за перевод ' + CAST(CAST(0.01 * (-s.t_sum - s.tag3) AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + soc
					WHEN s.who = 503 THEN ISNULL('№' + CAST(k.num AS VARCHAR) + ' (' + k.adres + ')', '(Пункт закрыт)')
					WHEN s.who = 309 THEN CAST(c.tag1 AS VARCHAR) 
					WHEN s.who IN (520, 720, 220) THEN ISNULL(dm.rus, '?????')
					ELSE '' END s,
				CASE
					WHEN s.who = 700 THEN ISNULL(ik1.summa, 0)
					ELSE t_sum END t_sum,
				CASE
					WHEN s.who = 9999
						THEN 'Вычет из баланса по ID карточки = ' + CAST(lc.tag1 AS VARCHAR) + ' от ' + CAST(lc.dt AS VARCHAR(19))
					WHEN s.who IN (702, 703, 707)
						THEN REPLACE(s.rem, '%sd%', CAST(CAST(0.01 * s.tag3 AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + s.soc)
					WHEN s.who IN (702, 703, 707) AND s.tag2 IS NOT NULL
						THEN REPLACE(REPLACE(s.rem,
							'%wmid%', CAST(s.tag2 AS VARCHAR)),
							'%sd%', CAST(CAST(0.01 * s.tag3 AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + s.soc)
					WHEN s.who IN (101, 120, 130)
						THEN REPLACE(s.rem, '%ip%', s.ip)
					WHEN s.who IN (207, 517) AND s.wmr IS NOT NULL
						THEN REPLACE(s.rem, '%4%', s.wmr)
					WHEN s.who = 218
						THEN REPLACE(rem, '%sd%', CAST(CAST(0.01 * s.tag3 AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					WHEN s.who IN (520, 720, 220)
						THEN REPLACE(rem, '%4%', ISNULL(s.wmr, '?????'))
					WHEN s.who = 502 AND s.wmr IS NOT NULL THEN s.rem + ' ' + s.wmr
					ELSE s.rem END rem
			FROM src2 s
			LEFT JOIN uni_d u ON u.id = s.tag1 AND s.who IN (530, 999)
			LEFT JOIN log_cli lc ON lc.id = s.tag3 AND s.who = 9999
			LEFT JOIN i_kzap ik1 ON ik1.id = s.tag2 AND s.who = 700
			LEFT JOIN kassa k ON k.id = s.tag1 AND s.who = 503
			LEFT JOIN card2 c ON c.id = s.tag1 AND s.who = 309
			LEFT JOIN dict_mini dm ON dm.fid = s.tag3 AND dm.who = 400 AND s.who IN (520,720,220)
	),
	src4 AS
	(
		SELECT cdate, who, t_sum, summ, bal, tag1, tag2, tag3, soc, s,
				CASE
					WHEN who IN (702, 703, 707) AND wmr IS NOT NULL
						THEN REPLACE(REPLACE(rem, '%K%', s), '%wmr%', wmr)
					WHEN who IN (702, 703, 707) THEN REPLACE(rem, '%K%', s)
					WHEN who = 517 THEN REPLACE(REPLACE(rem, '%2%', CAST(tag2 AS VARCHAR)), 'WMID=-1, ', '')
					WHEN who = 218 AND wmr IS NOT NULL THEN REPLACE(rem, '%4%', wmr)
					WHEN who IN (520, 720, 220) THEN REPLACE(rem, '%D3%', s)
					ELSE rem END rem
			FROM src3
	),
	src5 AS
	(
		SELECT cdate, who, t_sum, summ, bal, tag1, tag2, tag3, soc, s,
				CASE
					WHEN who IN (702, 703, 707) AND tag2 IS NOT NULL THEN REPLACE(rem, '%wmid%', tag2)
					WHEN who = 218 AND tag2 IS NOT NULL THEN REPLACE(rem, '%2%', tag2)
					ELSE rem END rem
			FROM src4
	),
	src6 AS
	(
		SELECT cdate, who, t_sum, summ, bal, tag2, tag3, soc, s,
				IIF(tag1 IS NOT NULL, REPLACE(rem, '%1%', tag1), rem) rem
			FROM src5
	),
	src7 AS
	(
		SELECT cdate, who, t_sum, summ, bal, tag3, soc, s,
				CASE WHEN tag2 IS NOT NULL THEN
						REPLACE(REPLACE(rem, '%2%', tag2), '%2s%', CAST(CAST(0.01 * ABS(tag2) AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					ELSE rem END rem
			FROM src5
	),
	src8 AS
	(
		SELECT cdate, who, t_sum, summ, bal, soc, s,
				IIF(tag3 IS NOT NULL, REPLACE(rem, '%3%', tag3), rem) rem
			FROM src7
	),
	src9 AS
	(
		SELECT cdate, who, t_sum, summ, bal, soc,
				IIF(s IS NOT NULL, REPLACE(rem, '%d%', s), rem) rem
			FROM src8
	),
	src10 AS
	(
		SELECT cdate, who, summ, bal,
				CASE WHEN t_sum IS NOT NULL THEN
						REPLACE(rem, '%s%', CAST(CAST(0.01 * ABS(t_sum) AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					ELSE rem END rem
			FROM src9
	)
	SELECT cdate, rem, who, summ, bal
		FROM src10
)
GO

*/