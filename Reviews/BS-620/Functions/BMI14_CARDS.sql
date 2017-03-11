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


--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetCommand("Select * FROM dbo.BMI14_CARDS(@client)",  conn))
--        {
--            cmd.AddIntParam("@client", request.client);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    int id = dr.GetInt32OrDefault("ID");
--                    DateTime dateP = dr.GetDateTimeOrNull("DATE_P");
--                    DateTime dateV = dr.GetDateTimeOrNull("DATE_V");
--                    decimal sumP = dr.GetDecimalOrDefault("SUM_P");
--                    decimal sumV = dr.GetDecimalOrDefault("SUM_V");
--                    short st = dr.GetInt16OrDefault("ST");
--                    short isArh = dr.GetInt16OrDefault("IS_ARH");
--                    int idStavki = dr.GetInt32OrDefault("ID_STAVKI");
--                    int typeS = dr.GetInt32OrDefault("TYPE_S");
--                    string sysof = dr.GetStringOrDefault("SYSOF");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}