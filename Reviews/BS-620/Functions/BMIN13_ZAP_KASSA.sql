IF OBJECT_ID ('dbo.F_BMIN13_ZAP_KASSA', 'IF') IS NOT NULL
    DROP FUNCTION dbo.F_BMIN13_ZAP_KASSA;
GO


CREATE FUNCTION dbo.BMIN13_ZAP_KASSA
(	
    @vid smallint,
	@md smallint
)
RETURNS TABLE 
AS
RETURN 
(
	-- Использовать только с OPTION(RECOMPILE)
	SELECT z.client, z.kassa, z.id, z.zdate, z.edate,
			-- Логика согласована с Виктором
			CASE
				WHEN v.kurs IS NULL OR c.valuta = 2 THEN 0.01 * z.summa
				ELSE 0.01 * v.kurs * z.summa / v2.kurs END summa,
			z.flag, z.remm, 
			CASE z.flag 
				WHEN 0 THEN 'в обработке'
				WHEN 1 THEN 'разрешен на выплату'
				WHEN 2 THEN 'выплачен'
				WHEN 3 THEN 'отказ клиента'
				WHEN 4 THEN '????'
				WHEN 5 THEN 'отклонен администрацией из-за ' + z.otkaz
				ELSE 'Error' END sfl,
			z.tag2, ISNULL(u.fl, 0) fl3, ISNULL(i1.v, 0) osb_kassa, ISNULL(i2.v, 0) osb_client
		FROM I_KZAP z
		LEFT JOIN uni_cli u on u.who = 2 AND u.client = z.client
		LEFT JOIN uni_int i1 ON i1.who = 7000 AND i1.fid = z.kassa
		LEFT JOIN uni_int i2 ON i2.who = 7001 AND i2.fid = z.client
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
)
GO

