SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





ALTER TABLE acquiring.Request ADD PaymentMethod nvarchar(100) NULL;
GO





ALTER PROCEDURE [acquiring].[CreateRequest]
	@PlaceId int,
	@EmployeeId int,
	@Amount decimal(18,2),
	@IpAddress varchar(15),
	@PaymentMethod nvarchar(100) = NULL,
	@RequestId int output
AS
BEGIN
	SET NOCOUNT ON;

	SET @RequestId = NEXT VALUE FOR acquiring.Seq_Request;
	INSERT INTO acquiring.Request(RequestId, Status, PlaceId, EmployeeId, Amount, IpAddress, PaymentMethod)
		VALUES (@RequestId, 0, @PlaceId, @EmployeeId, @Amount, @IpAddress, @PaymentMethod);
END;
GO





BEGIN TRANSACTION
GO
ALTER TABLE acquiring.Request
	DROP CONSTRAINT DF__acquiring_Request__RequestId
GO
CREATE TABLE acquiring.Tmp_Request
	(
	RequestId int NOT NULL,
	Status tinyint NOT NULL,
	PlaceId int NOT NULL,
	EmployeeId int NULL,
	Amount decimal(18, 2) NOT NULL,
	OrderId varchar(50) NULL,
	IpAddress varchar(15) NULL,
	PaymentMethod nvarchar(100) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE acquiring.Tmp_Request SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE acquiring.Tmp_Request ADD CONSTRAINT
	DF__acquiring_Request__RequestId DEFAULT (NEXT VALUE FOR [acquiring].[Seq_Request]) FOR RequestId
GO
IF EXISTS(SELECT * FROM acquiring.Request)
	 EXEC('INSERT INTO acquiring.Tmp_Request (RequestId, Status, PlaceId, EmployeeId, Amount, IpAddress, PaymentMethod)
		SELECT RequestId, Status, PlaceId, EmployeeId, Amount, IpAddress, PaymentMethod FROM acquiring.Request WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE acquiring.Request
GO
EXECUTE sp_rename N'acquiring.Tmp_Request', N'Request', 'OBJECT' 
GO
ALTER TABLE acquiring.Request ADD CONSTRAINT
	PK_acquiring_Request PRIMARY KEY CLUSTERED 
	(
	RequestId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
GO





CREATE PROCEDURE [acquiring].[SaveOrderId]
	@RequestId int,
	@OrderId varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE acquiring.Request SET OrderId = @OrderId WHERE RequestId = @RequestId;
	IF @@ROWCOUNT = 0
		THROW 50000, 'Платёж не найден', 1;  
END;
GO





CREATE PROCEDURE [acquiring].[FailRequest]
	@RequestId int
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE acquiring.Request SET Status = 1 WHERE RequestId = @RequestId;
	IF @@ROWCOUNT = 0
		THROW 50000, 'Платёж не найден', 1;  
END;
GO





INSERT INTO [payment].[DataSource] ([Id], [Code], [DisplayName]) VALUES (4, 'SBBACQ', N'Сбербанк. Эквайринг');
GO

INSERT INTO [payment].[Provider] ([Id], [Code], [DisplayName]) VALUES (7, 'SBBAGP', N'Сбербанк. Google Pay');
INSERT INTO [payment].[Provider] ([Id], [Code], [DisplayName]) VALUES (8, 'SBBACD', N'Сбербанк. Платёжная форма');
INSERT INTO [payment].[Provider] ([Id], [Code], [DisplayName]) VALUES (9, 'SBBAAP', N'Сбербанк. Apple Pay');
GO





ALTER PROCEDURE [acquiring].[FailRequest]
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

		IF @Status = 1
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN 0;
		END;

		IF @Status NOT IN (0, 2)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;
		
		UPDATE acquiring.Request SET Status = 1 WHERE RequestId = @RequestId;

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





CREATE PROCEDURE [acquiring].[SucceedRequest]
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

		IF @Status = 1
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF @Status != 0
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN 0;
		END;
		
		UPDATE acquiring.Request SET Status = 2 WHERE RequestId = @RequestId;

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





BEGIN TRANSACTION
GO
ALTER TABLE acquiring.Request
	DROP CONSTRAINT DF__acquiring_Request__RequestId
GO
CREATE TABLE acquiring.Tmp_Request
	(
	RequestId int NOT NULL,
	Status tinyint NOT NULL,
	CreateDateTime datetime2(7) NOT NULL,
	PlaceId int NOT NULL,
	EmployeeId int NULL,
	Amount decimal(18, 2) NOT NULL,
	OrderId varchar(50) NULL,
	IpAddress varchar(15) NULL,
	PaymentId bigint NULL,
	PaymentMethod nvarchar(100) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE acquiring.Tmp_Request SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE acquiring.Tmp_Request ADD CONSTRAINT
	DF__acquiring_Request__RequestId DEFAULT (NEXT VALUE FOR [acquiring].[Seq_Request]) FOR RequestId
GO
ALTER TABLE acquiring.Tmp_Request ADD CONSTRAINT
	DF_Request_CreateDateTime DEFAULT sysdatetime() FOR CreateDateTime
GO
IF EXISTS(SELECT * FROM acquiring.Request)
	 EXEC('INSERT INTO acquiring.Tmp_Request (RequestId, Status, PlaceId, EmployeeId, Amount, OrderId, IpAddress, PaymentMethod)
		SELECT RequestId, Status, PlaceId, EmployeeId, Amount, OrderId, IpAddress, PaymentMethod FROM acquiring.Request WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE acquiring.Request
GO
EXECUTE sp_rename N'acquiring.Tmp_Request', N'Request', 'OBJECT' 
GO
ALTER TABLE acquiring.Request ADD CONSTRAINT
	PK_acquiring_Request PRIMARY KEY CLUSTERED 
	(
	RequestId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT





CREATE SEQUENCE [acquiring].[Seq_RequestLog_SeqNum] AS [bigint] START WITH 1 INCREMENT BY 1 MINVALUE 1;
GO





CREATE TABLE acquiring.RequestLog
(
	RequestId int not null,
	SeqNum bigint not null CONSTRAINT DF__acquiring_RequestLog__SeqNum DEFAULT NEXT VALUE FOR acquiring.Seq_RequestLog_SeqNum,
	LogDateTime datetime2(7) not null CONSTRAINT DF__acquiring_RequestLog__LogDateTime DEFAULT sysdatetime(),
	Status tinyint not null,
	CONSTRAINT PK_acquiring_RequestLog PRIMARY KEY CLUSTERED (RequestId, SeqNum)
);
GO





ALTER PROCEDURE [acquiring].[CreateRequest]
	@PlaceId int,
	@EmployeeId int,
	@Amount decimal(18,2),
	@IpAddress varchar(15),
	@PaymentMethod nvarchar(100) = NULL,
	@RequestId int output
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @dt datetime2(7) = sysdatetime();
		SET @RequestId = NEXT VALUE FOR acquiring.Seq_Request;
		INSERT INTO acquiring.Request(RequestId, Status, CreateDateTime, PlaceId, EmployeeId, Amount, IpAddress, PaymentMethod)
			VALUES (@RequestId, 0, @dt, @PlaceId, @EmployeeId, @Amount, @IpAddress, @PaymentMethod);
		INSERT INTO acquiring.RequestLog (RequestId, LogDateTime, Status) VALUES (@RequestId, @dt, 0);
		
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO





ALTER PROCEDURE [acquiring].[SaveOrderId]
	@RequestId int,
	@OrderId varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE acquiring.Request SET OrderId = @OrderId, Status = 1 WHERE RequestId = @RequestId;
		IF @@ROWCOUNT = 0
			THROW 50000, 'Платёж не найден', 1;

		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 1);
		
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO





ALTER PROCEDURE [acquiring].[FailRequest]
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

		IF @Status = 2
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN 0;
		END;

		IF @Status != 1
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;
		
		UPDATE acquiring.Request SET Status = 2 WHERE RequestId = @RequestId;
		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 2);

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

		IF @Status = 2
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF @Status != 1
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





CREATE NONCLUSTERED INDEX IX_GetRequests ON acquiring.Request (RequestId) WHERE Status = 3;
GO





CREATE PROCEDURE payment.SbAcquiring_GetRequests
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP(50) RequestId, Status, CreateDateTime, PlaceId, EmployeeId, Amount, OrderId, IpAddress, PaymentId, PaymentMethod
		FROM acquiring.Request
		WHERE Status = 3
		ORDER BY RequestId;
END;
GO





CREATE PROCEDURE payment.SbAcquiring_CompleteRequest
	@RequestId int,
	@PaymentId int
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE acquiring.Request SET PaymentId = @PaymentId WHERE RequestId = @RequestId;
		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 4);

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





CREATE PROCEDURE payment.SbAcquiring_FailedToCompleteRequest
	@RequestId int
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 5);

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





BEGIN TRANSACTION
GO
ALTER TABLE acquiring.Request
	DROP CONSTRAINT DF__acquiring_Request__RequestId
GO
ALTER TABLE acquiring.Request
	DROP CONSTRAINT DF_Request_CreateDateTime
GO
CREATE TABLE acquiring.Tmp_Request
	(
	RequestId int NOT NULL,
	Type tinyint NOT NULL,
	Status tinyint NOT NULL,
	CreateDateTime datetime2(7) NOT NULL,
	PlaceId int NOT NULL,
	EmployeeId int NULL,
	Amount decimal(18, 2) NOT NULL,
	OrderId varchar(50) NULL,
	IpAddress varchar(15) NULL,
	PaymentId bigint NULL,
	PaymentMethod nvarchar(100) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE acquiring.Tmp_Request SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE acquiring.Tmp_Request ADD CONSTRAINT
	DF__acquiring_Request__RequestId DEFAULT (NEXT VALUE FOR [acquiring].[Seq_Request]) FOR RequestId
GO
ALTER TABLE acquiring.Tmp_Request ADD CONSTRAINT
	DF_Request_Type DEFAULT 0 FOR Type
GO
ALTER TABLE acquiring.Tmp_Request ADD CONSTRAINT
	DF_Request_CreateDateTime DEFAULT (sysdatetime()) FOR CreateDateTime
GO
IF EXISTS(SELECT * FROM acquiring.Request)
	 EXEC('INSERT INTO acquiring.Tmp_Request (RequestId, Status, CreateDateTime, PlaceId, EmployeeId, Amount, OrderId, IpAddress, PaymentId, PaymentMethod)
		SELECT RequestId, Status, CreateDateTime, PlaceId, EmployeeId, Amount, OrderId, IpAddress, PaymentId, PaymentMethod FROM acquiring.Request WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE acquiring.Request
GO
EXECUTE sp_rename N'acquiring.Tmp_Request', N'Request', 'OBJECT' 
GO
ALTER TABLE acquiring.Request ADD CONSTRAINT
	PK_acquiring_Request PRIMARY KEY CLUSTERED 
	(
	RequestId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_GetRequests ON acquiring.Request
	(
	RequestId
	) WHERE ([Status]=(3))
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
GO





ALTER PROCEDURE [acquiring].[CreateRequest]
	@Type tinyint,
	@PlaceId int,
	@EmployeeId int,
	@Amount decimal(18,2),
	@IpAddress varchar(15),
	@PaymentMethod nvarchar(100) = NULL,
	@RequestId int output
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @dt datetime2(7) = sysdatetime();
		SET @RequestId = NEXT VALUE FOR acquiring.Seq_Request;
		INSERT INTO acquiring.Request(RequestId, Type, Status, CreateDateTime, PlaceId, EmployeeId, Amount, IpAddress, PaymentMethod)
			VALUES (@RequestId, @Type, 0, @dt, @PlaceId, @EmployeeId, @Amount, @IpAddress, @PaymentMethod);
		INSERT INTO acquiring.RequestLog (RequestId, LogDateTime, Status) VALUES (@RequestId, @dt, 0);
		
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO





ALTER PROCEDURE [payment].[SbAcquiring_GetRequests]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP(50) RequestId, Type, Status, CreateDateTime, PlaceId, EmployeeId, Amount, OrderId, IpAddress, PaymentId, PaymentMethod
		FROM acquiring.Request
		WHERE Status = 3
		ORDER BY RequestId;
END;
GO





ALTER PROCEDURE [payment].[SbAcquiring_CompleteRequest]
	@RequestId int,
	@PaymentId int
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE acquiring.Request SET PaymentId = @PaymentId, Status = 4 WHERE RequestId = @RequestId;
		INSERT INTO acquiring.RequestLog (RequestId, Status) VALUES (@RequestId, 4);

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





