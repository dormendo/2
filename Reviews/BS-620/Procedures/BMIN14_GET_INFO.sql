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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN14_GET_INFO",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);

--            SqlParameter name1Param = cmd.AddVarCharParam("@name1", 250).Output();
--            SqlParameter balansParam = cmd.AddFloatParam("@balans").Output();
--            SqlParameter cityParam = cmd.AddVarCharParam("@city", 80).Output();
--            SqlParameter countryParam = cmd.AddVarCharParam("@country", 64).Output();
--            SqlParameter emailParam = cmd.AddVarCharParam("@email", 80).Output();
--            SqlParameter secret1Param = cmd.AddVarCharParam("@secret1", 250).Output();
--            SqlParameter secret2Param = cmd.AddVarCharParam("@secret2", 250).Output();
--            SqlParameter dtrParam = cmd.AddDateTime2Param("@dtr").Output();
--            SqlParameter socParam = cmd.AddVarCharParam("@soc", 16).Output();
--            SqlParameter sDepParam = cmd.AddFloatParam("@sDep").Output();
--            SqlParameter sVypParam = cmd.AddFloatParam("@sVyp").Output();
--            SqlParameter sPParam = cmd.AddFloatParam("@sP").Output();
--            SqlParameter sVParam = cmd.AddFloatParam("@sV").Output();
--            SqlParameter sRParam = cmd.AddFloatParam("@sR").Output();
--            SqlParameter prcParam = cmd.AddFloatParam("@prc").Output();
--            SqlParameter cpDtParam = cmd.AddDateTime2Param("@cpDt").Output();
--            SqlParameter u2DtParam = cmd.AddDateTime2Param("@u2Dt").Output();
--            SqlParameter u2FlParam = cmd.AddSmallIntParam("@u2Fl").Output();
--            SqlParameter u2D2Param = cmd.AddDateTime2Param("@u2D2").Output();
--            SqlParameter maxsumParam = cmd.AddIntParam("@maxsum").Output();
--            SqlParameter osobParam = cmd.AddIntParam("@osob").Output();
--            SqlParameter diParam = cmd.AddVarCharParam("@di", 255).Output();
--            SqlParameter dilgParam = cmd.AddVarCharParam("@dilg", 255).Output();
--            SqlParameter didtParam = cmd.AddVarCharParam("@didt", 255).Output();
--            SqlParameter passParam = cmd.AddVarCharParam("@pass", 255).Output();
--            SqlParameter envParam = cmd.AddIntParam("@env").Output();
--            SqlParameter fl1Param = cmd.AddIntParam("@fl1").Output();
--            SqlParameter fl2Param = cmd.AddIntParam("@fl2").Output();
--            SqlParameter dtrozParam = cmd.AddDateParam("@dtroz").Output();

--            await cmd.ExecuteNonQueryAsync();

--            string name1 = name1Param.Value.ToString();
--            double balans = balansParam.GetDoubleOrDefault();
--            string city = cityParam.Value.ToString();
--            string country = countryParam.Value.ToString();
--            string email = emailParam.Value.ToString();
--            string secret1 = secret1Param.Value.ToString();
--            string secret2 = secret2Param.Value.ToString();
--            DateTime dtr = dtrParam.GetDateTimeOrNull();
--            string soc = socParam.Value.ToString();
--            double sDep = sDepParam.GetDoubleOrDefault();
--            double sVyp = sVypParam.GetDoubleOrDefault();
--            double sP = sPParam.GetDoubleOrDefault();
--            double sV = sVParam.GetDoubleOrDefault();
--            double sR = sRParam.GetDoubleOrDefault();
--            double prc = prcParam.GetDoubleOrDefault();
--            DateTime cpDt = cpDtParam.GetDateTimeOrNull();
--            DateTime u2Dt = u2DtParam.GetDateTimeOrNull();
--            short u2Fl = u2FlParam.GetInt16OrDefault();
--            DateTime u2D2 = u2D2Param.GetDateTimeOrNull();
--            int maxsum = maxsumParam.GetInt32OrDefault();
--            int osob = osobParam.GetInt32OrDefault();
--            string di = diParam.Value.ToString();
--            string dilg = dilgParam.Value.ToString();
--            string didt = didtParam.Value.ToString();
--            string pass = passParam.Value.ToString();
--            int env = envParam.GetInt32OrDefault();
--            int fl1 = fl1Param.GetInt32OrDefault();
--            int fl2 = fl2Param.GetInt32OrDefault();
--            DateTime dtroz = dtrozParam.GetDateTimeOrNull();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}