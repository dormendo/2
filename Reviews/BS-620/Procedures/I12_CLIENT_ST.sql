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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("I12_CLIENT_ST",  conn))
--        {
--            cmd.AddIntParam("@clId", request.clId);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    string whoStr = dr.GetStringOrDefault("WHO_STR");
--                    short who = dr.GetInt16OrDefault("WHO");
--                    int cnt = dr.GetInt32OrDefault("CNT");
--                    decimal summa = dr.GetDecimalOrDefault("SUMMA");
--                    DateTime lastDt = dr.GetDateTimeOrNull("LAST_DT");
--                    int lastIp = dr.GetInt32OrDefault("LAST_IP");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}