SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

USE [tips]
GO

DROP PROCEDURE [dbo].[Employee_CheckBeforeRegistration];
GO

CREATE TABLE [dbo].[EmployeeRegistrationLink]
(
	[PlaceId] [int] NOT NULL,
	[CreateDateTime] [datetime] NOT NULL,
	[LinkParameter] [uniqueidentifier] NOT NULL,
	CONSTRAINT [PK_EmployeeRegistrationLink] PRIMARY KEY CLUSTERED 
	(
		[LinkParameter] ASC
	)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX IX_EmployeeRegistrationLink_PlaceId ON dbo.EmployeeRegistrationLink (PlaceId);
GO



CREATE OR ALTER PROCEDURE dbo.Employee_FollowRegistrationLink
	@LinkParameter uniqueidentifier,
	@PermanentKey char(10),
	@LinkPlaceId int output,
	@LinkPlaceName nvarchar(100) output,
	@LinkPlaceAddress nvarchar(100) output,
	@LinkPlaceCity nvarchar(40) output,
	@EmployeeId int output,
	@EmployeePlaceId int output,
	@EmployeeIsDisabled bit output
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @LinkCreateDateTime datetime;

	SELECT @LinkPlaceId = PlaceId, @LinkCreateDateTime = CreateDateTime FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
	IF (@@ROWCOUNT = 0)
		RETURN -1;

	IF (DATEDIFF(minute, @LinkCreateDateTime, GETDATE()) > 60 * 24)
		RETURN -2;

	IF (@PermanentKey IS NOT NULL)
	BEGIN
		SELECT @EmployeeId = EmployeeId FROM dbo.EmployeeAuth WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
			RETURN -3;
	END;

	SELECT @LinkPlaceName = DisplayName, @LinkPlaceAddress = Address, @LinkPlaceCity = City FROM dbo.Place WHERE PlaceId = @LinkPlaceId;

	IF (@EmployeeId IS NOT NULL)
		SELECT @EmployeePlaceId = PlaceId, @EmployeeIsDisabled = IsDisabled FROM dbo.Employee WHERE EmployeeId = @EmployeeId;

	RETURN 0;
END;
GO
