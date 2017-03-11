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



--using (SqlConnection conn = SqlServer.GetConnection())
--{
--    if (!await conn.SafeOpenAsync())
--    {
--        await context.SendErrorJsonAsync("Ошибка коннекта к базе");
--        return;
--    }


--    try
--    {
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMI_SET_MAX",  conn))
--        {
--            cmd.AddIntParam("@kl", request.kl);
--            cmd.AddBigIntParam("@summ", request.summ);
--            cmd.AddSmallIntParam("@val2", request.val2);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}