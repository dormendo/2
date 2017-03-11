IF OBJECT_ID ('dbo.BMIN13_SET_DICL', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN13_SET_DICL;
GO

CREATE PROCEDURE dbo.BMIN13_SET_DICL
    @id int,
    @name varchar(250),
    @login varchar(250),
    @dt varchar(250)
AS
BEGIN
    SET NOCOUNT ON;
	
	MERGE INTO uni_str trg
		USING (SELECT 1309, @id) AS src(who, i1) ON trg.who = src.who AND trg.i1 = src.i1
		WHEN NOT MATCHED THEN
			INSERT (who, i1, s1, s2, s3) VALUES (src.who, src.i1, @name, @login, @dt)
		WHEN MATCHED THEN
			UPDATE SET s1 = @name, s2 = @login, s3 = @dt;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN13_SET_DICL",  conn))
--        {
--            cmd.AddIntParam("@id", request.id);
--            cmd.AddVarCharParam("@name", 250, request.name);
--            cmd.AddVarCharParam("@login", 250, request.login);
--            cmd.AddVarCharParam("@dt", 250, request.dt);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}