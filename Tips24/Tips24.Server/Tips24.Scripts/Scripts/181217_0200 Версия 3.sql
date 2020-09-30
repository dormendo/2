USE [tipsv2]
GO
/****** Object:  User [EF3R36S25LR0JMQ\PaymentService]    Script Date: 12/17/2018 2:06:29 AM ******/
CREATE USER [EF3R36S25LR0JMQ\PaymentService] FOR LOGIN [EF3R36S25LR0JMQ\PaymentService] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [EF3R36S25LR0JMQ\TelegramBotService]    Script Date: 12/17/2018 2:06:29 AM ******/
CREATE USER [EF3R36S25LR0JMQ\TelegramBotService] FOR LOGIN [EF3R36S25LR0JMQ\TelegramBotService] WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [EF3R36S25LR0JMQ\Tips24Service]    Script Date: 12/17/2018 2:06:29 AM ******/
CREATE USER [EF3R36S25LR0JMQ\Tips24Service] FOR LOGIN [EF3R36S25LR0JMQ\Tips24Service] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [EF3R36S25LR0JMQ\Tips24Service]
GO
ALTER ROLE [db_datareader] ADD MEMBER [EF3R36S25LR0JMQ\Tips24Service]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [EF3R36S25LR0JMQ\Tips24Service]
GO
/****** Object:  Schema [admin]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE SCHEMA [admin]
GO
/****** Object:  Schema [payment]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE SCHEMA [payment]
GO
/****** Object:  Schema [payout]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE SCHEMA [payout]
GO
/****** Object:  Schema [telegram]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE SCHEMA [telegram]
GO
/****** Object:  UserDefinedTableType [dbo].[FreeBigIntList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[FreeBigIntList] AS TABLE(
	[Id] [bigint] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[FreeGuidList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[FreeGuidList] AS TABLE(
	[Id] [uniqueidentifier] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[FreeIntList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[FreeIntList] AS TABLE(
	[Id] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[NonUniqueBigIntList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[NonUniqueBigIntList] AS TABLE(
	[Id] [int] NOT NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[NonUniqueGuidList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[NonUniqueGuidList] AS TABLE(
	[Id] [uniqueidentifier] NOT NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[NonUniqueIntList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[NonUniqueIntList] AS TABLE(
	[Id] [int] NOT NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[OrderedGuidList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[OrderedGuidList] AS TABLE(
	[SeqNum] [int] NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [dbo].[UniqueBigIntList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[UniqueBigIntList] AS TABLE(
	[Id] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [dbo].[UniqueGuidList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[UniqueGuidList] AS TABLE(
	[Id] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [dbo].[UniqueIntList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [dbo].[UniqueIntList] AS TABLE(
	[Id] [int] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [payment].[EmployeeBalanceLogList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [payment].[EmployeeBalanceLogList] AS TABLE(
	[EmployeeId] [int] NOT NULL,
	[SeqNum] [int] IDENTITY(1,1) NOT NULL,
	[OperationType] [tinyint] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentId] [bigint] NULL,
	PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[SeqNum] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [payment].[ModApiPaymentList]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [payment].[ModApiPaymentList] AS TABLE(
	[SeqNum] [int] NOT NULL,
	[Id] [uniqueidentifier] NOT NULL,
	[ExecutedDateTime] [datetime2](7) NOT NULL,
	[RawData] [nvarchar](max) NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[SeqNum] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  UserDefinedTableType [payment].[PaymentShare]    Script Date: 12/17/2018 2:06:30 AM ******/
CREATE TYPE [payment].[PaymentShare] AS TABLE(
	[EmployeeId] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
/****** Object:  Table [dbo].[Employee]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[EmployeeId] [int] NOT NULL,
	[Phone] [char](10) NOT NULL,
	[RegisterDateTime] [datetime2](7) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[LastBalanceLogId] [bigint] NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeAuth]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeAuth](
	[EmployeeId] [int] NOT NULL,
	[PinCode] [char](4) NOT NULL,
	[PermanentKey] [binary](16) NULL,
	[SecuredKey] [binary](16) NULL,
	[SecuredKeyLastAccessDt] [datetime2](7) NULL,
	[FailedAuthCount] [tinyint] NOT NULL,
	[LastFailedAuthDt] [datetime2](7) NULL,
 CONSTRAINT [PK_EmployeeAuth] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeBalanceLog]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeBalanceLog](
	[EmployeeId] [int] NOT NULL,
	[EmployeeBalanceLogId] [bigint] NOT NULL,
	[LogDateTime] [datetime2](7) NOT NULL,
	[OperationType] [tinyint] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
	[PaymentId] [bigint] NULL,
 CONSTRAINT [PK_EmployeeBalanceLog] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[EmployeeBalanceLogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeMembershipHistory]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeMembershipHistory](
	[EmployeeId] [int] NOT NULL,
	[SeqNum] [int] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[BeginDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NOT NULL,
	[IsFired] [bit] NOT NULL,
	[IsManager] [bit] NOT NULL,
	[IsOwner] [bit] NOT NULL,
	[OpenedBy] [int] NULL,
 CONSTRAINT [PK_EmployeeMembershipHistory] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[SeqNum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeePersonalDataHistory]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeePersonalDataHistory](
	[EmployeeId] [int] NOT NULL,
	[SeqNum] [int] NOT NULL,
	[BeginDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NOT NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[Email] [varchar](50) NOT NULL,
	[PassportNum] [char](10) NULL,
	[PassportScanFile] [varchar](50) NULL,
	[CardNumber] [varchar](20) NULL,
 CONSTRAINT [PK_EmployeePersonalDataHistory] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[SeqNum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeRegistrationLink]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeRegistrationLink](
	[LinkParameter] [uniqueidentifier] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[CreateDateTime] [datetime2](7) NOT NULL,
	[ExpireDateTime] [datetime2](7) NULL,
	[IsManager] [bit] NOT NULL,
	[IsOwner] [bit] NOT NULL,
	[Phone] [char](10) NULL,
 CONSTRAINT [PK_EmployeeRegistrationLink] PRIMARY KEY CLUSTERED 
(
	[LinkParameter] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payout]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payout](
	[PayoutId] [bigint] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[EmployeeBalanceLogId] [bigint] NOT NULL,
	[PaidAmount] [decimal](18, 2) NOT NULL,
	[CommissionAmount] [decimal](18, 2) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[PaidDateTime] [datetime2](7) NOT NULL,
	[ArrivedDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Payout] PRIMARY KEY CLUSTERED 
(
	[PayoutId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Place]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Place](
	[PlaceID] [int] NOT NULL,
	[RegisterDateTime] [datetime2](7) NOT NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](100) NOT NULL,
	[City] [nvarchar](40) NOT NULL,
	[Phone] [char](10) NOT NULL,
	[Inn] [varchar](20) NULL,
	[Email] [varchar](50) NULL,
	[TimeZoneId] [varchar](32) NOT NULL,
	[Info] [nvarchar](max) NULL,
 CONSTRAINT [PK_Place] PRIMARY KEY CLUSTERED 
(
	[PlaceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlaceGroup]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlaceGroup](
	[GroupId] [int] NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_PlaceGroup_1] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PpAcceptance]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PpAcceptance](
	[EmployeeId] [int] NOT NULL,
	[PrivacyPolicyId] [int] NOT NULL,
	[AcceptDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_PpAcceptance] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[PrivacyPolicyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PrivacyPolicy]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrivacyPolicy](
	[PrivacyPolicyId] [int] NOT NULL,
	[Version] [nvarchar](10) NOT NULL,
	[Url] [varchar](200) NOT NULL,
	[ExposureDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_PrivacyPolicy] PRIMARY KEY CLUSTERED 
(
	[PrivacyPolicyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShareSchemeHistory]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShareSchemeHistory](
	[ShareSchemeHistoryId] [int] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[PersonalShare] [tinyint] NOT NULL,
	[BeginDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ShareSchemeHistory_1] PRIMARY KEY CLUSTERED 
(
	[ShareSchemeHistoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SmsMessage]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SmsMessage](
	[MessageId] [bigint] NOT NULL,
	[MessageType] [tinyint] NOT NULL,
	[CreateDateTime] [datetime2](7) NOT NULL,
	[SendDateTime] [datetime2](7) NULL,
	[MessageParams] [nvarchar](max) NULL,
 CONSTRAINT [PK_SmsMessage] PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SsGroupShareHistory]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SsGroupShareHistory](
	[ShareSchemeHistoryId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[GroupWeight] [tinyint] NOT NULL,
 CONSTRAINT [PK_SsGroupShareHistory_1] PRIMARY KEY CLUSTERED 
(
	[ShareSchemeHistoryId] ASC,
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UaAcceptance]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UaAcceptance](
	[EmployeeId] [int] NOT NULL,
	[UserAgreementId] [int] NOT NULL,
	[AcceptDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_UaAcceptance] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[UserAgreementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAgreement]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAgreement](
	[UserAgreementId] [int] NOT NULL,
	[Version] [nvarchar](10) NOT NULL,
	[Url] [varchar](200) NOT NULL,
	[ExposureDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_UserAgreement] PRIMARY KEY CLUSTERED 
(
	[UserAgreementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VerificationCode]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VerificationCode](
	[VerificationId] [uniqueidentifier] NOT NULL,
	[Code] [char](4) NOT NULL,
	[GenerationDateTime] [datetime2](7) NOT NULL,
	[ExpireDateTime] [datetime2](7) NULL,
 CONSTRAINT [PK_VerificationCode] PRIMARY KEY CLUSTERED 
(
	[VerificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[Converter]    Script Date: 12/17/2018 2:06:30 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[Converter](
	[Id] [tinyint] NOT NULL,
	[Code] [varchar](30) NOT NULL,
 CONSTRAINT [PK_payment_Converter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_payment_Converter] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[DataSource]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[DataSource](
	[Id] [tinyint] NOT NULL,
	[Code] [char](6) NOT NULL,
	[DisplayName] [nvarchar](100) NULL,
 CONSTRAINT [PK_payment_DataSource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_payment_DataSource_Code] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[Document]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[Document](
	[DocumentId] [int] NOT NULL,
	[DataSourceId] [tinyint] NOT NULL,
	[ProviderId] [tinyint] NOT NULL,
	[DocumentName] [varchar](100) NOT NULL,
	[DocumentNumber] [varchar](40) NULL,
	[DocumentDate] [date] NULL,
 CONSTRAINT [PK_payment_Document] PRIMARY KEY CLUSTERED 
(
	[DocumentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[ModApiInvalidPayment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[ModApiInvalidPayment](
	[Id] [uniqueidentifier] NOT NULL,
	[ProcessedDateTime] [datetime2](7) NOT NULL,
	[ExecutedDateTime] [datetime2](7) NOT NULL,
	[RawData] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ModApiInvalidPayment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [payment].[ModApiPayment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[ModApiPayment](
	[Id] [uniqueidentifier] NOT NULL,
	[CreateDateTime] [datetime2](7) NOT NULL,
	[ExecutedDateTime] [datetime2](7) NOT NULL,
	[RawData] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_ModApiPayment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [payment].[ModApiProcessedPayment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[ModApiProcessedPayment](
	[Id] [uniqueidentifier] NOT NULL,
	[ProcessedDateTime] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_payment_ModApiProcessedPayment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[Payment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[Payment](
	[PaymentId] [bigint] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[EmployeeId] [int] NULL,
	[ShareSchemeHistoryId] [int] NOT NULL,
	[DataSourceId] [tinyint] NOT NULL,
	[ProviderId] [tinyint] NOT NULL,
	[Status] [tinyint] NOT NULL,
	[OriginalAmount] [decimal](18, 2) NOT NULL,
	[ReceivedAmount] [decimal](18, 2) NOT NULL,
	[BankCommissionAmount] [decimal](18, 2) NOT NULL,
	[AgentCommissionAmount] [decimal](18, 2) NOT NULL,
	[IncomeAmount] [decimal](18, 2) NOT NULL,
	[PayoutAmount] [decimal](18, 2) NOT NULL,
	[PaymentDateTime] [datetime2](7) NOT NULL,
	[IsPaymentTimeSpecified] [bit] NOT NULL,
	[ArrivalDateTime] [datetime2](7) NOT NULL,
	[ReturnDateTime] [datetime2](7) NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[PaymentExternalData]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[PaymentExternalData](
	[PaymentId] [bigint] NOT NULL,
	[DataSourceId] [tinyint] NOT NULL,
	[ExternalId] [varchar](50) NOT NULL,
	[DocumentId] [int] NULL,
	[ProviderId] [tinyint] NOT NULL,
	[Fio] [nvarchar](100) NULL,
	[Address] [nvarchar](150) NULL,
	[Purpose] [nvarchar](150) NULL,
 CONSTRAINT [PK_PaymentExternalData] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[PaymentShare]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[PaymentShare](
	[PaymentId] [bigint] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_PaymentShare] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC,
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[Provider]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[Provider](
	[Id] [tinyint] NOT NULL,
	[Code] [char](6) NOT NULL,
	[DisplayName] [nvarchar](100) NULL,
 CONSTRAINT [PK_payment_Provider] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_payment_Provider_Code] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[RawData]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[RawData](
	[PaymentId] [bigint] NOT NULL,
	[RawData] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_payment_RawData] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [payment].[YandexKassaRequest]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[YandexKassaRequest](
	[RequestId] [int] NOT NULL,
	[Status] [tinyint] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[EmployeeId] [int] NULL,
	[CreateDateTime] [datetime2](7) NOT NULL,
	[UserLogin] [varchar](50) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[ProviderId] [tinyint] NOT NULL,
	[PaymentId] [bigint] NULL,
	[KassaPaymentId] [varchar](50) NULL,
	[ParentId] [int] NULL,
 CONSTRAINT [PK_payment_YandexKassaRequest] PRIMARY KEY CLUSTERED 
(
	[RequestId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payment].[YandexKassaRequestLog]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payment].[YandexKassaRequestLog](
	[RequestId] [int] NOT NULL,
	[LogDateTime] [datetime2](7) NOT NULL,
	[Operation] [tinyint] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [payout].[Provider]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payout].[Provider](
	[Id] [tinyint] NOT NULL,
	[Code] [char](6) NOT NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_payout_Provider] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UK_payuout_Provider_Code] UNIQUE NONCLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [payout].[Request]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [payout].[Request](
	[PayoutId] [bigint] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[EmployeeBalanceLogId] [bigint] NOT NULL,
	[ProviderId] [tinyint] NOT NULL,
	[PaidAmount] [decimal](18, 2) NOT NULL,
	[CommissionAmount] [decimal](18, 2) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateDateTime] [datetime2](7) NOT NULL,
	[PaidDateTime] [datetime2](7) NULL,
 CONSTRAINT [PK_Payout] PRIMARY KEY CLUSTERED 
(
	[PayoutId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [telegram].[BotSettings]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [telegram].[BotSettings](
	[BotOffset] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [telegram].[BotUser]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [telegram].[BotUser](
	[TelegramId] [bigint] NOT NULL,
	[Phone] [char](10) NOT NULL,
	[QrCodeFileId] [varchar](64) NULL,
	[LastBalanceLogId] [bigint] NULL,
 CONSTRAINT [PK_TelegramUser] PRIMARY KEY CLUSTERED 
(
	[TelegramId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [telegram].[YandexKassaSession]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [telegram].[YandexKassaSession](
	[TelegramId] [bigint] NOT NULL,
	[UserLogin] [varchar](50) NULL,
	[Amount] [decimal](18, 2) NULL,
	[ProviderId] [tinyint] NOT NULL,
	[Step] [tinyint] NOT NULL,
 CONSTRAINT [PK_telegram_YandexKassaSession] PRIMARY KEY CLUSTERED 
(
	[TelegramId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Employee_Phone]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Employee_Phone] ON [dbo].[Employee]
(
	[Phone] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_EmployeeAuth_EmployeeId]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_EmployeeAuth_EmployeeId] ON [dbo].[EmployeeAuth]
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_EmployeeAuth_PermanentKey]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_EmployeeAuth_PermanentKey] ON [dbo].[EmployeeAuth]
(
	[PermanentKey] ASC
)
WHERE ([PermanentKey] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_EmployeeBalanceLog_PaymentLog]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_EmployeeBalanceLog_PaymentLog] ON [dbo].[EmployeeBalanceLog]
(
	[PaymentId] ASC,
	[EmployeeId] ASC,
	[OperationType] ASC
)
INCLUDE ( 	[LogDateTime],
	[Amount]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_EmployeeMembershipHistory_LoadShare]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_EmployeeMembershipHistory_LoadShare] ON [dbo].[EmployeeMembershipHistory]
(
	[PlaceId] ASC,
	[BeginDateTime] ASC
)
INCLUDE ( 	[GroupId],
	[EndDateTime],
	[IsFired],
	[IsManager],
	[IsOwner]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ShareSchemeHistory_LoadShare]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_ShareSchemeHistory_LoadShare] ON [dbo].[ShareSchemeHistory]
(
	[PlaceId] ASC,
	[BeginDateTime] ASC
)
INCLUDE ( 	[PersonalShare],
	[EndDateTime]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_payment_Document_Date_Number]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_payment_Document_Date_Number] ON [payment].[Document]
(
	[DataSourceId] ASC,
	[DocumentDate] ASC,
	[DocumentNumber] ASC
)
WHERE ([DocumentDate] IS NOT NULL AND [DocumentNumber] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_payment_Document_Name]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_payment_Document_Name] ON [payment].[Document]
(
	[DataSourceId] ASC,
	[DocumentName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_payment_ModApiProcessedPayment]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_payment_ModApiProcessedPayment] ON [payment].[ModApiProcessedPayment]
(
	[ProcessedDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_EmployeeId]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_EmployeeId] ON [payment].[Payment]
(
	[EmployeeId] ASC,
	[PaymentDateTime] ASC
)
WHERE ([EmployeeId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_PlaceId]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_PlaceId] ON [payment].[Payment]
(
	[PlaceId] ASC,
	[PaymentDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_SearchForPayment]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_SearchForPayment] ON [payment].[PaymentExternalData]
(
	[DataSourceId] ASC,
	[ExternalId] ASC,
	[DocumentId] ASC
)
INCLUDE ( 	[ProviderId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_PaymentShare_EmployeeId_PaymentId]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_PaymentShare_EmployeeId_PaymentId] ON [payment].[PaymentShare]
(
	[EmployeeId] ASC,
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_ChildRequests]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_ChildRequests] ON [payment].[YandexKassaRequest]
(
	[ParentId] ASC
)
WHERE ([ParentId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_FailedRequests]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_FailedRequests] ON [payment].[YandexKassaRequest]
(
	[Status] ASC,
	[RequestId] ASC
)
WHERE ([Status] IN ((5), (6)))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_FailedToCreateRequests]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_FailedToCreateRequests] ON [payment].[YandexKassaRequest]
(
	[Status] ASC,
	[RequestId] ASC
)
WHERE ([Status]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_PendingRequestsByStatus]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_PendingRequestsByStatus] ON [payment].[YandexKassaRequest]
(
	[Status] ASC,
	[RequestId] ASC
)
WHERE ([Status] IN ((0), (2), (3), (4)))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_SuccessfulRequests]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_SuccessfulRequests] ON [payment].[YandexKassaRequest]
(
	[Status] ASC,
	[RequestId] ASC
)
WHERE ([Status]=(7))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_payment_YandexKassaRequestLog]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_payment_YandexKassaRequestLog] ON [payment].[YandexKassaRequestLog]
(
	[RequestId] ASC,
	[LogDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_payout_Request_ProviderId_Status]    Script Date: 12/17/2018 2:06:31 AM ******/
CREATE NONCLUSTERED INDEX [IX_payout_Request_ProviderId_Status] ON [payout].[Request]
(
	[ProviderId] ASC,
	[Status] ASC,
	[CreateDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF__Employee__EmployeeId]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_Employee]) FOR [EmployeeId]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF_Employee_Balance]  DEFAULT ((0)) FOR [Balance]
GO
ALTER TABLE [dbo].[EmployeeAuth] ADD  CONSTRAINT [DF__EmployeeA__FailedAuthCount]  DEFAULT ((0)) FOR [FailedAuthCount]
GO
ALTER TABLE [dbo].[EmployeeBalanceLog] ADD  CONSTRAINT [DF_EmployeeBalanceLog_SeqNum]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_EmployeeBalanceLog_SeqNum]) FOR [EmployeeBalanceLogId]
GO
ALTER TABLE [dbo].[EmployeeBalanceLog] ADD  CONSTRAINT [DF_EmployeeBalanceLog_LogDateTime]  DEFAULT (sysdatetime()) FOR [LogDateTime]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_SeqNum]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_EmployeeMembershipHistory_SeqNum]) FOR [SeqNum]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_EndDateTime]  DEFAULT (CONVERT([datetime2](7),'9999-12-31 23:59:59.9999999')) FOR [EndDateTime]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_IsFired]  DEFAULT ((0)) FOR [IsFired]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_IsManager]  DEFAULT ((0)) FOR [IsManager]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_IsOwner]  DEFAULT ((0)) FOR [IsOwner]
GO
ALTER TABLE [dbo].[EmployeePersonalDataHistory] ADD  CONSTRAINT [DF_EmployeePersonalDataHistory_SeqNum]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_EmployeePersonalDataHistory_SeqNum]) FOR [SeqNum]
GO
ALTER TABLE [dbo].[EmployeePersonalDataHistory] ADD  CONSTRAINT [DF_EmployeePersonalDataHistory_EndDateTime]  DEFAULT (CONVERT([datetime2](7),'9999-12-31 23:59:59.9999999')) FOR [EndDateTime]
GO
ALTER TABLE [dbo].[EmployeeRegistrationLink] ADD  CONSTRAINT [DF_EmployeeRegistrationLink_LinkParameter]  DEFAULT (newid()) FOR [LinkParameter]
GO
ALTER TABLE [dbo].[EmployeeRegistrationLink] ADD  CONSTRAINT [DF_EmployeeRegistrationLink_IsManager]  DEFAULT ((0)) FOR [IsManager]
GO
ALTER TABLE [dbo].[EmployeeRegistrationLink] ADD  CONSTRAINT [DF_EmployeeRegistrationLink_IsOwner]  DEFAULT ((0)) FOR [IsOwner]
GO
ALTER TABLE [dbo].[Place] ADD  CONSTRAINT [DF__Place__PlaceID]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_Place]) FOR [PlaceID]
GO
ALTER TABLE [dbo].[Place] ADD  CONSTRAINT [DF_Place_RegisterDateTime]  DEFAULT (sysdatetimeoffset()) FOR [RegisterDateTime]
GO
ALTER TABLE [dbo].[Place] ADD  CONSTRAINT [DF_Place_TimeZoneId]  DEFAULT ('RTZ2') FOR [TimeZoneId]
GO
ALTER TABLE [dbo].[PlaceGroup] ADD  CONSTRAINT [DF__PlaceGrou__GroupId]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_PlaceGroup]) FOR [GroupId]
GO
ALTER TABLE [dbo].[ShareSchemeHistory] ADD  CONSTRAINT [DF_ShareSchemeHistory_SeqNum]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_ShareSchemeHistory_SeqNum]) FOR [ShareSchemeHistoryId]
GO
ALTER TABLE [dbo].[ShareSchemeHistory] ADD  CONSTRAINT [DF_ShareSchemeHistory_EndDateTime]  DEFAULT (CONVERT([datetime2](7),'9999-12-31 23:59:59.9999999')) FOR [EndDateTime]
GO
ALTER TABLE [dbo].[SmsMessage] ADD  CONSTRAINT [DF__SmsMessag__MessageId]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_SmsMessage]) FOR [MessageId]
GO
ALTER TABLE [dbo].[UserAgreement] ADD  CONSTRAINT [DF_UserAgreement_ExposureDateTime]  DEFAULT (getdate()) FOR [ExposureDateTime]
GO
ALTER TABLE [dbo].[VerificationCode] ADD  CONSTRAINT [DF_VerificationCode_VerificationId]  DEFAULT (newid()) FOR [VerificationId]
GO
ALTER TABLE [payment].[Document] ADD  CONSTRAINT [DF_Document_DocumentId]  DEFAULT (NEXT VALUE FOR [payment].[Seq_Document]) FOR [DocumentId]
GO
ALTER TABLE [payment].[ModApiInvalidPayment] ADD  CONSTRAINT [DF_ModApiInvalidPayment_ProcessedDateTime]  DEFAULT (sysdatetime()) FOR [ProcessedDateTime]
GO
ALTER TABLE [payment].[ModApiPayment] ADD  CONSTRAINT [DF_ModApiPayment_CreateDateTime]  DEFAULT (sysdatetime()) FOR [CreateDateTime]
GO
ALTER TABLE [payment].[ModApiProcessedPayment] ADD  CONSTRAINT [DF__payment__ModApiProcessedPayment__ProcessedDateTime]  DEFAULT (sysdatetime()) FOR [ProcessedDateTime]
GO
ALTER TABLE [payment].[Payment] ADD  CONSTRAINT [DF_Payment_PaymentId]  DEFAULT (NEXT VALUE FOR [payment].[Seq_Payment]) FOR [PaymentId]
GO
ALTER TABLE [payment].[Payment] ADD  CONSTRAINT [DF_Payment_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [payment].[Payment] ADD  CONSTRAINT [DF_Payment_IsPaymentTimeSpecified]  DEFAULT ((0)) FOR [IsPaymentTimeSpecified]
GO
ALTER TABLE [payment].[YandexKassaRequest] ADD  CONSTRAINT [DF__YandexKas__Creat__002AF460]  DEFAULT (sysdatetime()) FOR [CreateDateTime]
GO
ALTER TABLE [telegram].[BotSettings] ADD  CONSTRAINT [DF_TelegramBotSettings_BotOffset]  DEFAULT ((0)) FOR [BotOffset]
GO
ALTER TABLE [telegram].[YandexKassaSession] ADD  CONSTRAINT [DF_telegram_YandexKassaSession_ProviderId]  DEFAULT ((0)) FOR [ProviderId]
GO
ALTER TABLE [telegram].[YandexKassaSession] ADD  CONSTRAINT [DF_telegram_YandexKassaSession_Step]  DEFAULT ((0)) FOR [Step]
GO
/****** Object:  StoredProcedure [admin].[DeletePayments]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE    PROCEDURE [admin].[DeletePayments]
	@PaymentIdList dbo.UniqueIntList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @FakeCount tinyint;

		SELECT @FakeCount = COUNT(*)
			FROM payment.Payment p with(xlock, rowlock, serializable)
			INNER JOIN @PaymentIdList l ON p.PaymentId = l.Id;

		WITH s(EmployeeId, AmountSum) AS
		(
			SELECT ps.EmployeeId, SUM(ps.Amount)
				FROM payment.Payment p
				INNER JOIN payment.PaymentShare ps ON p.PaymentId = ps.PaymentId
				INNER JOIN @PaymentIdList l ON p.PaymentId = l.Id
				WHERE p.Status = 1
				GROUP BY ps.EmployeeId
		)
		UPDATE e SET Balance = e.Balance - s.AmountSum
			FROM dbo.Employee e
			INNER JOIN s ON e.EmployeeId = s.EmployeeId;

		DELETE FROM payment.RawData WHERE PaymentId IN (SELECT Id FROM @PaymentIdList);
		DELETE FROM payment.PaymentExternalData WHERE PaymentId IN (SELECT Id FROM @PaymentIdList);
		DELETE FROM payment.PaymentShare WHERE PaymentId IN (SELECT Id FROM @PaymentIdList);
		DELETE FROM dbo.EmployeeBalanceLog WHERE PaymentId IN (SELECT Id FROM @PaymentIdList);
		DELETE FROM payment.Payment WHERE PaymentId IN (SELECT Id FROM @PaymentIdList);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [admin].[YksSms_DeleteRequest]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [admin].[YksSms_DeleteRequest]
	@RequestId int
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRANSACTION;
	BEGIN TRY
		DELETE FROM payment.YandexKassaRequest WHERE RequestId = @RequestId;
		DELETE FROM payment.YandexKassaRequestLog WHERE RequestId = @RequestId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [admin].[YksSms_RecreateCanceledRequest]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE    PROCEDURE [admin].[YksSms_RecreateCanceledRequest]
	@RequestId int
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @Status tinyint;
		SELECT @Status = Status FROM payment.YandexKassaRequest with (xlock) WHERE RequestId = @RequestId;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION
			RETURN -1;
		END;

		IF (@Status != 1)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		DECLARE @RootRequestId int;

		WITH r AS
		(
			SELECT RequestId, ParentId, 1 lvl FROM payment.YandexKassaRequest WHERE RequestId = @RequestId
			UNION ALL
			SELECT p.RequestId, p.ParentId, r.lvl + 1
				FROM payment.YandexKassaRequest p with(xlock, rowlock)
				INNER JOIN r ON p.RequestId = r.ParentId AND p.Status = 1
		)
		SELECT TOP(1) @RootRequestId = r.RequestId
			FROM r
			ORDER BY lvl DESC;
		
		SELECT @RootRequestId;

		DECLARE @curDt datetime2(7) = sysdatetime();
		DECLARE @NewRequestId int = NEXT VALUE FOR payment.Seq_YandexKassaRequest;
		INSERT INTO payment.YandexKassaRequest (RequestId, Status, PlaceId, EmployeeId, CreateDateTime, UserLogin, Amount, ProviderId, ParentId)
		SELECT @NewRequestId, 0, PlaceId, EmployeeId, @curDt, UserLogin, Amount, ProviderId, RequestId
			FROM payment.YandexKassaRequest
			WHERE RequestId = @RootRequestId;
		
		INSERT INTO payment.YandexKassaRequestLog (RequestId, LogDateTime, Operation) VALUES (@NewRequestId, @curDt, 0);

		SELECT * FROM payment.YandexKassaRequest WHERE RequestId = @NewRequestId;
		SELECT * FROM payment.YandexKassaRequestLog WHERE RequestId = @NewRequestId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[Employee_CheckEmployeeStatus]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [dbo].[Employee_CheckEmployeeStatus]
	@PermanentKey binary(16),
	@PlaceId int,
	@EmployeeId int output,
	@EmployeeFirstName nvarchar(50) output,
	@EmployeeLastName nvarchar(50) output,
	@EmployeeIsDisabled bit output,
	@PlaceGroupId int output,
	@PlaceGroupName nvarchar(50) output
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL)
		RETURN -1;

	DECLARE @StoredPlaceId int;

	SELECT @EmployeeId = e.EmployeeId, @StoredPlaceId = e.PlaceId, @EmployeeIsDisabled = e.IsDisabled, @PlaceGroupId = e.GroupId, @PlaceGroupName = g.Name
		FROM dbo.EmployeeAuth a
		INNER JOIN dbo.Employee e ON e.EmployeeId = e.EmployeeId
		LEFT JOIN dbo.PlaceGroup g ON e.GroupId = g.GroupId
		WHERE a.PermanentKey = @PermanentKey;
		
	IF (@@ROWCOUNT = 0)
		RETURN -2;

	IF (@StoredPlaceId != @PlaceId)
		RETURN -3;

	SELECT @EmployeeFirstName = FirstName, @EmployeeLastName = LastName FROM tips_personal.dbo.EmployeePersonalData WHERE EmployeeId = @EmployeeId;
	IF (@@ROWCOUNT = 0)
		RETURN -4;
END;
GO
/****** Object:  StoredProcedure [dbo].[Employee_CheckVerificationCode]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [dbo].[Employee_CheckVerificationCode]
	@VerificationId uniqueidentifier,
	@Code char(6)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@VerificationId IS NULL OR @Code IS NULL)
		RETURN -1;

	DECLARE @GenerationDateTime datetime;
	DECLARE @StoredCode char(6);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @GenerationDateTime = GenerationDateTime, @StoredCode = Code FROM dbo.VerificationCode with(xlock) WHERE VerificationId = @VerificationId;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (DATEDIFF(minute, @GenerationDateTime, GETDATE()) > 60)
		BEGIN
			DELETE FROM dbo.VerificationCode WHERE VerificationId = @VerificationId;
			COMMIT TRANSACTION;
			RETURN -3;
		END;

		IF (@StoredCode != @Code)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END;

		DELETE FROM dbo.VerificationCode WHERE VerificationId = @VerificationId;
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
/****** Object:  StoredProcedure [dbo].[Employee_EnterSecuredSession]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_EnterSecuredSession]
	@PermanentKey binary(16),
	@SecuredKey binary(16),
	@Phone char(10),
	@PinCode char(4)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@Phone IS NULL OR @PinCode IS NULL OR @PermanentKey IS NULL OR @SecuredKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeIdByPermanentKey int;
	DECLARE @EmployeeIdByPersonalData int;
	DECLARE @StoredPinCode char(4);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeIdByPermanentKey = EmployeeId FROM dbo.EmployeeAuth with(updlock) WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		SELECT @EmployeeIdByPersonalData = EmployeeId, @StoredPinCode = PinCode FROM tips_personal.dbo.EmployeePersonalData WHERE Phone = @Phone;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (@PinCode != @StoredPinCode)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END;

		IF (@EmployeeIdByPermanentKey != @EmployeeIdByPersonalData)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -5;
		END;

		UPDATE dbo.EmployeeAuth SET
				SpecialKey = @SecuredKey,
				SpecialKeyLastAccessDt = GETDATE(),
				FailedAuthCount = 0,
				LastFailedAuthDt = NULL
			WHERE EmployeeId = @EmployeeIdByPermanentKey;

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
/****** Object:  StoredProcedure [dbo].[Employee_ExitSecuredSession]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_ExitSecuredSession]
	@PermanentKey binary(16),
	@SecuredKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL OR @SecuredKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredSecuredKey binary(16);
	DECLARE @SecuredKeyLastAccessDt datetime;
	DECLARE @StoredPinCode char(4);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId, @StoredSecuredKey = SpecialKey
			FROM dbo.EmployeeAuth with(updlock)
			WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredSecuredKey IS NULL OR @StoredSecuredKey != @SecuredKey)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		UPDATE dbo.EmployeeAuth SET
				SpecialKey = NULL,
				SpecialKeyLastAccessDt = NULL,
				FailedAuthCount = 0,
				LastFailedAuthDt = NULL
			WHERE EmployeeId = @EmployeeId;

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
/****** Object:  StoredProcedure [dbo].[Employee_FollowRegistrationLink]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_FollowRegistrationLink]
	@LinkParameter uniqueidentifier,
	@PermanentKey binary(16),
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
	
	IF (@LinkParameter IS NULL OR @PermanentKey IS NULL)
		RETURN -1;

	DECLARE @LinkCreateDateTime datetime;

	SELECT @LinkPlaceId = PlaceId, @LinkCreateDateTime = CreateDateTime FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
	IF (@@ROWCOUNT = 0)
		RETURN -2;

	IF (DATEDIFF(minute, @LinkCreateDateTime, GETDATE()) > 60 * 24)
		RETURN -3;

	IF (@PermanentKey IS NOT NULL)
	BEGIN
		SELECT @EmployeeId = EmployeeId FROM dbo.EmployeeAuth WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
			RETURN -4;
	END;

	SELECT @LinkPlaceName = DisplayName, @LinkPlaceAddress = Address, @LinkPlaceCity = City FROM dbo.Place WHERE PlaceId = @LinkPlaceId;

	IF (@EmployeeId IS NOT NULL)
		SELECT @EmployeePlaceId = PlaceId, @EmployeeIsDisabled = IsDisabled FROM dbo.Employee WHERE EmployeeId = @EmployeeId;

	RETURN 0;
END;
GO
/****** Object:  StoredProcedure [dbo].[Employee_JoinPlace]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_JoinPlace]
	@PermanentKey binary(16),
	@LinkParameter uniqueidentifier,
	@PlaceId int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF (@LinkParameter IS NULL OR @PermanentKey IS NULL)
		RETURN -1;

	DECLARE @StoredPlaceId int;
	DECLARE @EmployeeId int;
	DECLARE @OldPlaceId int;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @StoredPlaceId = PlaceId FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredPlaceId != @PlaceId)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (NOT EXISTS(SELECT * FROM dbo.Place WHERE PlaceId = @PlaceId))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END

		SELECT @EmployeeId = e.EmployeeId, @OldPlaceId = e.PlaceId
			FROM dbo.EmployeeAuth a
			INNER JOIN dbo.Employee e with(updlock) ON e.EmployeeId = e.EmployeeId
			WHERE a.PermanentKey = @PermanentKey;
		
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -5;
		END;

		UPDATE dbo.Employee SET PlaceId = @PlaceId, GroupId = NULL, IsDisabled = 0 WHERE EmployeeId = @EmployeeId;
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
/****** Object:  StoredProcedure [dbo].[Employee_KeepSecuredSessionAlive]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_KeepSecuredSessionAlive]
	@PermanentKey binary(16),
	@SecuredKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL OR @SecuredKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredSecuredKey binary(16);
	DECLARE @SecuredKeyLastAccessDt datetime;
	DECLARE @StoredPinCode char(4);

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId, @StoredSecuredKey = SpecialKey, @SecuredKeyLastAccessDt = SpecialKeyLastAccessDt
			FROM dbo.EmployeeAuth with(updlock)
			WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredSecuredKey IS NULL OR @StoredSecuredKey != @SecuredKey)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (DATEDIFF(minute, @SecuredKeyLastAccessDt, GETDATE()) > 10)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END;

		UPDATE dbo.EmployeeAuth SET SpecialKeyLastAccessDt = GETDATE() WHERE EmployeeId = @EmployeeId;

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
/****** Object:  StoredProcedure [dbo].[Employee_Login]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_Login]
	@Phone char(10),
	@PinCode char(4),
	@PermanentKey binary(16) output
AS
BEGIN
	SET NOCOUNT ON;

	IF (@Phone IS NULL OR @PinCode IS NULL)
		RETURN -1;

	DECLARE @StoredPinCode char(4);
	DECLARE @EmployeeId int;

	SELECT @EmployeeId = EmployeeId, @StoredPinCode = PinCode FROM tips_personal.dbo.EmployeePersonalData WHERE Phone = @Phone;
	IF (@@ROWCOUNT = 0)
		RETURN -2;

	IF (@PinCode != @StoredPinCode)
		RETURN -3;

	SELECT @PermanentKey = PermanentKey FROM dbo.EmployeeAuth WHERE EmployeeId = @EmployeeId;
	IF (@@ROWCOUNT = 0)
		RETURN -4;

	RETURN 0;
END;
GO
/****** Object:  StoredProcedure [dbo].[Employee_Logout]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_Logout]
	@PermanentKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PermanentKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @EmployeeId = EmployeeId FROM dbo.EmployeeAuth with(updlock) WHERE PermanentKey = @PermanentKey;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		UPDATE dbo.EmployeeAuth SET
				PermanentKey = NULL,
				SpecialKey = NULL,
				SpecialKeyLastAccessDt = NULL,
				FailedAuthCount = 0,
				LastFailedAuthDt = NULL
			WHERE EmployeeId = @EmployeeId;

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
/****** Object:  StoredProcedure [dbo].[Employee_Register]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_Register]
	@FirstName nvarchar(50),
	@LastName nvarchar(50),
	@Phone char(10),
	@PinCode char(4),
	@LinkParameter uniqueidentifier,
	@PlaceId int,
	@PermanentKey binary(16)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@PlaceId IS NULL OR @FirstName IS NULL OR @LastName IS NULL OR @Phone IS NULL OR @PinCode IS NULL OR @LinkParameter IS NULL OR @PermanentKey IS NULL)
		RETURN -1;

	DECLARE @EmployeeId int;
	DECLARE @StoredPlaceId int;

	BEGIN TRY
		BEGIN TRANSACTION;

		SELECT @StoredPlaceId = PlaceId FROM dbo.EmployeeRegistrationLink WHERE LinkParameter = @LinkParameter;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -2;
		END;

		IF (@StoredPlaceId != @PlaceId)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -3;
		END;

		IF (NOT EXISTS(SELECT * FROM dbo.Place WHERE PlaceId = @PlaceId))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -4;
		END

		IF (EXISTS(SELECT * FROM tips_personal.dbo.EmployeePersonalData with(xlock, serializable) WHERE Phone = @Phone))
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -5;
		END;

		SET @EmployeeId = NEXT VALUE FOR dbo.Seq_Employee;
		INSERT INTO dbo.Employee (EmployeeId, PlaceId, RegisterDateTime, IsDisabled) VALUES (@EmployeeId, @PlaceId, GETDATE(), 0);
		INSERT INTO dbo.EmployeeAuth (EmployeeId, PermanentKey) VALUES (@EmployeeId, @PermanentKey);
		INSERT INTO tips_personal.dbo.EmployeePersonalData (EmployeeId, Phone, PinCode, FirstName, LastName) VALUES (@EmployeeId, @Phone, @PinCode, @FirstName, @LastName);

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
/****** Object:  StoredProcedure [dbo].[Employee_RegisterVerificationCode]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[Employee_RegisterVerificationCode]
	@VerificationId uniqueidentifier,
	@Code char(6)
AS
BEGIN
	SET NOCOUNT ON;

	IF (@VerificationId IS NULL OR @Code IS NULL)
		RETURN -1;

	INSERT INTO dbo.VerificationCode (VerificationId, Code, GenerationDateTime) VALUES (@VerificationId, @Code, GETDATE());
	RETURN 0;
END;
GO
/****** Object:  StoredProcedure [payment].[ApprovePayment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [payment].[ApprovePayment]
	@PaymentId int
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE payment.Payment SET Status = 1 WHERE PaymentId = @PaymentId AND Status = 0;

		IF (@@ROWCOUNT = 1)
		BEGIN
			DECLARE @list payment.EmployeeBalanceLogList;

			INSERT INTO @list (EmployeeId, OperationType, Amount, PaymentId)
			SELECT EmployeeId, 1, Amount, @PaymentId
				FROM payment.PaymentShare with (updlock, rowlock)
				WHERE PaymentId = @PaymentId;

			EXECUTE payment.InsertEmployeeBalanceLog @list = @list;
		END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [payment].[CheckForExistingPayment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE     PROCEDURE [payment].[CheckForExistingPayment]
	@DataSource char(6),
	@DocumentName varchar(100),
	@ExternalId varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF (
		EXISTS (
			SELECT *
				FROM payment.PaymentExternalData ped
				WHERE ped.ExternalId = @ExternalId
					AND EXISTS(SELECT * FROM payment.DataSource ds WHERE ped.DataSourceId = ds.Id AND ds.Code = @DataSource)
					AND (@DocumentName IS NULL OR EXISTS(SELECT * FROM payment.Document d WHERE ped.DocumentId = d.DocumentId AND d.DocumentName = @DocumentName))
		)
	)
	BEGIN
		RETURN 1;
	END;

	RETURN 0;
END;
GO
/****** Object:  StoredProcedure [payment].[CheckModApiPaymentList]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE    PROCEDURE [payment].[CheckModApiPaymentList]
	@IdList dbo.OrderedGuidList READONLY
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
/****** Object:  StoredProcedure [payment].[CompleteYandexKassaRequest]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [payment].[CompleteYandexKassaRequest]
	@RequestId int,
	@PaymentId bigint
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE payment.YandexKassaRequest SET Status = 7, PaymentId = @PaymentId WHERE RequestId = @RequestId;
		INSERT INTO payment.YandexKassaRequestLog(RequestId, LogDateTime, Operation) VALUES (@RequestId, sysdatetime(), 7);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [payment].[GetModApiPaymentToProcess]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [payment].[GetModApiPaymentToProcess]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP(1) RawData FROM payment.ModApiPayment;
END;
GO
/****** Object:  StoredProcedure [payment].[GetYandexKassaRequest]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [payment].[GetYandexKassaRequest]
	@Status tinyint
AS
BEGIN
	SELECT TOP(1) r.RequestId, r.Status, r.PlaceId, r.EmployeeId, r.CreateDateTime, r.UserLogin, r.Amount, r.ProviderId, r.KassaPaymentId, p.DisplayName PlaceDisplayName
		FROM payment.YandexKassaRequest r
		INNER JOIN dbo.Place p ON p.PlaceID = r.PlaceId
		WHERE Status = @Status
		ORDER BY RequestId;
END;
GO
/****** Object:  StoredProcedure [payment].[GetYandexKassaRequestById]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [payment].[GetYandexKassaRequestById]
	@RequestId int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT r.RequestId, r.Status, r.PlaceId, r.EmployeeId, r.CreateDateTime, r.UserLogin, r.Amount, r.ProviderId, r.KassaPaymentId, p.DisplayName PlaceDisplayName
		FROM payment.YandexKassaRequest r
		INNER JOIN dbo.Place p ON p.PlaceID = r.PlaceId
		WHERE r.RequestId = @RequestId;
END;
GO
/****** Object:  StoredProcedure [payment].[InsertEmployeeBalanceLog]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [payment].[InsertEmployeeBalanceLog]
	@list payment.EmployeeBalanceLogList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY

		DECLARE @t1 TABLE
		(
			EmployeeId int NOT NULL,
			SeqNum int NOT NULL DEFAULT NEXT VALUE FOR dbo.Seq_EmployeeBalanceLog_SeqNum,
			OperationType tinyint NOT NULL,
			Amount decimal(18,2) NOT NULL,
			Balance decimal(18,2) NOT NULL,
			PaymentId bigint NULL,
			PRIMARY KEY CLUSTERED (EmployeeId, SeqNum)
		);

		INSERT INTO @t1 (EmployeeId, OperationType, PaymentId, Amount, Balance)
		SELECT l.EmployeeId, l.OperationType, l.PaymentId, l.Amount, SUM(l.Amount) OVER(PARTITION BY l.EmployeeId ORDER BY l.SeqNum ROWS UNBOUNDED PRECEDING) + e.Balance Balance
			FROM @list l
			INNER JOIN dbo.Employee e with(updlock, rowlock) ON e.EmployeeId = l.EmployeeId
			ORDER BY l.SeqNum;

		INSERT INTO dbo.EmployeeBalanceLog (EmployeeId, EmployeeBalanceLogId, OperationType, Amount, Balance, PaymentId)
		SELECT EmployeeId, SeqNum, OperationType, Amount, Balance, PaymentId
			FROM @t1
			ORDER BY SeqNum;

		WITH l AS
		(
			SELECT EmployeeId, Balance, SeqNum, ROW_NUMBER() OVER(PARTITION BY EmployeeId ORDER BY SeqNum DESC) rn FROM @t1
		)
		UPDATE e SET Balance = l.Balance, LastBalanceLogId = l.SeqNum
			FROM dbo.Employee e
			INNER JOIN l ON e.EmployeeId = l.EmployeeId AND l.rn = 1;

		--DROP TABLE #t1;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [payment].[LoadShareData]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [payment].[LoadShareData]
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

	SET @SystemCommission = 17;

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
			FROM dbo.EmployeeMembershipHistory
			WHERE PlaceId = @PlaceId AND @PaymentDateTime BETWEEN BeginDateTime AND EndDateTime AND IsFired = 0
			ORDER BY EmployeeId, BeginDateTime;
	END
	ELSE
	BEGIN
		SELECT EmployeeId, GroupId, IsManager, IsOwner,
				IIF(BeginDateTime < @BeginOfDay, @BeginOfDay, BeginDateTime) BeginDateTime,
				IIF(EndDateTime > @EndOfDay, @EndOfDay, EndDateTime) EndDateTime
			FROM dbo.EmployeeMembershipHistory
			WHERE PlaceId = @PlaceId AND BeginDateTime <= @EndOfDay AND EndDateTime >= @BeginOfDay AND IsFired = 0
			ORDER BY EmployeeId, BeginDateTime;
	END;
END;
GO
/****** Object:  StoredProcedure [payment].[PrepareYandexKassaRequestsForNewCheckIteration]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE     PROCEDURE [payment].[PrepareYandexKassaRequestsForNewCheckIteration]
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE payment.YandexKassaRequest SET Status = 2 WHERE Status IN (3, 4);
END;
GO
/****** Object:  StoredProcedure [payment].[ReturnPayment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE     PROCEDURE [payment].[ReturnPayment]
	@PaymentId int
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRANSACTION;
	BEGIN TRY
		DECLARE @OldStatus tinyint;

		SELECT @OldStatus = Status FROM payment.Payment with(updlock) WHERE PaymentId = @PaymentId AND Status IN (0, 1);
		IF (@@ROWCOUNT = 1)
		BEGIN
			UPDATE payment.Payment SET Status = 2, ReturnDateTime = sysdatetime() WHERE PaymentId = @PaymentId;

			IF (@OldStatus = 1)
			BEGIN
				DECLARE @list payment.EmployeeBalanceLogList;

				INSERT INTO @list (EmployeeId, OperationType, Amount, PaymentId)
				SELECT EmployeeId, 3, -Amount, @PaymentId
					FROM payment.PaymentShare with (updlock, rowlock)
					WHERE PaymentId = @PaymentId;

				EXECUTE payment.InsertEmployeeBalanceLog @list = @list;
			END;
		END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [payment].[SaveDocumentProperties]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE     PROCEDURE [payment].[SaveDocumentProperties]
	@DocumentId int,
	@DocumentNumber varchar(40),
	@DocumentDate date
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE payment.Document SET DocumentNumber = @DocumentNumber, DocumentDate = @DocumentDate WHERE DocumentId = @DocumentId;
END;
GO
/****** Object:  StoredProcedure [payment].[SaveModApiPaymentList]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [payment].[SaveModApiPaymentList]
	@List payment.ModApiPaymentList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	INSERT INTO payment.ModApiPayment(Id, ExecutedDateTime, RawData)
	SELECT Id, ExecutedDateTime, RawData
		FROM @List
		WHERE Id NOT IN (SELECT Id FROM payment.ModApiPayment)
			AND Id NOT IN (SELECT Id FROM payment.ModApiProcessedPayment)
			AND Id NOT IN (SELECT Id FROM payment.ModApiInvalidPayment)
		ORDER BY SeqNum;
END;
GO
/****** Object:  StoredProcedure [payment].[SavePayment]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE   PROCEDURE [payment].[SavePayment]
	@PlaceId int,
	@EmployeeId int,
	@DataSource char(6),
	@Provider char(6),
	@ShareSchemeHistoryId int,
	@OriginalAmount decimal(18,2),
	@ReceivedAmount decimal(18,2),
	@BankCommissionAmount decimal(18,2),
	@AgentCommissionAmount decimal(18,2),
	@IncomeAmount decimal(18,2),
	@PayoutAmount decimal(18,2),
	@PaymentDateTime datetime2(7),
	@IsTimeSpecified bit,
	@ArrivalDateTime datetime2(7),
	@DocumentName varchar(100),
	@DocumentNumber varchar(40),
	@DocumentDate date,
	@DocumentId int output,
	@ExternalId varchar(50),
	@Fio nvarchar(100),
	@Address nvarchar(150),
	@Purpose nvarchar(150),
	@ApprovePayment bit,
	@RawData nvarchar(max),
	@Amounts payment.PaymentShare READONLY,
	@PaymentId bigint output,
	@Status tinyint output
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @DataSourceId tinyint;
	DECLARE @ProviderId tinyint;

	BEGIN TRANSACTION;
	BEGIN TRY
		SELECT @DataSourceId = Id FROM payment.DataSource WHERE Code = @DataSource;
		SELECT @ProviderId = Id FROM payment.Provider WHERE Code = @Provider;

		IF (@DocumentName IS NOT NULL)
		BEGIN
			SELECT @DocumentId = DocumentId
				FROM payment.Document with (xlock, rowlock, serializable)
				WHERE DataSourceId = @DataSourceId AND DocumentName = @DocumentName;

			IF (@@ROWCOUNT = 0)
			BEGIN
				SET @DocumentId = NEXT VALUE FOR payment.Seq_Document;
				INSERT INTO payment.Document (DocumentId, DataSourceId, ProviderId, DocumentName, DocumentNumber, DocumentDate)
					VALUES (@DocumentId, @DataSourceId, @ProviderId, @DocumentName, @DocumentNumber, @DocumentDate);
			END;
		END;

		SELECT @PaymentId = NEXT VALUE FOR payment.Seq_Payment;
		INSERT INTO payment.Payment (PaymentId, PlaceId, EmployeeId, ShareSchemeHistoryId, DataSourceId, ProviderId, Status, OriginalAmount, ReceivedAmount,
				BankCommissionAmount, AgentCommissionAmount, IncomeAmount, PayoutAmount, PaymentDateTime, IsPaymentTimeSpecified, ArrivalDateTime)
			VALUES (@PaymentId, @PlaceId, @EmployeeId, @ShareSchemeHistoryId, @DataSourceId, @ProviderId, 0, @OriginalAmount, @ReceivedAmount,
				@BankCommissionAmount, @AgentCommissionAmount, @IncomeAmount, @PayoutAmount, @PaymentDateTime, @IsTimeSpecified, @ArrivalDateTime);

		INSERT INTO payment.PaymentExternalData (PaymentId, DocumentId, ExternalId, DataSourceId, ProviderId, Fio, Address, Purpose)
			VALUES (@PaymentId, @DocumentId, @ExternalId, @DataSourceId, @ProviderId, @Fio, @Address, @Purpose)

		INSERT INTO payment.PaymentShare (PaymentId, EmployeeId, Amount)
			SELECT @PaymentId, EmployeeId, Amount FROM @Amounts;

		IF (@RawData IS NOT NULL)
		BEGIN
			INSERT INTO payment.RawData (PaymentId, RawData) VALUES (@PaymentId, @RawData);
		END;

		IF (@ApprovePayment = 1)
		BEGIN
			EXECUTE payment.ApprovePayment @PaymentId = @PaymentId;
		END;

		SELECT @Status = Status FROM payment.Payment WHERE PaymentId = @PaymentId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO
/****** Object:  StoredProcedure [payment].[SetModApiPaymentAsInvalid]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [payment].[SetModApiPaymentAsInvalid]
	@PaymentId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		IF (NOT EXISTS(SELECT * FROM payment.ModApiInvalidPayment with (xlock, rowlock) WHERE Id = @PaymentId))
		BEGIN
			INSERT INTO payment.ModApiInvalidPayment (Id, ExecutedDateTime, RawData)
			SELECT Id, ExecutedDateTime, RawData
				FROM payment.ModApiPayment with(xlock, rowlock)
				WHERE Id = @PaymentId;
		END;

		DELETE FROM payment.ModApiPayment WHERE Id = @PaymentId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [payment].[SetModApiPaymentAsProcessed]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [payment].[SetModApiPaymentAsProcessed]
	@PaymentId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DELETE FROM payment.ModApiPayment WHERE Id = @PaymentId;
		IF (NOT EXISTS(SELECT * FROM payment.ModApiProcessedPayment with (xlock, rowlock) WHERE Id = @PaymentId))
		BEGIN
			INSERT INTO payment.ModApiProcessedPayment (Id) VALUES (@PaymentId);
		END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [payment].[UpdateYandexKassaRequestStatus]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [payment].[UpdateYandexKassaRequestStatus]
	@RequestId int,
	@Status tinyint,
	@KassaPaymentId varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE payment.YandexKassaRequest SET Status = @Status, KassaPaymentId = @KassaPaymentId WHERE RequestId = @RequestId;
		IF (@Status NOT IN (3, 4))
		BEGIN
			INSERT INTO payment.YandexKassaRequestLog(RequestId, LogDateTime, Operation) VALUES (@RequestId, sysdatetime(), @Status);
		END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO
/****** Object:  StoredProcedure [telegram].[CancelYandexKassaSession]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE     PROCEDURE [telegram].[CancelYandexKassaSession]
	@TelegramId bigint
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM telegram.YandexKassaSession WHERE TelegramId = @TelegramId;
END
GO
/****** Object:  StoredProcedure [telegram].[CompleteYandexKassaSession]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE     PROCEDURE [telegram].[CompleteYandexKassaSession]
	@TelegramId bigint,
	@PlaceId int,
	@EmployeeId int
AS
BEGIN
	SET NOCOUNT ON;
	
	BEGIN TRANSACTION;
	BEGIN TRY
		
		DECLARE @curDt datetime2(7) = sysdatetime();
		DECLARE @RequestId int = NEXT VALUE FOR payment.Seq_YandexKassaRequest;
		INSERT INTO payment.YandexKassaRequest (RequestId, Status, PlaceId, EmployeeId, CreateDateTime, UserLogin, Amount, ProviderId)
		SELECT @RequestId, 0, @PlaceId, @EmployeeId, @curDt, yks.UserLogin, yks.Amount, yks.ProviderId
			FROM telegram.YandexKassaSession yks
			WHERE yks.TelegramId = @TelegramId;
		IF (@@ROWCOUNT = 0)
		BEGIN
			ROLLBACK TRANSACTION;
			RETURN -1;
		END;
		
		INSERT INTO payment.YandexKassaRequestLog (RequestId, LogDateTime, Operation) VALUES (@RequestId, @curDt, 0);
		DELETE FROM telegram.YandexKassaSession WHERE TelegramId = @TelegramId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO
/****** Object:  StoredProcedure [telegram].[CreateAndGetUserRecord]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [telegram].[CreateAndGetUserRecord]
	@UserId bigint,
	@Phone char(10)
AS
BEGIN
	BEGIN TRANSACTION
	BEGIN TRY
		IF (NOT EXISTS(SELECT * FROM telegram.BotUser with (updlock, serializable) WHERE TelegramId = @UserId))
			INSERT INTO telegram.BotUser (TelegramId, Phone) VALUES (@UserId, @Phone);
		ELSE
			UPDATE telegram.BotUser SET Phone = @Phone WHERE TelegramId = @UserId;

		COMMIT TRANSACTION;

		EXECUTE telegram.GetUserRecordByUserId @UserId = @UserId;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO
/****** Object:  StoredProcedure [telegram].[GetBalance]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [telegram].[GetBalance]
	@EmployeeId bigint
AS
BEGIN
	SELECT TOP(10) bl.LogDateTime, bl.OperationType, bl.Amount, bl.PaymentId, p.OriginalAmount
		FROM dbo.EmployeeBalanceLog bl
		LEFT JOIN payment.Payment p ON p.PaymentId = bl.PaymentId
		WHERE bl.EmployeeId = @EmployeeId
		ORDER BY bl.EmployeeBalanceLogId DESC
END;
GO
/****** Object:  StoredProcedure [telegram].[GetEmployeeReport]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE     PROCEDURE [telegram].[GetEmployeeReport]
	@PlaceId int
AS
BEGIN
	DECLARE @curDt datetime2(7) = SYSDATETIME();
	DECLARE @dayTimeLimit datetime2(7) = CAST(CAST(DATEADD(day, -30, @curDt) as date) as datetime2(7));

	SELECT *
		FROM
		(
			SELECT epd.FirstName, epd.LastName, em.GroupId, g.Name GroupName,
					(SELECT ISNULL(SUM(Amount), 0)
						FROM dbo.EmployeeBalanceLog ebl
						WHERE ebl.EmployeeId = em.EmployeeId AND ebl.OperationType IN (1, 3) AND ebl.LogDateTime >= @dayTimeLimit) Amount
				FROM dbo.EmployeeMembershipHistory em
				INNER JOIN dbo.EmployeePersonalDataHistory epd ON epd.EmployeeId = em.EmployeeId AND @curDt BETWEEN epd.BeginDateTime AND epd.EndDateTime
				INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
				WHERE @curDt BETWEEN em.BeginDateTime AND em.EndDateTime AND em.PlaceId = @PlaceId AND em.IsFired = 0
		) t
		ORDER BY Amount DESC, CONCAT(FirstName, ' ', LastName);
END;
GO
/****** Object:  StoredProcedure [telegram].[GetUserRecordByUserId]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [telegram].[GetUserRecordByUserId]
	@UserId bigint
AS
BEGIN
	DECLARE @curDt datetime2(7) = SYSDATETIME();

	SELECT e.EmployeeId, e.Phone, pd.FirstName, pd.LastName, e.Balance, em.PlaceId, p.DisplayName PlaceName, p.Address PlaceAddress, p.City PlaceCity,
			em.GroupId, g.Name GroupName, em.IsFired, em.IsManager, em.IsOwner, t.QrCodeFileId,
			yks.Step YksStep, yks.UserLogin YksUserLogin, yks.Amount YksAmount, yks.ProviderId YksProviderId
		FROM telegram.BotUser t
		INNER JOIN dbo.Employee e ON e.Phone = t.Phone
		INNER JOIN dbo.EmployeeMembershipHistory em ON e.EmployeeId = em.EmployeeId AND @curDt BETWEEN em.BeginDateTime AND em.EndDateTime
		INNER JOIN dbo.Place p ON p.PlaceId = em.PlaceId
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
		INNER JOIN dbo.EmployeePersonalDataHistory pd ON pd.EmployeeId = e.EmployeeId AND @curDt BETWEEN pd.BeginDateTime AND pd.EndDateTime
		LEFT JOIN telegram.YandexKassaSession yks ON yks.TelegramId = t.TelegramId
		WHERE t.TelegramId = @UserId;
END;
GO
/****** Object:  StoredProcedure [telegram].[GetUsersToNotify]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE     PROCEDURE [telegram].[GetUsersToNotify]
AS
BEGIN
	SET NOCOUNT ON;

	CREATE TABLE #t1
	(
		EmployeeId int NOT NULL PRIMARY KEY CLUSTERED,
		TelegramId bigint NOT NULL,
		LastNotifiedLogId bigint NOT NULL
	);

	INSERT INTO #t1 (EmployeeId, TelegramId, LastNotifiedLogId)
	SELECT e.EmployeeId, u.TelegramId, ISNULL(u.LastBalanceLogId, 0) LastNotifiedLogId
		FROM telegram.BotUser u
		INNER JOIN dbo.Employee e ON u.Phone = e.Phone AND e.LastBalanceLogId IS NOT NULL AND ISNULL(u.LastBalanceLogId, 0) < e.LastBalanceLogId
		WHERE NOT EXISTS (SELECT * FROM telegram.YandexKassaSession yks WHERE yks.TelegramId = u.TelegramId);

	SELECT t.EmployeeId, t.TelegramId, l.EmployeeBalanceLogId, l.LogDateTime, l.OperationType, l.Amount, l.Balance, l.PaymentId, p.OriginalAmount
		FROM #t1 t
		INNER JOIN dbo.EmployeeBalanceLog l ON t.EmployeeId = l.EmployeeId AND l.EmployeeBalanceLogId > t.LastNotifiedLogId
		LEFT JOIN payment.Payment p ON l.PaymentId = p.PaymentId
		ORDER BY l.EmployeeId, l.EmployeeBalanceLogId;

	DROP TABLE #t1;
END;
GO
/****** Object:  StoredProcedure [telegram].[LoadSettings]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [telegram].[LoadSettings]
AS
BEGIN
	SELECT BotOffset FROM telegram.BotSettings;
END;
GO
/****** Object:  StoredProcedure [telegram].[StartYandexKassaSession]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [telegram].[StartYandexKassaSession]
	@TelegramId bigint
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRANSACTION;
	BEGIN TRY
		DELETE FROM telegram.YandexKassaSession WHERE TelegramId = @TelegramId;
		INSERT INTO telegram.YandexKassaSession (TelegramId, UserLogin, Amount, ProviderId, Step)
			VALUES (@TelegramId, NULL, NULL, 0, 0);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END
GO
/****** Object:  StoredProcedure [telegram].[UpdateBotOffset]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE     PROCEDURE [telegram].[UpdateBotOffset]
	@BotOffset int
AS
BEGIN
	BEGIN TRANSACTION;
	BEGIN TRY
		UPDATE telegram.BotSettings SET BotOffset = @BotOffset;
		IF (@@ROWCOUNT = 0)
		BEGIN
			INSERT INTO telegram.BotSettings (BotOffset) VALUES (@BotOffset);
		END;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO
/****** Object:  StoredProcedure [telegram].[UpdateLastBalanceLogId]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [telegram].[UpdateLastBalanceLogId]
	@UserId bigint,
	@LastBalanceLogId bigint
AS
BEGIN
	UPDATE telegram.BotUser SET LastBalanceLogId = @LastBalanceLogId WHERE TelegramId = @UserId;
END;
GO
/****** Object:  StoredProcedure [telegram].[UpdateQrCodeFileId]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE     PROCEDURE [telegram].[UpdateQrCodeFileId]
	@UserId bigint,
	@QrCodeFileId varchar(64)
AS
BEGIN
	UPDATE telegram.BotUser SET QrCodeFileId = @QrCodeFileId WHERE TelegramId = @UserId;
END;
GO
/****** Object:  StoredProcedure [telegram].[UpdateYandexKassaSession]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE     PROCEDURE [telegram].[UpdateYandexKassaSession]
	@TelegramId bigint,
	@UserLogin varchar(50),
	@Amount decimal(18,2),
	@ProviderId tinyint,
	@Step tinyint
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE telegram.YandexKassaSession SET UserLogin = @UserLogin, Amount = @Amount, ProviderId = @ProviderId, Step = @Step
		WHERE TelegramId = @TelegramId;
	IF (@@ROWCOUNT = 0)
		RETURN -1;
	
	RETURN 0;
END
GO
/****** Object:  Trigger [dbo].[EmployeeBalanceLog_UpdateMaxId]    Script Date: 12/17/2018 2:06:31 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER [dbo].[EmployeeBalanceLog_UpdateMaxId] 
   ON  [dbo].[EmployeeBalanceLog]
   AFTER INSERT
AS 
BEGIN
	SET NOCOUNT ON;
	UPDATE e SET LastBalanceLogId = (SELECT MAX(l.EmployeeBalanceLogId) FROM inserted l WHERE l.EmployeeId = e.EmployeeId)
		FROM dbo.Employee e;
END
GO
ALTER TABLE [dbo].[EmployeeBalanceLog] ENABLE TRIGGER [EmployeeBalanceLog_UpdateMaxId]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Номер телефона (логин)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'Phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время регистрации' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'RegisterDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Текущий баланс' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'Balance'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeAuth', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Пин-код (пароль)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeAuth', @level2type=N'COLUMN',@level2name=N'PinCode'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Постоянный ключ авторизации' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeAuth', @level2type=N'COLUMN',@level2name=N'PermanentKey'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Ключ авторизации в закрытый раздел' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeAuth', @level2type=N'COLUMN',@level2name=N'SecuredKey'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время последнего доступа по защищённому ключу' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeAuth', @level2type=N'COLUMN',@level2name=N'SecuredKeyLastAccessDt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Количество неправильных вводов логина и пароля' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeAuth', @level2type=N'COLUMN',@level2name=N'FailedAuthCount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время последнего неправильного ввода логина и пароля' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeAuth', @level2type=N'COLUMN',@level2name=N'LastFailedAuthDt'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Порядковый номер изменения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'EmployeeBalanceLogId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время изменения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'LogDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Тип операции' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'OperationType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Величина изменения баланса' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор заведения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'GroupId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время начала членства в группе' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время окончания членства в группе' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'EndDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "уволен"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'IsFired'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "менеджер"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'IsManager'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "владелец"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembershipHistory', @level2type=N'COLUMN',@level2name=N'IsOwner'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalDataHistory', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВП/C. Дата и время начала действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalDataHistory', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВП/C. Дата и время окончания действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalDataHistory', @level2type=N'COLUMN',@level2name=N'EndDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Имя' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalDataHistory', @level2type=N'COLUMN',@level2name=N'FirstName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Фамилия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalDataHistory', @level2type=N'COLUMN',@level2name=N'LastName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Серия и номер паспорта' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalDataHistory', @level2type=N'COLUMN',@level2name=N'PassportNum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Файл со сканом паспорта' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalDataHistory', @level2type=N'COLUMN',@level2name=N'PassportScanFile'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Параметр ссылки на регистрацию' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeRegistrationLink', @level2type=N'COLUMN',@level2name=N'LinkParameter'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Заведение, к которому будет прикреплён сотрудник' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeRegistrationLink', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время генерации ссылки' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeRegistrationLink', @level2type=N'COLUMN',@level2name=N'CreateDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время истечение срока годности ссылки' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeRegistrationLink', @level2type=N'COLUMN',@level2name=N'ExpireDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "менеджер"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeRegistrationLink', @level2type=N'COLUMN',@level2name=N'IsManager'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "владелец"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeRegistrationLink', @level2type=N'COLUMN',@level2name=N'IsOwner'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Телефон, который будет логином нового менеджера' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeRegistrationLink', @level2type=N'COLUMN',@level2name=N'Phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'PlaceID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время регистрации в системе' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'RegisterDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Отображаемое наименование' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'DisplayName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Адрес' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'Address'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Город' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'City'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Контактный телефон' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'Phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ИНН' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'Inn'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Электронный адрес' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'Email'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'РАЗРАБОТАТЬ!!!' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'TimeZoneId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Дополнительная информация' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Place', @level2type=N'COLUMN',@level2name=N'Info'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PlaceGroup', @level2type=N'COLUMN',@level2name=N'GroupId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Название группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PlaceGroup', @level2type=N'COLUMN',@level2name=N'Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PlaceGroup'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PpAcceptance', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор версии политики по обработке персональных данных' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PpAcceptance', @level2type=N'COLUMN',@level2name=N'PrivacyPolicyId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВП/С. Дата и время принятия версии политики по обработке персональных данных' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PpAcceptance', @level2type=N'COLUMN',@level2name=N'AcceptDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrivacyPolicy', @level2type=N'COLUMN',@level2name=N'PrivacyPolicyId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Версия документа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrivacyPolicy', @level2type=N'COLUMN',@level2name=N'Version'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL с текстом документа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrivacyPolicy', @level2type=N'COLUMN',@level2name=N'Url'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время выпуска' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PrivacyPolicy', @level2type=N'COLUMN',@level2name=N'ExposureDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор заведения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareSchemeHistory', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Индивидуальная доля' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareSchemeHistory', @level2type=N'COLUMN',@level2name=N'PersonalShare'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время начала действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareSchemeHistory', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время окончания действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareSchemeHistory', @level2type=N'COLUMN',@level2name=N'EndDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сообщения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SmsMessage', @level2type=N'COLUMN',@level2name=N'MessageId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Тип сообщения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SmsMessage', @level2type=N'COLUMN',@level2name=N'MessageType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время создания сообщения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SmsMessage', @level2type=N'COLUMN',@level2name=N'CreateDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время отправки сообщения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SmsMessage', @level2type=N'COLUMN',@level2name=N'SendDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Параметры сообщения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SmsMessage', @level2type=N'COLUMN',@level2name=N'MessageParams'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShareHistory', @level2type=N'COLUMN',@level2name=N'GroupId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Вес группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShareHistory', @level2type=N'COLUMN',@level2name=N'GroupWeight'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'UaAcceptance', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор версии пользовательского соглашения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'UaAcceptance', @level2type=N'COLUMN',@level2name=N'UserAgreementId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВП/С. Дата и время принятия версии политики по обработке персональных данных' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'UaAcceptance', @level2type=N'COLUMN',@level2name=N'AcceptDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'UserAgreement', @level2type=N'COLUMN',@level2name=N'UserAgreementId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Версия документа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'UserAgreement', @level2type=N'COLUMN',@level2name=N'Version'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'URL с текстом документа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'UserAgreement', @level2type=N'COLUMN',@level2name=N'Url'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время выпуска' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'UserAgreement', @level2type=N'COLUMN',@level2name=N'ExposureDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор кода верификации' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'VerificationCode', @level2type=N'COLUMN',@level2name=N'VerificationId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Код верификации' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'VerificationCode', @level2type=N'COLUMN',@level2name=N'Code'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время генерации кода верификации' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'VerificationCode', @level2type=N'COLUMN',@level2name=N'GenerationDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата в время истечение срока действия кода' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'VerificationCode', @level2type=N'COLUMN',@level2name=N'ExpireDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор заведения' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника для персонального платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Тип платежа (0 - Сбербанк.Онлайн)' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'DataSourceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Исходная сумма платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'OriginalAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма, полученая нами' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'ReceivedAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма комиссии платёжной системы' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'BankCommissionAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма предполагаемого дохода до уплаты комиссии на вывод' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'IncomeAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма к выплате сотрудникам заведения' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PayoutAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВНО. Дата и время совершения платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PaymentDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время внесения данных в систему' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'ArrivalDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Внешний идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'ExternalId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ФИО, прописанное в платеже' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'Fio'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Адрес платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'Address'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Назначение платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'Purpose'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentShare', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentShare', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма, полагающая к выплате сотруднику по итогам конкретного платежа' , @level0type=N'SCHEMA',@level0name=N'payment', @level1type=N'TABLE',@level1name=N'PaymentShare', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id пользователя в телеграм' , @level0type=N'SCHEMA',@level0name=N'telegram', @level1type=N'TABLE',@level1name=N'BotUser', @level2type=N'COLUMN',@level2name=N'TelegramId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Телефон пользователя' , @level0type=N'SCHEMA',@level0name=N'telegram', @level1type=N'TABLE',@level1name=N'BotUser', @level2type=N'COLUMN',@level2name=N'Phone'
GO
