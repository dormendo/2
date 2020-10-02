IF OBJECT_ID ('dbo.UpdateCashboxClientFields', 'P') IS NOT NULL 
    DROP PROCEDURE dbo.UpdateCashboxClientFields;
GO

CREATE PROCEDURE dbo.UpdateCashboxClientFields
    @kassaId int,
    @clientId int,
    @docDate date,
    @docFrom varchar(250),
    @tel varchar(96),
    @hasStavkomat bit OUTPUT,
    @smsCode varchar(6) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @outerTranCount int;

    IF EXISTS (SELECT * FROM SmClient WHERE Id = @clientId)
        RETURN 2; -- для клиента ИРМ не должна быть вызвана данная команда
  
    SELECT @hasStavkomat = IIF(COUNT(*) > 0, 1, 0)
        FROM KASSA K
        WHERE IsEnabled = 1 AND KassaOwnerId = @kassaId

    BEGIN TRY
        SET @outerTranCount = @@TRANCOUNT;
        IF @outerTranCount = 0
            BEGIN TRANSACTION;
        
        IF @hasStavkomat = 1
        BEGIN
            IF EXISTS (SELECT * FROM cli_kassa C WITH(UPDLOCK) INNER JOIN SmClient S WITH(UPDLOCK, SERIALIZABLE) ON S.Id = C.Id WHERE C.TEL = @tel) 
            BEGIN
                IF @outerTranCount = 0
                    ROLLBACK TRANSACTION;
                RETURN 1;
            END;

            SET @smsCode  = ROUND((899999  * RAND() + 100000), 0); --генерация смс-кода

            -- создать клиента
            INSERT INTO [dbo].[SmClient]([Id], [Password], [ChangePswOnNextLogin], [IsPaymentDisabled], [IsEnabled], [Balance], [LiveDelay])
                VALUES(@clientId, @smsCode, 1, 0, 1, 0, 10);

            -- создать лог
            INSERT INTO SmClientResults(Id)
                VALUES(@clientId);
        END;

        UPDATE cli_kassa SET doc_from = @docFrom, doc_date = @docDate, tel = @tel WHERE id = @clientId

        IF @outerTranCount = 0
            COMMIT TRANSACTION;

        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 AND @outerTranCount = 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
