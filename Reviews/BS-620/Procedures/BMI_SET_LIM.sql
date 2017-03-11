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


--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMI_SET_LIM",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);
--            cmd.AddIntParam("@sport", request.sport);
--            cmd.AddBigIntParam("@summa", request.summa);
--            cmd.AddSmallIntParam("@val2", request.val2);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}