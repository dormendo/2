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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN13_DO_WAIT",  conn))
--        {
--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int zid = dr.GetInt32OrDefault("ZID");
--                    DateTime zdate = dr.GetDateTimeOrNull("ZDATE");
--                    decimal summa = dr.GetDecimalOrDefault("SUMMA");
--                    string soc = dr.GetStringOrDefault("SOC");
--                    int client = dr.GetInt32OrDefault("CLIENT");
--                    int tid = dr.GetInt32OrDefault("TID");
--                    string remm = dr.GetStringOrDefault("REMM");
--                    int valuta = dr.GetInt32OrDefault("VALUTA");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}