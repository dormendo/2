SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [payment].[LoadShareData]
	@PlaceId int,
	@EmployeeId int,
	@PaymentDateTime datetime2(7),
	@IsTimeSpecified bit,
	@SystemCommission decimal(4,2) output,
	@PlaceDisplayName nvarchar(100) output,
	@ShareSchemeHistoryId int output,
	@PersonalShare tinyint output,
	@EmployeeFirstName nvarchar(50) output,
	@EmployeeLastName nvarchar(50) output
AS
BEGIN
	SET NOCOUNT ON;

	SET @SystemCommission = 10;

	IF (@EmployeeId IS NOT NULL)
	BEGIN
		SELECT @EmployeeFirstName = FirstName, @EmployeeLastName = LastName
			FROM dbo.EmployeePersonalDataHistory
			WHERE EmployeeId = @EmployeeId AND @PaymentDateTime BETWEEN BeginDateTime AND EndDateTime;
	END;
	
	SELECT @ShareSchemeHistoryId = ssh.ShareSchemeHistoryId, @PlaceDisplayName = p.DisplayName, @PersonalShare = ssh.PersonalShare
		FROM dbo.Place p
		INNER JOIN dbo.ShareSchemeHistory ssh ON p.PlaceID = ssh.PlaceId
		WHERE ssh.PlaceId = @PlaceId AND @PaymentDateTime BETWEEN ssh.BeginDateTime AND ssh.EndDateTime;

	SELECT g.Name, sh.GroupId, sh.GroupWeight
		FROM dbo.SsGroupShareHistory sh
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = sh.GroupId
		WHERE sh.ShareSchemeHistoryId = @ShareSchemeHistoryId
		ORDER BY sh.GroupId;

	DECLARE @BeginOfDay datetime2(7) = CAST(CAST(@PaymentDateTime as date) as datetime2(7));
	DECLARE @BeginOfNextDay datetime2(7) = DATEADD(day, 1, @BeginOfDay);
	DECLARE @EndOfDay datetime2(7) = DATEADD(ns, -100, @BeginOfNextDay);

	IF (@IsTimeSpecified = 1)
	BEGIN
		SELECT EmployeeId, GroupId, IsManager, IsOwner,
				IIF(BeginDateTime < @BeginOfDay, @BeginOfDay, BeginDateTime) BeginDateTime,
				IIF(EndDateTime > @EndOfDay, @EndOfDay, EndDateTime) EndDateTime
			FROM
			(
				SELECT m.EmployeeId, m.GroupId, m.IsManager, m.IsOwner,
						IIF(m.BeginDateTime < t.BeginDateTime, t.BeginDateTime, m.BeginDateTime) BeginDateTime,
						IIF(m.EndDateTime > t.EndDateTime, t.EndDateTime, m.EndDateTime) EndDateTime
					FROM dbo.EmployeeMembershipHistory m
					INNER JOIN dbo.EmployeeTurnHistory t ON t.EmployeeId = m.EmployeeId AND @PaymentDateTime BETWEEN t.BeginDateTime AND t.EndDateTime
					WHERE m.PlaceId = @PlaceId AND @PaymentDateTime BETWEEN m.BeginDateTime AND m.EndDateTime AND m.IsFired = 0
			) a
			ORDER BY EmployeeId, BeginDateTime;
	END
	ELSE
	BEGIN
		SELECT EmployeeId, GroupId, IsManager, IsOwner,
				IIF(BeginDateTime < @BeginOfDay, @BeginOfDay, BeginDateTime) BeginDateTime,
				IIF(EndDateTime > @EndOfDay, @EndOfDay, EndDateTime) EndDateTime
			FROM
			(
				SELECT m.EmployeeId, m.GroupId, m.IsManager, m.IsOwner,
						IIF(m.BeginDateTime < t.BeginDateTime, t.BeginDateTime, m.BeginDateTime) BeginDateTime,
						IIF(m.EndDateTime > t.EndDateTime, t.EndDateTime, m.EndDateTime) EndDateTime
					FROM dbo.EmployeeMembershipHistory m
					INNER JOIN dbo.EmployeeTurnHistory t ON t.EmployeeId = m.EmployeeId AND
						(t.BeginDateTime BETWEEN m.BeginDateTime AND m.EndDateTime OR t.EndDateTime BETWEEN m.BeginDateTime AND m.EndDateTime)
					WHERE m.PlaceId = @PlaceId AND m.BeginDateTime <= @EndOfDay AND m.EndDateTime >= @BeginOfDay AND m.IsFired = 0
						AND t.BeginDateTime <= @EndOfDay AND t.EndDateTime >= @BeginOfDay
			) a
			ORDER BY EmployeeId, BeginDateTime;
	END;
END;
GO


-- Связать EmployeeTurnHistory c EmployeeMembershipHistory и ввести формальный признак закрытости смены