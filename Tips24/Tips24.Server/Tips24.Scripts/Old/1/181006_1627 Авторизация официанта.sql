ALTER TABLE dbo.Employee
	DROP CONSTRAINT DF__Employee__EmployeeId
GO
ALTER TABLE dbo.Employee
	DROP CONSTRAINT DF__Employee__RegisterDate
GO
ALTER TABLE dbo.Employee
	DROP CONSTRAINT DF__Employee__IsDisabled
GO
CREATE TABLE dbo.Tmp_Employee
	(
	EmployeeId int NOT NULL,
	PlaceId int NULL,
	Phone char(10) NOT NULL,
	RegisterDateTime datetime NOT NULL,
	IsVerified bit NOT NULL,
	IsDisabled bit NOT NULL
	)
GO
ALTER TABLE dbo.Tmp_Employee ADD CONSTRAINT
	DF__Employee__EmployeeId DEFAULT (NEXT VALUE FOR [dbo].[Seq_Employee]) FOR EmployeeId
GO
ALTER TABLE dbo.Tmp_Employee ADD CONSTRAINT
	DF__Employee__RegisterDate DEFAULT getdate() FOR RegisterDateTime
GO
ALTER TABLE dbo.Tmp_Employee ADD CONSTRAINT
	DF__Employee__IsVerified DEFAULT 0 FOR IsVerified
GO
ALTER TABLE dbo.Tmp_Employee ADD CONSTRAINT
	DF__Employee__IsDisabled DEFAULT 0 FOR IsDisabled
GO
IF EXISTS(SELECT * FROM dbo.Employee)
	 EXEC('INSERT INTO dbo.Tmp_Employee (EmployeeId, PlaceId, Phone, RegisterDateTime, IsDisabled)
		SELECT EmployeeId, PlaceId, Phone, CONVERT(datetime, RegisterDate), IsDisabled FROM dbo.Employee WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.Employee
GO
EXECUTE sp_rename N'dbo.Tmp_Employee', N'Employee', 'OBJECT' 
GO
ALTER TABLE dbo.Employee ADD CONSTRAINT
	PK_Employee PRIMARY KEY CLUSTERED 
	(
	EmployeeId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_Employee_Phone ON dbo.Employee
	(
	Phone,
	IsDisabled
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_Employee_PlaceId ON dbo.Employee
	(
	PlaceId,
	IsDisabled
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO










CREATE TABLE dbo.VerificationCode
	(
	UserId int NOT NULL,
	UserType tinyint NOT NULL,
	Code char(6) NOT NULL,
	GenerationDateTime datetime NOT NULL
	)
GO
ALTER TABLE dbo.VerificationCode ADD CONSTRAINT
	DF__VerificationCode__GenerationDateTime DEFAULT getdate() FOR GenerationDateTime
GO
ALTER TABLE dbo.VerificationCode ADD CONSTRAINT
	PK_VerificationCode PRIMARY KEY CLUSTERED 
	(
	UserId,
	UserType
	)
GO





ALTER TABLE dbo.EmployeeAuth
	DROP CONSTRAINT DF__EmployeeA__FailedAuthCount
GO
CREATE TABLE dbo.Tmp_EmployeeAuth
	(
	EmployeeId int NOT NULL,
	Phone char(10) NOT NULL,
	PinCode char(4) NOT NULL,
	PermanentKey char(10) NULL,
	SpecialKey char(10) NULL,
	SpecialKeyLastAccessDt datetime NULL,
	FailedAuthCount tinyint NOT NULL,
	LastFailedAuthDt datetime NULL
	)
GO
ALTER TABLE dbo.Tmp_EmployeeAuth ADD CONSTRAINT
	DF__EmployeeA__FailedAuthCount DEFAULT ((0)) FOR FailedAuthCount
GO
IF EXISTS(SELECT * FROM dbo.EmployeeAuth)
	 EXEC('INSERT INTO dbo.Tmp_EmployeeAuth (EmployeeId, Phone, SpecialKeyLastAccessDt, FailedAuthCount, LastFailedAuthDt)
		SELECT EmployeeId, Phone, CONVERT(datetime, SpecialKeyLastAccessDt), FailedAuthCount, CONVERT(datetime, LastFailedAuthDt) FROM dbo.EmployeeAuth WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.EmployeeAuth
GO
EXECUTE sp_rename N'dbo.Tmp_EmployeeAuth', N'EmployeeAuth', 'OBJECT' 
GO
ALTER TABLE dbo.EmployeeAuth ADD CONSTRAINT
	PK_EmployeeAuth PRIMARY KEY CLUSTERED 
	(
	Phone
	)
GO
CREATE NONCLUSTERED INDEX IX_EmployeeAuth_EmployeeId ON dbo.EmployeeAuth
	(
	EmployeeId
	)
GO






ALTER TABLE dbo.Place
	DROP CONSTRAINT DF__Place__PlaceID
GO
CREATE TABLE dbo.Tmp_Place
	(
	PlaceID int NOT NULL,
	PrincipalManagerId int NULL,
	DisplayName nvarchar(100) NOT NULL,
	Address nvarchar(100) NOT NULL,
	City nvarchar(40) NOT NULL,
	Phone char(10) NOT NULL,
	Email varchar(50) NOT NULL,
	Info nvarchar(MAX) NULL
	)
GO
ALTER TABLE dbo.Tmp_Place ADD CONSTRAINT
	DF__Place__PlaceID DEFAULT (NEXT VALUE FOR [dbo].[Seq_Place]) FOR PlaceID
GO
IF EXISTS(SELECT * FROM dbo.Place)
	 EXEC('INSERT INTO dbo.Tmp_Place (PlaceID, PrincipalManagerId, DisplayName, Address, City, Phone, Email, Info)
		SELECT PlaceID, PrincipalManager, DisplayName, Address, City, Phone, Email, Info FROM dbo.Place WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.Place
GO
EXECUTE sp_rename N'dbo.Tmp_Place', N'Place', 'OBJECT' 
GO
ALTER TABLE dbo.Place ADD CONSTRAINT
	PK_Place PRIMARY KEY CLUSTERED 
	(
	PlaceID
	)

GO
