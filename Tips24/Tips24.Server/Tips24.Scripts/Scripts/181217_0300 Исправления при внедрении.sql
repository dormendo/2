ALTER SCHEMA payment TRANSFER type::dbo.OrderedGuidList;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [payment].[CheckModApiPaymentList]
	@IdList payment.OrderedGuidList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT SeqNum, Id
		FROM @IdList
		WHERE Id NOT IN (SELECT Id FROM payment.ModApiPayment)
			AND Id NOT IN (SELECT Id FROM payment.ModApiProcessedPayment)
			AND Id NOT IN (SELECT Id FROM payment.ModApiInvalidPayment)
		ORDER BY SeqNum;
END;
GO

ALTER PROCEDURE [payment].[CompleteYandexKassaRequest]
	@RequestId int,
	@KassaPaymentId varchar(50),
	@PaymentId bigint
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE payment.YandexKassaRequest SET Status = 7, KassaPaymentId = @KassaPaymentId, PaymentId = @PaymentId WHERE RequestId = @RequestId;
		INSERT INTO payment.YandexKassaRequestLog(RequestId, LogDateTime, Operation) VALUES (@RequestId, sysdatetime(), 7);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
