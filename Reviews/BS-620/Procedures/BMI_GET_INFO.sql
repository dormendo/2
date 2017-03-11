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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMI_GET_INFO",  conn))
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
--            SqlParameter vnParam = cmd.AddVarCharParam("@vn", 64).Output();
--            SqlParameter sDepParam = cmd.AddFloatParam("@sDep").Output();
--            SqlParameter sVypParam = cmd.AddFloatParam("@sVyp").Output();
--            SqlParameter sPParam = cmd.AddFloatParam("@sP").Output();
--            SqlParameter sVParam = cmd.AddFloatParam("@sV").Output();
--            SqlParameter sRParam = cmd.AddFloatParam("@sR").Output();
--            SqlParameter prcParam = cmd.AddFloatParam("@prc").Output();

--            await cmd.ExecuteNonQueryAsync();

--            string name1 = name1Param.Value.ToString();
--            double balans = balansParam.GetDoubleOrDefault();
--            string city = cityParam.Value.ToString();
--            string country = countryParam.Value.ToString();
--            string email = emailParam.Value.ToString();
--            string secret1 = secret1Param.Value.ToString();
--            string secret2 = secret2Param.Value.ToString();
--            DateTime dtr = dtrParam.GetDateTimeOrNull();
--            string vn = vnParam.Value.ToString();
--            double sDep = sDepParam.GetDoubleOrDefault();
--            double sVyp = sVypParam.GetDoubleOrDefault();
--            double sP = sPParam.GetDoubleOrDefault();
--            double sV = sVParam.GetDoubleOrDefault();
--            double sR = sRParam.GetDoubleOrDefault();
--            double prc = prcParam.GetDoubleOrDefault();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}