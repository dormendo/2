DECLARE @RC int
DECLARE @PlaceId int
DECLARE @BeginDateTime datetime2(7)
DECLARE @PersonalShare tinyint
DECLARE @Weight1 tinyint
DECLARE @Weight2 tinyint
DECLARE @Weight3 tinyint
DECLARE @Weight4 tinyint

SELECT @PlaceId = 101, @BeginDateTime = NULL, @PersonalShare = 30, @Weight1 = 50, @Weight2 = 100, @Weight3 = 20, @Weight4 = 20

EXECUTE @RC = [admin].[UpdateShareScheme] 
   @PlaceId
  ,@BeginDateTime
  ,@PersonalShare
  ,@Weight1
  ,@Weight2
  ,@Weight3
  ,@Weight4



SELECT * FROM dbo.Place WHERE PlaceID = @PlaceId;
SELECT * FROM dbo.ShareSchemeHistory WHERE PlaceId = @PlaceId;
SELECT * FROM dbo.SsGroupShareHistory WHERE ShareSchemeHistoryId IN (SELECT ShareSchemeHistoryId FROM dbo.ShareSchemeHistory WHERE PlaceId = @PlaceId)
GO

