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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMI14_STAVKI",  conn))
--        {
--            cmd.AddIntParam("@idStavki", request.idStavki);
--            cmd.AddSmallIntParam("@isArh", request.isArh);

--            using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
--            {
--                while (dr.Read())
--                {
--                    string k1 = dr.GetStringOrDefault("K1");
--                    string k2 = dr.GetStringOrDefault("K2");
--                    DateTime dateL = dr.GetDateTimeOrNull("DATE_L");
--                    string betName = dr.GetStringOrDefault("BET_NAME");
--                    decimal kf = dr.GetDecimalOrDefault("KF");
--                    short ready = dr.GetInt16OrDefault("READY");
--                }
--            }
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}