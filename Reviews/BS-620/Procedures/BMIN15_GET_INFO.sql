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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN15_GET_INFO",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    string name1 = dr.GetStringOrDefault("NAME1");
--                    double balans = dr.GetDoubleOrDefault("BALANS");
--                    string city = dr.GetStringOrDefault("CITY");
--                    string country = dr.GetStringOrDefault("COUNTRY");
--                    string email = dr.GetStringOrDefault("EMAIL");
--                    string secret1 = dr.GetStringOrDefault("SECRET1");
--                    string secret2 = dr.GetStringOrDefault("SECRET2");
--                    DateTime dtr = dr.GetDateTimeOrNull("DTR");
--                    string vn = dr.GetStringOrDefault("VN");
--                    double sDep = dr.GetDoubleOrDefault("S_DEP");
--                    double sVyp = dr.GetDoubleOrDefault("S_VYP");
--                    double sP = dr.GetDoubleOrDefault("S_P");
--                    double sV = dr.GetDoubleOrDefault("S_V");
--                    double sR = dr.GetDoubleOrDefault("S_R");
--                    double prc = dr.GetDoubleOrDefault("PRC");
--                    DateTime cpDt = dr.GetDateTimeOrNull("CP_DT");
--                    DateTime u2Dt = dr.GetDateTimeOrNull("U2_DT");
--                    short u2Fl = dr.GetInt16OrDefault("U2_FL");
--                    DateTime u2D2 = dr.GetDateTimeOrNull("U2_D2");
--                    int fl2 = dr.GetInt32OrDefault("FL2");
--                    int maxsum = dr.GetInt32OrDefault("MAXSUM");
--                    int osob = dr.GetInt32OrDefault("OSOB");
--                    string di = dr.GetStringOrDefault("DI");
--                    string dilg = dr.GetStringOrDefault("DILG");
--                    string didt = dr.GetStringOrDefault("DIDT");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}