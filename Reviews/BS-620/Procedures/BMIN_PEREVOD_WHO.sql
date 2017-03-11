IF OBJECT_ID ('dbo.BMIN_PEREVOD_WHO', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN_PEREVOD_WHO;
GO

CREATE PROCEDURE dbo.BMIN_PEREVOD_WHO
    @kl int,
    @who int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @soc VARCHAR(10);

	IF (@who = 999)
	BEGIN
		SELECT zdate cdate, remm rem, summa / 100.00 summa, CAST(NULL AS FLOAT) bal, CAST(NULL AS BIGINT) tag2
			FROM i_kzap
			WHERE client = @kl AND paymethod = 2 AND flag = 2
			ORDER BY cdate DESC
	END
	ELSE IF (@who = 998)
	BEGIN
		SELECT
				dt cdate,
				'Выплата с кассы: ' + CAST(kassa AS VARCHAR(10)) + ' на сумму ' + CAST(CAST(-summa / 100.00 AS DECIMAL(15, 2)) AS VARCHAR) rem,
				summa / 100.00 summa,
				CAST(NULL AS FLOAT) bal,
				CAST(NULL AS BIGINT) tag2
			FROM kass_log
			WHERE tag1 = @kl AND who = 25
			ORDER BY cdate DESC
	END
	ELSE
	BEGIN
		SELECT @soc = v.soc
			FROM VALUTA V
			INNER JOIN CLIENT C ON V.ID = C.VALUTA
			WHERE C.id = @kl;

		-- Два практически одинаковых запроса. Исключение - объединение с kassa и разные d_replacer
		IF (@who = 503)
		BEGIN
			WITH src AS
			(
				SELECT
						l.dt cdate, 0.01 * l.summa summa, ISNULL(l.tag1, 0) tag1, ISNULL(l.tag2, 0) tag2, 0.01 * l.bal bal, d.rus rem,
						CAST(CAST(ABS(summa) AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + @soc s_replacer,
						CASE
							WHEN k.id IS NULL THEN ''
							WHEN (k.num IS NULL OR k.adres IS NULL) THEN '(Пункт закрыт)'
							ELSE '№' + CAST(k.num AS VARCHAR) + ' (' + k.adres + ')' END d_replacer
					FROM log_cli l
					INNER JOIN dict_mini d ON d.who = 201 AND d.fid = l.who
					LEFT JOIN kassa k ON k.id = ISNULL(l.tag1, 0)
					WHERE l.client = @kl AND l.who = @who
			)
			SELECT
					cdate,
					REPLACE(REPLACE(REPLACE(REPLACE(rem,
						'%1%', CAST(tag1 AS VARCHAR)),
						'%2%', CAST(tag2 AS VARCHAR)),
						'%d%', d_replacer),
						'%s%', s_replacer) rem,
					summa, bal, tag2
				FROM src;
		END
		ELSE
		BEGIN
			WITH src AS
			(
				SELECT
						l.dt cdate, 0.01 * l.summa summa, ISNULL(l.tag1, 0) tag1, ISNULL(l.tag2, 0) tag2, 0.01 * l.bal bal, d.rus rem,
						CAST(CAST(ABS(summa) AS DECIMAL(15, 2)) AS VARCHAR) + ' ' + @soc s_replacer,
						'' d_replacer
					FROM log_cli l
					INNER JOIN dict_mini d ON d.who = 201 AND d.fid = l.who
					WHERE l.client = @kl AND l.who = @who
			)
			SELECT
					cdate,
					REPLACE(REPLACE(REPLACE(REPLACE(rem,
						'%1%', CAST(tag1 AS VARCHAR)),
						'%2%', CAST(tag2 AS VARCHAR)),
						'%d%', d_replacer),
						'%s%', s_replacer) rem,
					summa, bal, tag2
				FROM src;
		END
	END;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN_PEREVOD_WHO",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);
--            cmd.AddIntParam("@who", request.who);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    DateTime cdate = dr.GetDateTimeOrNull("CDATE");
--                    string rem = dr.GetStringOrDefault("REM");
--                    double summa = dr.GetDoubleOrDefault("SUMMA");
--                    double bal = dr.GetDoubleOrDefault("BAL");
--                    long tag2 = dr.GetInt64OrDefault("TAG2");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}