IF OBJECT_ID ('dbo.BMI_ALL_VYP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMI_ALL_VYP;
GO

CREATE PROCEDURE dbo.BMI_ALL_VYP
    @pm int,
    @dt date,
    @sumv int
AS
BEGIN
    SET NOCOUNT ON;

	WITH src AS
	(
		SELECT i.id
			FROM i_kzap i
			INNER JOIN client c ON c.id = i.client
			INNER JOIN valuta v ON v.id = c.valuta
			WHERE i.paymethod = @pm AND i.flag = 0 AND i.zdate < @dt AND i.summa / v.kurs < @sumv
	)
	UPDATE i_kzap SET flag=1
		WHERE id IN (SELECT id FROM src);

END;
GO




IF OBJECT_ID ('dbo.BMI_GET_INFO', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMI_GET_INFO;
GO

CREATE PROCEDURE dbo.BMI_GET_INFO
    @kl int,
    @name1 varchar(250) OUTPUT,
    @balans double precision OUTPUT,
    @city varchar(80) OUTPUT,
    @country varchar(64) OUTPUT,
    @email varchar(80) OUTPUT,
    @secret1 varchar(250) OUTPUT,
    @secret2 varchar(250) OUTPUT,
    @dtr datetime2 OUTPUT,
    @vn varchar(64) OUTPUT,
    @sDep float OUTPUT,
    @sVyp float OUTPUT,
    @sP float OUTPUT,
    @sV float OUTPUT,
    @sR float OUTPUT,
    @prc float OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
			@name1 = c.name1 + ' ' + c.name2,
			@balans = c.balans / 100.00,
			@city = c.city,
			@country = c.country,
			@email = c.email,
			@secret1 = c.secret1,
			@secret2 = c.secret2,
			@dtr = c.dtreg,
			@vn = v.soc
		FROM client c
		INNER JOIN valuta v ON v.id = c.valuta
		WHERE c.id = @kl;

	SELECT @sP = 0.01 * SUM(SUMMA) FROM CLIENT_CNT WHERE client = @kl AND who = 20;
	SELECT @sV = 0.01 * SUM(SUMMA) FROM CLIENT_CNT WHERE client = @kl AND who = 21;
	SELECT @sDep = 0.01 * SUM(SUMMA) FROM CLIENT_CNT WHERE client = @kl AND who IN (100,101,102,103,104,150,171,172);
	SELECT @sVyp = 0.01 * SUM(SUMMA) FROM CLIENT_CNT WHERE client = @kl AND who IN (200,201,204);
    
	SET @sR = @sP - @sV;
	SET @prc = IIF(@sP = 0, 0.0, 100.0 * @sR / @sP
	);
END;
GO




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




IF OBJECT_ID ('dbo.BMI_SET_LIM', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMI_SET_LIM;
GO

CREATE PROCEDURE dbo.BMI_SET_LIM
    @kl int,
    @sport int,
    @summa bigint,
    @val2 smallint
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @v1Kurs FLOAT;
	DECLARE @v2Kurs FLOAT;

	IF (@val2 > 0)
	BEGIN
		SELECT @v1Kurs = v.kurs
			FROM valuta v
			INNER JOIN client c ON c.valuta = v.id
			WHERE c.id = @kl;

		SELECT @v2Kurs = v.kurs
			FROM valuta v
			WHERE v.id = @val2;

		SET @summa = @v1Kurs * @summa / @v2Kurs;
	END

	MERGE INTO cl_limit trg
		USING (SELECT @kl, @sport) AS src(cl, sport) ON trg.cl = src.cl AND trg.sport = src.sport
		WHEN MATCHED THEN
			UPDATE SET summa = @summa
		WHEN NOT MATCHED THEN
			INSERT (cl, sport, summa) VALUES (@kl, @sport, @summa);
END;
GO




IF OBJECT_ID ('dbo.BMI_SET_MAX', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMI_SET_MAX;
GO

CREATE PROCEDURE dbo.BMI_SET_MAX
    @kl int,
    @summ bigint,
    @val2 smallint
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @v1Kurs FLOAT;
	DECLARE @v2Kurs FLOAT;

	IF (@val2 > 0)
	BEGIN
		SELECT @v1Kurs = v.kurs
			FROM valuta v
			INNER JOIN client c ON c.valuta = v.id
			WHERE c.id = @kl;

		SELECT @v2Kurs = v.kurs
			FROM valuta v
			WHERE v.id = @val2;

		SET @summ = @v1Kurs * @summ / @v2Kurs;
	END
	
	UPDATE client SET maxsum = @summ WHERE id = @kl;
END;
GO




IF OBJECT_ID ('dbo.BMI14_CARDS', 'IF') IS NOT NULL
    DROP FUNCTION dbo.BMI14_CARDS;
GO


CREATE FUNCTION dbo.BMI14_CARDS
(	
	@client int
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT c.id id, c.kdate date_p, c.dt_v date_v, 0.01 * c.summa_p sum_p, 0.01 * c.summa_v sum_v, c.cstatus st, s.id is_stavki, s.stype type_s,
			IIF(s.stype = 2, CAST(s.skoko AS VARCHAR(5)) + '/' + CAST(s.poskoko AS VARCHAR(5)),'') sysof
		FROM card c
		INNER JOIN stavki s ON s.card = c.id
		WHERE c.kass = 600 and c.klient = @client
	UNION ALL
	SELECT c.id id, c.kdate date_p, c.dt_v date_v, 0.01 * c.summa_p sum_p, 0.01 * c.summa_v sum_v, c.cstatus st, s.id is_stavki, s.stype type_s,
			IIF(s.stype = 2, CAST(s.skoko AS VARCHAR(5)) + '/' + CAST(s.poskoko AS VARCHAR(5)),'') sysof
		FROM ARH.dbo.card c
		INNER JOIN ARH.dbo.stavki s ON s.card = c.id
		WHERE c.kass = 600 and c.klient = @client
);
GO




IF OBJECT_ID ('dbo.BMI14_STAVKI', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMI14_STAVKI;
GO

CREATE PROCEDURE dbo.BMI14_STAVKI
    @idStavki int,
    @isArh smallint
AS
BEGIN
    SET NOCOUNT ON;

	IF (@isArh = 1)
		SELECT c1.rus k1,
				c2.rus k2,
				l.ldate date_l,
				REPLACE(REPLACE(REPLACE(REPLACE(b.rus, '%1', o.ad1), '%2', o.ad2), '%f', c1.rus), '%s', c2.rus) bet_name,
				0.001 * s.koef kf,
				s.ready ready
			FROM ARH.dbo.stavka s
			INNER JOIN ARH.dbo.odd  o ON o.id = s.odd
			INNER JOIN ARH.dbo.line l ON l.id = o.line
			INNER JOIN ARH.dbo.com c1 ON c1.id = l.k1
			INNER JOIN ARH.dbo.com c2 ON c2.id = l.k2
			INNER JOIN ARH.dbo.bet b  ON b.id = o.bet
			WHERE s.stavki = @idStavki
			ORDER BY s.id;
	ELSE
		SELECT
				c1.rus k1,
				c2.rus k2,
				l.ldate date_l,
				REPLACE(REPLACE(REPLACE(REPLACE(b.rus, '%1', o.ad1), '%2', o.ad2), '%f', c1.rus), '%s', c2.rus) bet_name,
				0.001 * s.koef kf,
				s.ready ready
			FROM stavka s
			INNER JOIN odd  o ON o.id = s.odd
			INNER JOIN line l ON l.id = o.line
			INNER JOIN com c1 ON c1.id = l.k1
			INNER JOIN com c2 ON c2.id = l.k2
			INNER JOIN bet b  ON b.id = o.bet
			WHERE s.stavki = @idStavki
			ORDER BY s.id;
END;
GO




IF OBJECT_ID ('dbo.BMIN_CANCEL_DEP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN_CANCEL_DEP;
GO

CREATE PROCEDURE dbo.BMIN_CANCEL_DEP
    @kassa int,
    @client int,
    @sum2 int,
    @st varchar(48) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @bal BIGINT;
	DECLARE @kassirId INTEGER;
	DECLARE @cnt INTEGER;
	DECLARE @id INTEGER;
	DECLARE @today DATE = CAST(SYSDATETIME() AS DATE);
	DECLARE @remm AS VARCHAR(250);
	DECLARE @outerTranCount int;

	SET @st = 'Не найдено';

	-- Изменение в DATEADD с 10 дней на 7 согласовано с Виктором
	SELECT @cnt = count(*)
		FROM kass_log l
		WHERE l.kassa = @kassa AND l.tag1 = @client AND l.dt > DATEADD(day, -7, @today) AND l.summa = @sum2 AND l.who = 5;

	IF (@cnt > 0)
	BEGIN
		IF (@cnt > 1)
			SET @st = 'Найдено: ' + CAST(@cnt AS VARCHAR(10)) + ' , отменен 1 последний депозит';
		ELSE
			SET @st = 'Отменен';

		SET @remm = 'Отмена депозита по клиенту: ' + CAST(@client AS VARCHAR(10)) +
			', касса: ' + CAST(@kassa AS VARCHAR(10)) +
			', сумма: ' + CAST(CAST(@sum2 / 100.00 AS DECIMAL(15, 2)) AS VARCHAR);

		BEGIN TRY
			SET @outerTranCount = @@TRANCOUNT;
			IF @outerTranCount = 0
				BEGIN TRANSACTION;

			INSERT INTO log_oper (who, remm) VALUES (2, @remm);

			WITH src AS
			(
				SELECT TOP(1) id
					FROM kvit
					WHERE kassa = @kassa AND client = @client AND kdate > DATEADD(day, -7, @today) AND summa = @sum2 AND fl = 1
					ORDER BY id DESC
			)
			DELETE FROM kvit
				WHERE id IN (SELECT id FROM src);

			WITH src AS
			(
				SELECT TOP(1) id
					FROM kass_log
					WHERE kassa = @kassa AND tag1 = @client AND dt > DATEADD(day, -7, @today) AND summa = @sum2 AND who = 5
					ORDER BY id DESC
			)
			UPDATE kass_log SET who = 1021, @kassirId = kassir
				WHERE id IN (SELECT id FROM src);

			UPDATE client SET balans = balans - @sum2
				WHERE id = @client;

			UPDATE kassa SET @bal = balans = balans - @sum2
				WHERE id = @kassa;

			WITH src AS
			(
				SELECT TOP(1) id
					FROM log_cli
					WHERE client = @client AND who = 503 AND summa = @sum2 AND dt > DATEADD(day, -7, @today)
					ORDER BY dt DESC
			)
			DELETE FROM log_cli
				WHERE id IN (SELECT id FROM src);

			INSERT INTO kass_log (kassa, who, summa, bal, tag1, tag2, kassir)
				VALUES (@kassa, 1020, -@sum2, @bal, 0, @client, @kassirId);

			IF @outerTranCount = 0
				COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
			IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
				ROLLBACK TRANSACTION;
			THROW;
		END CATCH;
	END;
END;
GO




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




IF OBJECT_ID ('dbo.BMIN_SET_BL_IP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN_SET_BL_IP;
GO

CREATE PROCEDURE dbo.BMIN_SET_BL_IP
    @ip int,
    @allCl smallint
AS
BEGIN
    SET NOCOUNT ON;
	
	IF (@allCl = 1)
	BEGIN
		DECLARE @count INT, @today date = CAST(SYSDATETIME() AS DATE);
		DECLARE @outerTranCount int;

		BEGIN TRY
			SET @outerTranCount = @@TRANCOUNT;
			IF @outerTranCount = 0
				BEGIN TRANSACTION;
		
			SELECT @count = COUNT(*)
				FROM log_cli
				WHERE who = 101 AND dt > DATEADD(day, -900, @today) AND tag1 = @ip;

			MERGE ip_black trg
				USING (SELECT @ip) AS src(ip) ON trg.ip = src.ip
				WHEN NOT MATCHED THEN
					INSERT (ip, cnt) VALUES (@ip, @count)
				WHEN MATCHED THEN
					UPDATE SET cnt = cnt + @count;

			UPDATE client SET
					fl1 = 1,
					fl2 = 1,
					maxsum = CASE valuta
						WHEN  1 THEN 1000
						WHEN  2 THEN 30000
						WHEN  3 THEN 2000000
						WHEN  4 THEN 5000
						WHEN  5 THEN 1000
						WHEN  6 THEN 150000
						WHEN  7 THEN 10000
						WHEN  8 THEN 40000
						WHEN  9 THEN 10000
						WHEN 10 THEN 400000
						WHEN 11 THEN 1000
						ELSE 10000 END
				WHERE id IN (SELECT client FROM log_cli WHERE who = 101 AND dt > DATEADD(day, -900, @today) AND tag1 = @ip);

			IF @outerTranCount = 0
				COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
			IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
				ROLLBACK TRANSACTION;
			THROW;
		END CATCH;
	END
	ELSE
	BEGIN
		MERGE ip_black trg
			USING (SELECT @ip) AS src(ip) ON trg.ip = src.ip
			WHEN NOT MATCHED THEN
				INSERT (ip) VALUES (@ip);
	END;
END;
GO




IF OBJECT_ID ('dbo.BMIN11_ADD_TEXDEP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN11_ADD_TEXDEP;
GO

CREATE PROCEDURE dbo.BMIN11_ADD_TEXDEP
    @kl int,
    @remm varchar(250),
    @summa int,
    @login varchar(64)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @bal BIGINT;
	DECLARE @idd INT;
	DECLARE @logId INT;

	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		UPDATE client
			SET @bal = balans = balans + @summa
			WHERE id = @kl;

		SELECT TOP(1) @idd = id FROM uni_d with(serializable) WHERE who = 200 AND v = @remm;

		IF (@idd IS NULL)
		BEGIN
			INSERT INTO uni_d (who, v) VALUES (200, @remm);
			SET @idd = SCOPE_IDENTITY();
		END;

		INSERT INTO log_cli (who, client, summa, bal, tag1)
			VALUES (999, @kl, @summa, @bal, @idd);
		SET @logId = SCOPE_IDENTITY();

		MERGE client_cnt trg
			USING (SELECT @kl, 199) AS src(client, who) ON trg.client = src.client AND trg.who = src.who
			WHEN NOT MATCHED THEN
				INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, 1, @summa)
			WHEN MATCHED THEN
				UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt + 1, summa = summa + @summa;

		DECLARE @ip VARCHAR(24) = CAST(CONNECTIONPROPERTY('client_net_address') AS VARCHAR(24));
		DECLARE @appName VARCHAR(128) = CAST(APP_NAME() AS VARCHAR(128));

		INSERT INTO log_uni (tb, who, prog, ip, tag1, tag2)
			VALUES (55, 55, '(' + @login + ') ' + @appName, @ip, @logId, @kl);
		
		IF @outerTranCount = 0
			COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO




IF OBJECT_ID ('dbo.BMIN11_OTKAZ', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN11_OTKAZ;
GO

CREATE PROCEDURE dbo.BMIN11_OTKAZ
    @idzap int,
    @fl int,
    @otkaz varchar(250),
    @f2 int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @idcl INT;
	DECLARE @bal BIGINT;
	DECLARE @summ BIGINT;
	DECLARE @fl2 INT;
	DECLARE @tag1 INT;
	DECLARE @pmd SMALLINT;
	DECLARE @idd INT;
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		SELECT @idcl = i.client, @summ = i.summa, @bal = c.balans, @fl2 = i.flag, @tag1 = i.tag1
			FROM I_KZAP i with(updlock)
			INNER JOIN client c with(updlock) ON c.id = i.client
			WHERE i.id = @idzap;

		IF (@idcl IS NULL OR @fl2 > @F2)
			RETURN;
	
		IF (@tag1 IS NOT NULL AND @tag1 > @summ)
			SET @summ = @tag1;
	
		SET @bal = @bal + @summ;

		UPDATE I_KZAP
			SET flag = @fl, otkaz = @otkaz, @pmd = paymethod
			WHERE id = @idzap;

		UPDATE client SET balans = @bal WHERE id = @idcl;

		SELECT TOP(1) @idd = id FROM uni_d with(serializable) WHERE who = 200 AND v = @otkaz;

		IF (@idd IS NULL)
		BEGIN
			INSERT INTO uni_d (who, v) VALUES (200, @otkaz);
			SET @idd = SCOPE_IDENTITY();
		END;

		INSERT INTO log_cli (who, client, summa, bal, tag1, tag2)
			VALUES (530, @idcl, @summ, @bal, @idd, @idzap);

		IF (@pmd = 2)
		BEGIN
			MERGE client_cnt trg
				USING (SELECT @idcl, 201) AS src(client, who) ON trg.client = src.client AND trg.who = src.who
				WHEN NOT MATCHED THEN
					INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, -1, -@summ)
				WHEN MATCHED THEN
					UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt - 1, summa = summa - @summ;
		END;
		
		IF @outerTranCount = 0
			COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO




IF OBJECT_ID ('dbo.BMIN12_BAN_CLIENT', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_BAN_CLIENT;
GO

CREATE PROCEDURE dbo.BMIN12_BAN_CLIENT
    @client int,
    @fl3 int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @ip VARCHAR(128) = CAST(CONNECTIONPROPERTY('client_net_address') AS VARCHAR(128));
	DECLARE @appName VARCHAR(128) = CAST(APP_NAME() AS VARCHAR(128));
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		IF (@fl3 = 1)
			MERGE uni_cli trg
				USING (SELECT 2, @client) AS src(who, client) ON trg.who = src.who AND trg.client = src.client
				WHEN NOT MATCHED THEN
					INSERT (who, client, fl) VALUES (src.who, src.client, 1)
				WHEN MATCHED THEN
					UPDATE SET fl = 1;
		ELSE
			UPDATE uni_cli SET fl = 0, d2 = SYSDATETIME()
			WHERE who = 2 AND client = @client;

		INSERT INTO log_uni (tb, who, prog, ip, tag1, tag2)
			VALUES (609, 609, @appName, @ip, @client, @fl3);
		
		IF @outerTranCount = 0
			COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO




IF OBJECT_ID ('dbo.BMIN12_CB_QIWI', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_CB_QIWI;
GO

CREATE PROCEDURE dbo.BMIN12_CB_QIWI
    @id int,
    @flQiwi int
AS
BEGIN
    SET NOCOUNT ON;
    IF (@flQiwi = 1)
		MERGE INTO uni_cli trg
			USING (SELECT 3, @id) AS src(who, client) ON trg.who = src.who AND trg.client = src.client
			WHEN NOT MATCHED THEN
				INSERT (who, client, fl) VALUES (src.who, src.client, 1)
			WHEN MATCHED THEN
				UPDATE SET fl = 1;
    ELSE
		UPDATE uni_cli SET fl = 0, d2 = SYSDATETIME() WHERE who = 3 AND client = @id;
END;
GO




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




IF OBJECT_ID ('dbo.BMIN12_QIWI_PAY_OK', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_QIWI_PAY_OK;
GO

CREATE PROCEDURE dbo.BMIN12_QIWI_PAY_OK
    @id int,
    @trid bigint,
    @bl int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @cl INT;
	DECLARE @summa INT;
	DECLARE @r VARCHAR(24);
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		UPDATE i_kzap
			SET flag = 2, @cl = client, @summa = summa, @r = remm
			WHERE id = @id;

		INSERT INTO log_cli (client, who, tag1, tag2, tag3, tag4)
			VALUES (@cl, 207, @bl, @trid, @id, @r);

		MERGE client_cnt trg
			USING (SELECT @cl, 204) AS src(client) ON trg.client = src.client AND trg.who = src.who
			WHEN NOT MATCHED THEN
				INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, 1, @summa)
			WHEN MATCHED THEN
				UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt + 1, summa = summa + @summa;
		
		IF @outerTranCount = 0
			COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO




IF OBJECT_ID ('dbo.BMIN12_VR_ZAPR', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_VR_ZAPR;
GO

CREATE PROCEDURE dbo.BMIN12_VR_ZAPR
    @idzap int,
    @tag2 smallint OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE i_kzap SET @tag2 = tag2 = 1 - tag2 WHERE id = @idzap;
END;
GO




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




IF OBJECT_ID ('dbo.BMIN12_ZAP_STAT', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_ZAP_STAT;
GO

CREATE PROCEDURE dbo.BMIN12_ZAP_STAT
    @clId int,
    @cardSump decimal(15,2) OUTPUT,
    @cardSumv decimal(15,2) OUTPUT,
    @cardRaz decimal(15,2) OUTPUT,
    @cardPrc decimal(15,2) OUTPUT,
    @totoSump decimal(15,2) OUTPUT,
    @totoSumv decimal(15,2) OUTPUT,
    @totoRaz decimal(15,2) OUTPUT,
    @totoPrc decimal(15,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	SELECT @cardSump = ISNULL(0.01 * SUM(c.summa), 0) FROM client_cnt c WHERE c.client = @clId AND c.who = 20;
	SELECT @cardSumv = ISNULL(0.01 * SUM(c.summa), 0) FROM client_cnt c WHERE c.client = @clId AND c.who = 21;
	SELECT @totoSump = ISNULL(0.01 * SUM(c.summa), 0) FROM client_cnt c WHERE c.client = @clId AND c.who = 30;
	SELECT @totoSumv = ISNULL(0.01 * SUM(c.summa), 0) FROM client_cnt c WHERE c.client = @clId AND c.who = 31;

	SET @cardRaz = @cardSump - @cardSumv;
	SET @totoRaz = @totoSump - @totoSumv;

	SET @cardPrc = IIF (@cardSump > 0, 100.00 * (@cardRaz / @cardSump), NULL);
	SET @totoPrc = IIF (@totoSump > 0, 100.00 * (@totoRaz / @totoSump), NULL);
END;




IF OBJECT_ID ('dbo.BMIN12_ZAP_WMZ', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN12_ZAP_WMZ;
GO

CREATE PROCEDURE dbo.BMIN12_ZAP_WMZ
    @md smallint,
	@client int = NULL
AS
BEGIN
    SET NOCOUNT ON;

	SELECT z.client, z.id, z.zdate, z.edate, 0.01 * z.summa summa, z.flag, z.remm, z.paymethod,
			CASE z.flag 
				WHEN 0 THEN 'в обработке'
				WHEN 1 THEN 'Запрос на выплату WMZ'
				WHEN 2 THEN 'выплачен'
				WHEN 3 THEN 'отказ клиента'
				WHEN 4 THEN 'отклонен администрацией: не совпадает WMID с номером кошелька'
				WHEN 5 THEN 'отклонен администрацией из-за ' + z.otkaz
				ELSE 'Error' END sfl,
			z.tag2, ISNULL(u.fl, 0) fl3,
			(SELECT COUNT(*) FROM uni_int i WHERE i.who = 600 and i.fid = z.client) obr
		FROM I_KZAP z
		LEFT JOIN uni_cli u ON u.who = 2 AND u.client = z.client
		WHERE z.kassa IS NULL AND z.paymethod = 2
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




IF OBJECT_ID ('dbo.BMIN13_DO_WAIT', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN13_DO_WAIT;
GO

CREATE PROCEDURE dbo.BMIN13_DO_WAIT
AS
BEGIN
    SET NOCOUNT ON;
    
	SELECT z.id zid, z.zdate, 0.01 * z.summa summa, v.soc, z.client, u.id tid, z.remm, c.valuta
		FROM i_kzap z
		INNER JOIN client c ON c.id = z.client
		INNER JOIN valuta v ON v.id = c.valuta
		INNER JOIN uni_int u ON u.who = 105 AND u.fid = z.id
		WHERE z.paymethod = 2 AND z.flag = 9
		ORDER BY z.zdate;
END;
GO




IF OBJECT_ID ('dbo.BMIN13_SET_DICL', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN13_SET_DICL;
GO

CREATE PROCEDURE dbo.BMIN13_SET_DICL
    @id int,
    @name varchar(250),
    @login varchar(250),
    @dt varchar(250)
AS
BEGIN
    SET NOCOUNT ON;
	
	MERGE INTO uni_str trg
		USING (SELECT 1309, @id) AS src(who, i1) ON trg.who = src.who AND trg.i1 = src.i1
		WHEN NOT MATCHED THEN
			INSERT (who, i1, s1, s2, s3) VALUES (src.who, src.i1, @name, @login, @dt)
		WHEN MATCHED THEN
			UPDATE SET s1 = @name, s2 = @login, s3 = @dt;
END;
GO




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




IF OBJECT_ID ('dbo.BMIN14_ALL_VYP', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN14_ALL_VYP;
GO

CREATE PROCEDURE dbo.BMIN14_ALL_VYP
    @pm int,
    @dt date,
    @sumv int
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE i_kzap SET flag = 1
		FROM i_kzap z
		INNER JOIN client c ON c.id = z.client
		INNER JOIN valuta v ON v.id = c.valuta
		WHERE z.paymethod = @pm AND z.flag = 0 AND z.zdate < @dt AND z.summa / v.kurs < @sumv AND
			NOT EXISTS (SELECT * FROM uni_cli u2 WHERE u2.who = 3 AND u2.client = z.id AND u2.fl = 1);
END;
GO




IF OBJECT_ID ('dbo.BMIN14_GET_INFO', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN14_GET_INFO;
GO

CREATE PROCEDURE dbo.BMIN14_GET_INFO
    @kl int,
    @name1 varchar(250) OUTPUT,
    @balans double precision OUTPUT,
    @city varchar(80) OUTPUT,
    @country varchar(64) OUTPUT,
    @email varchar(80) OUTPUT,
    @secret1 varchar(250) OUTPUT,
    @secret2 varchar(250) OUTPUT,
    @dtr datetime2 OUTPUT,
    @soc varchar(16) OUTPUT,
    @sDep float OUTPUT,
    @sVyp float OUTPUT,
    @sP float OUTPUT,
    @sV float OUTPUT,
    @sR float OUTPUT,
    @prc float OUTPUT,
    @cpDt datetime2 OUTPUT,
    @u2Dt datetime2 OUTPUT,
    @u2Fl smallint OUTPUT,
    @u2D2 datetime2 OUTPUT,
    @maxsum int OUTPUT,
    @osob int OUTPUT,
    @di varchar(255) OUTPUT,
    @dilg varchar(255) OUTPUT,
    @didt varchar(255) OUTPUT,
    @pass varchar(255) OUTPUT,
    @env int OUTPUT,
    @fl1 int OUTPUT,
    @fl2 int OUTPUT,
    @dtroz date OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	-- 07.10.2013
	-- 15.03.2014
	SELECT @pass = psw FROM PSW..psw WHERE who = 0 AND fid = @kl;

	SELECT
			@name1 = c.name1 + ' ' + c.name2,
			@balans = 0.01 * c.balans,
			@city = c.city,
			@country = c.country,
			@email = c.email,
			@secret1 = c.secret1,
			@secret2 = c.secret2,
			@dtr = c.dtreg,
			@soc = v.soc,
			@env = c.evn,
			@fl1 = c.fl1,
			@fl2 = c.fl2,
			@maxsum = c.maxsum,
			@cpDt = u1.dt,
			@u2Dt = u2.dt,
			@u2Fl = u2.fl,
			@u2D2 = u2.d2,
			@osob = ISNULL(u3.v, 0),
			@di = u4.s1,
			@dilg = u4.s2,
			@didt = u4.s3,
			@dtroz = c.dtr
		FROM client c
		INNER JOIN valuta v ON v.id = c.valuta
		LEFT JOIN uni_cli u1 ON u1.who = 1 AND u1.client = c.id
		LEFT JOIN uni_cli u2 ON u2.who = 2 AND u2.client = c.id
		LEFT JOIN uni_int u3 ON u3.who = 7001 AND u3.fid = c.id
		LEFT JOIN uni_str u4 ON u4.who = 1309 AND u4.i1 = c.id
		WHERE c.id = @kl;

	SELECT @sP = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND who = 20;

	SELECT @sV = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND (who = 21);

	SELECT @sDep = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND who IN (100, 101, 102, 150);

	SELECT @sVyp = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND who IN (200, 201);

	SET @sR = @sP - @sV;

	SET @prc = IIF(@sP = 0, 0, 100.0000000 * @sR / @sP);
END;
GO




IF OBJECT_ID ('dbo.BMIN15_AP_PAY_OK', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN15_AP_PAY_OK;
GO

CREATE PROCEDURE dbo.BMIN15_AP_PAY_OK
    @zId int
AS
BEGIN
    SET NOCOUNT ON;
	
	DECLARE @client INT;
	DECLARE @summa INT;
	DECLARE @purse VARCHAR(24);
	DECLARE @idClcnt INT;
	DECLARE @typeId INT;
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;
	
		UPDATE i_kzap SET flag = 2, @client = client, @summa = summa, @purse = remm, @typeId = tag2
		FROM i_kzap
		WHERE id = @zId

		UPDATE bill SET ex_fl = 1, edate = SYSDATETIME()
			WHERE who IN (503, 504) AND tag1 = @zId;

		INSERT INTO log_cli (client, who, tag1, tag2, tag3, tag4)
			VALUES (@client, 220, @zId, @zId, @typeId, @purse);

		MERGE INTO client_cnt trg
			USING (SELECT @client, 220) AS src(client, who) ON trg.client = src.client AND trg.who = src.who
			WHEN NOT MATCHED THEN
				INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, 1, @summa)
			WHEN MATCHED THEN
				UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt + 1, summa = summa + 1;

		IF @outerTranCount = 0
			COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO




IF OBJECT_ID ('dbo.BMIN15_GET_INFO', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN15_GET_INFO;
GO

CREATE PROCEDURE dbo.BMIN15_GET_INFO
    @kl int,
	@name1 varchar(250) OUTPUT,
    @balans double precision OUTPUT,
    @city varchar(80) OUTPUT,
    @country varchar(64) OUTPUT,
    @email varchar(80) OUTPUT,
    @secret1 varchar(250) OUTPUT,
    @secret2 varchar(250) OUTPUT,
    @dtr datetime2 OUTPUT,
    @vn varchar(64) OUTPUT,
    @sDep float OUTPUT,
    @sVyp float OUTPUT,
    @sP float OUTPUT,
    @sV float OUTPUT,
    @sR float OUTPUT,
    @prc float OUTPUT,
    @cpDt datetime2 OUTPUT,
    @u2Dt datetime2 OUTPUT,
    @u2Fl smallint OUTPUT,
    @u2D2 datetime2 OUTPUT,
    @fl2 int OUTPUT,
    @maxsum int OUTPUT,
    @osob int OUTPUT,
    @di varchar(255) OUTPUT,
    @dilg varchar(255) OUTPUT,
    @didt varchar(255) OUTPUT

AS
BEGIN
    SET NOCOUNT ON;

	SELECT
			@name1 = c.name1 + ' ' + c.name2,
			@balans = 0.01 * c.balans,
			@city = c.city,
			@country = c.country,
			@email = c.email,
			@secret1 = c.secret1,
			@secret2 = c.secret2,
			@dtr = c.dtreg,
			@vn = v.soc,
			@cpDt = u1.dt,
			@u2Dt = u2.dt,
			@u2Fl = u2.fl,
			@u2D2 = u2.d2,
			@fl2 = c.fl2,
			@maxsum = c.maxsum,
			@osob = u3.v,
			@di = u4.s1,
			@dilg = u4.s2,
			@didt = u4.s3
		FROM client c
		INNER JOIN valuta v ON v.id = c.valuta
		LEFT JOIN uni_cli u1 ON u1.who = 1 AND u1.client = c.id
		LEFT JOIN uni_cli u2 ON u2.who = 2 AND u2.client = c.id
		LEFT JOIN uni_int u3 ON u3.who = 7001 AND u3.fid = c.id
		LEFT JOIN uni_str u4 ON u4.who = 1309 AND u4.i1 = c.id
		WHERE c.id = @kl;

	SELECT @sP = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND who = 20;

	SELECT @sV = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND who = 21;

	SELECT @sDep = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND who IN (100, 101, 102, 150);
	
	SELECT @sVyp = 0.01 * SUM(summa) FROM client_cnt WHERE client = @kl AND who IN (200, 201);

	SET @sR = @sP - @sV;

	SET @prc = IIF(@sP = 0, 0, 100.0000000 * @sR / @sP);
END;
GO




IF OBJECT_ID ('dbo.BMIN15_LOGS', 'TF') IS NOT NULL
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




IF OBJECT_ID ('dbo.BMIN15_LOGS_MINI', 'TF') IS NOT NULL
    DROP FUNCTION dbo.BMIN15_LOGS_MINI;
GO


CREATE FUNCTION dbo.BMIN15_LOGS_MINI
(	
    @kli int
)
RETURNS @result TABLE (cdate datetime2 not null, rem varchar(1000) not null)
AS
BEGIN
	DECLARE @t1 TABLE
	(
		[cdate] [datetime2](7) NOT NULL,
		[who] [smallint] NOT NULL,
		[tag1] [int] NOT NULL,
		[tag2] [bigint] NOT NULL,
		[wmr] [varchar](24) NULL,
		[soc] [varchar](16) NULL,
		[s] [varchar](250) NULL,
		[t_sum] [bigint] NOT NULL,
		[rem] [varchar](1000) NULL
	);

	WITH src1 AS
	(
		SELECT l.dt cdate, l.who, ISNULL(l.summa, 0) t_sum, ISNULL(l.tag1, 0) tag1, ISNULL(l.tag2, 0) tag2, l.tag3, l.tag4 wmr, d.rus rem, v2.soc,
				CAST(ISNULL(l.tag1, 0) AS binary(4)) tag1_bin
			FROM log_cli l
			INNER JOIN dict_mini d ON d.who = 201 AND d.fid = l.who
			LEFT JOIN client c ON c.id = @kli
			LEFT JOIN valuta v2 ON v2.id = c.valuta
			WHERE l.client = @kli
	),
	src2 AS
	(
		SELECT cdate, who, t_sum, tag1, tag2, tag3, wmr, soc,
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
	INSERT INTO @t1 (cdate, who, tag1, tag2, wmr, soc, s, t_sum, rem)
		SELECT s.cdate, s.who, s.tag1, s.tag2, s.wmr, s.soc,
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
					WHEN s.who IN (702, 703, 707)
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
					WHEN s.who = 502 AND s.wmr IS NOT NULL
						THEN s.rem + ' ' + s.wmr
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
		SELECT cdate, who, t_sum, tag1, tag2, soc, s,
				CASE
					WHEN who IN (702, 703, 707) AND wmr IS NOT NULL
						THEN REPLACE(REPLACE(rem, '%K%', s), '%wmr%', wmr)
					WHEN who IN (702, 703, 707) THEN REPLACE(rem, '%K%', s)
					WHEN who = 517 THEN REPLACE(REPLACE(rem, '%2%', CAST(tag2 AS VARCHAR)), 'WMID=-1, ', '')
					WHEN who = 218 AND wmr IS NOT NULL THEN REPLACE(rem, '%4%', wmr)
					WHEN who IN (520, 720, 220) THEN REPLACE(rem, '%D3%', s)
					WHEN who IN (200, 201, 341)
						THEN REPLACE(rem, '%2s%', CAST(CAST(0.01 * ABS(tag2) AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					ELSE rem END rem
			FROM @t1
	),
	src4 AS
	(
		SELECT cdate, who, t_sum, soc,
				CASE
					WHEN who IN (702, 703, 707) AND tag2 IS NOT NULL THEN REPLACE(rem, '%wmid%', tag2)
					WHEN who IN (200,301,309,331,341,509,512,517,551,552,553,700)
						THEN REPLACE(rem, '%1%', CAST(tag1 AS VARCHAR))
					WHEN who IN (207,217,218,220,502,506,507,508,518,520)
						THEN REPLACE(rem, '%2%', CAST(tag2 AS VARCHAR))
					WHEN who IN (418,503,530,550,999) AND s IS NOT NULL THEN
						REPLACE(rem, '%d%', s)
					ELSE rem END rem
			FROM src3
	)
	INSERT INTO @result (cdate, rem)
		SELECT cdate, 
				CASE WHEN t_sum IS NOT NULL AND who IN (300,301,341,418,502,503,507,508,509,512,518,520,700,720) THEN
						REPLACE(rem, '%s%', CAST(CAST(0.01 * ABS(t_sum) AS DECIMAL(15,2)) AS VARCHAR) + ' ' + soc)
					ELSE rem END rem
			FROM src4
			OPTION (MAXDOP 1);

	RETURN;
END;
GO




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




IF OBJECT_ID ('dbo.BMIN15_QIWI_PAY_BID', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN15_QIWI_PAY_BID;
GO

CREATE PROCEDURE dbo.BMIN15_QIWI_PAY_BID
    @client int,
    @zid int,
    @summa decimal(15,2),
    @id int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	INSERT INTO bill (client, who, zdate, ex_fl, summa, tag1) VALUES (@client, 708, SYSDATETIME(), 0, @summa, @zid);
	SET @id = SCOPE_IDENTITY();
END;
GO




IF OBJECT_ID ('dbo.BMIN15_UNI_PAY_BID', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN15_UNI_PAY_BID;
GO

CREATE PROCEDURE dbo.BMIN15_UNI_PAY_BID
    @client int,
    @zid int,
    @summa decimal(15,2),
    @who int,
    @id int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
	-- 16.03.2015  Генерация транзакции для выплаты (Универсально). Данные можно подчищать (пару недель)
	-- Киви - 707 (старый) 708 новый
	-- AccentPay - WM - 503
	-- AccentPay - ЯД - 504

	INSERT INTO bill (client, who, zdate, ex_fl, summa, tag1) VALUES (@client, @who, SYSDATETIME(), 0, @summa, @zid);
	SET @id = SCOPE_IDENTITY();
END;
GO




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




IF OBJECT_ID ('dbo.I12_CLIENT_ST', 'P') IS NOT NULL
    DROP PROCEDURE dbo.I12_CLIENT_ST;
GO

CREATE PROCEDURE dbo.I12_CLIENT_ST
    @clId int
AS
BEGIN
    SET NOCOUNT ON;

	SELECT ISNULL(d.rus, '???') AS who_str, c.who, c.cnt, 0.01 * c.summa summa, c.last_dt, c.last_ip
		FROM client_cnt c
		LEFT JOIN dict_mini d ON d.who = 4200 AND d.fid = c.who
		WHERE c.client = @clId
		ORDER BY c.who;
END;
GO
