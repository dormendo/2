IF OBJECT_ID ('dbo.BMIN11_OTKAZ', 'P') IS NOT NULL
    DROP PROCEDURE dbo.BMIN11_OTKAZ;
GO

CREATE PROCEDURE dbo.BMIN11_OTKAZ
    @idzap int,
    @fl int,
    @otkaz varchar(250),
    @f2 int
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @idcl INT;
	DECLARE @bal BIGINT;
	DECLARE @summ BIGINT;
	DECLARE @fl2 INT;
	DECLARE @tag1 INT;
	DECLARE @pmd SMALLINT;
	DECLARE @idd INT;
	DECLARE @outerTranCount int;

	BEGIN TRY
		SET @outerTranCount = @@TRANCOUNT;
		IF @outerTranCount = 0
			BEGIN TRANSACTION;

		SELECT @idcl = i.client, @summ = i.summa, @bal = c.balans, @fl2 = i.flag, @tag1 = i.tag1
			FROM I_KZAP i with(updlock)
			INNER JOIN client c with(updlock) ON c.id = i.client
			WHERE i.id = @idzap;

		IF (@idcl IS NULL OR @fl2 > @F2)
			RETURN;
	
		IF (@tag1 IS NOT NULL AND @tag1 > @summ)
			SET @summ = @tag1;
	
		SET @bal = @bal + @summ;

		UPDATE I_KZAP
			SET flag = @fl, otkaz = @otkaz, @pmd = paymethod
			WHERE id = @idzap;

		UPDATE client SET balans = @bal WHERE id = @idcl;

		SELECT TOP(1) @idd = id FROM uni_d with(serializable) WHERE who = 200 AND v = @otkaz;

		IF (@idd IS NULL)
		BEGIN
			INSERT INTO uni_d (who, v) VALUES (200, @otkaz);
			SET @idd = SCOPE_IDENTITY();
		END;

		INSERT INTO log_cli (who, client, summa, bal, tag1, tag2)
			VALUES (530, @idcl, @summ, @bal, @idd, @idzap);

		IF (@pmd = 2)
		BEGIN
			MERGE client_cnt trg
				USING (SELECT @idcl, 201) AS src(client, who) ON trg.client = src.client AND trg.who = src.who
				WHEN NOT MATCHED THEN
					INSERT (client, who, last_dt, last_ip, cnt, summa) VALUES (src.client, src.who, SYSDATETIME(), 0, -1, -@summ)
				WHEN MATCHED THEN
					UPDATE SET last_dt = SYSDATETIME(), last_ip = 0, cnt = cnt - 1, summa = summa - @summ;
		END;
		
		IF @outerTranCount = 0
			COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 AND @outerTranCount = 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
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
--        using (SqlCommand cmd = SqlServer.GetSpCommand("BMIN11_OTKAZ",  conn))
--        {
--            cmd.AddIntParam("@idzap", request.idzap);
--            cmd.AddIntParam("@fl", request.fl);
--            cmd.AddVarCharParam("@otkaz", 250, request.otkaz);
--            cmd.AddIntParam("@f2", request.f2);

--            await cmd.ExecuteNonQueryAsync();
--        }
--    }
--    catch (Exception e)
--    {

--    }
--}