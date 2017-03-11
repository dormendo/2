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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN12_ZAP_STAT",  conn))
--        {
--            cmd.AddIntParam("@clId", request.clId);

--            SqlParameter cardSumpParam = cmd.AddDecimalParam("@cardSump", 15, 2).Output();
--            SqlParameter cardSumvParam = cmd.AddDecimalParam("@cardSumv", 15, 2).Output();
--            SqlParameter cardRazParam = cmd.AddDecimalParam("@cardRaz", 15, 2).Output();
--            SqlParameter cardPrcParam = cmd.AddDecimalParam("@cardPrc", 15, 2).Output();
--            SqlParameter totoSumpParam = cmd.AddDecimalParam("@totoSump", 15, 2).Output();
--            SqlParameter totoSumvParam = cmd.AddDecimalParam("@totoSumv", 15, 2).Output();
--            SqlParameter totoRazParam = cmd.AddDecimalParam("@totoRaz", 15, 2).Output();
--            SqlParameter totoPrcParam = cmd.AddDecimalParam("@totoPrc", 15, 2).Output();

--            await cmd.ExecuteNonQueryAsync();

--            decimal cardSump = cardSumpParam.GetDecimalOrDefault();
--            decimal cardSumv = cardSumvParam.GetDecimalOrDefault();
--            decimal cardRaz = cardRazParam.GetDecimalOrDefault();
--            decimal cardPrc = cardPrcParam.GetDecimalOrDefault();
--            decimal totoSump = totoSumpParam.GetDecimalOrDefault();
--            decimal totoSumv = totoSumvParam.GetDecimalOrDefault();
--            decimal totoRaz = totoRazParam.GetDecimalOrDefault();
--            decimal totoPrc = totoPrcParam.GetDecimalOrDefault();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}