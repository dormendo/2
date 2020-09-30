DECLARE @RC int
DECLARE @DisplayName nvarchar(100)
DECLARE @Address nvarchar(100)
DECLARE @City nvarchar(40)
DECLARE @Phone char(10)
DECLARE @Inn varchar(20)
DECLARE @Email varchar(50)
DECLARE @TimeZoneId varchar(32)
DECLARE @Info nvarchar(max)
DECLARE @PersonalShare tinyint
DECLARE @Weight1 tinyint
DECLARE @Weight2 tinyint
DECLARE @Weight3 tinyint
DECLARE @Weight4 tinyint
DECLARE @PlaceId int

SELECT
	@DisplayName = 'Тестовое заведение',
	@Address = 'ул. Сибирский тракт, 9/5',
	@City = 'Казань',
	@Phone = '9063236971',
	@Email = 'test@mail.ru',
	@TimeZoneId = 'RTZ2',
	@Info = 'Дополнительная информация',
	@PersonalShare = 25,
	@Weight1 = '40',
	@Weight2 = '50',
	@Weight3 = '25',
	@Weight4 = '15'

EXECUTE @RC = [admin].[CreatePlace] 
   @DisplayName
  ,@Address
  ,@City
  ,@Phone
  ,@Inn
  ,@Email
  ,@TimeZoneId
  ,@Info
  ,@PersonalShare
  ,@Weight1
  ,@Weight2
  ,@Weight3
  ,@Weight4
  ,@PlaceId output

SELECT * FROM dbo.Place WHERE PlaceID = @PlaceId;
SELECT * FROM dbo.ShareSchemeHistory WHERE PlaceId = @PlaceId;
SELECT * FROM dbo.SsGroupShareHistory WHERE ShareSchemeHistoryId IN (SELECT ShareSchemeHistoryId FROM dbo.ShareSchemeHistory WHERE PlaceId = @PlaceId)
GO

