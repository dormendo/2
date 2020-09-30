SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





DROP PROCEDURE payment.SbAcquiring_FailedToCompleteRequest;
GO





ALTER PROCEDURE [acquiring].[SucceedRequest]
	@RequestId int
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @Status tinyint;
		SELECT @Status = Status FROM acquiring.Request with (updlock, rowlock) WHERE RequestId = @RequestId;

		IF @@ROWCOUNT = 0
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -1;
		END;


		IF @Status NOT IN (1, 2)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN 0;
		END;
		
		UPDATE acquiring.Request SET Status = 3 WHERE RequestId = @RequestId;
		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 3);

		COMMIT TRANSACTION;
		RETURN 0;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO
