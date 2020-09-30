USE [master]
GO
/****** Object:  Database [tipsv2]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE DATABASE [tipsv2];
GO
ALTER DATABASE [tipsv2] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [tipsv2].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [tipsv2] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [tipsv2] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [tipsv2] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [tipsv2] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [tipsv2] SET ARITHABORT OFF 
GO
ALTER DATABASE [tipsv2] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [tipsv2] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [tipsv2] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [tipsv2] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [tipsv2] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [tipsv2] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [tipsv2] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [tipsv2] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [tipsv2] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [tipsv2] SET  DISABLE_BROKER 
GO
ALTER DATABASE [tipsv2] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [tipsv2] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [tipsv2] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [tipsv2] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [tipsv2] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [tipsv2] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [tipsv2] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [tipsv2] SET RECOVERY FULL 
GO
ALTER DATABASE [tipsv2] SET  MULTI_USER 
GO
ALTER DATABASE [tipsv2] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [tipsv2] SET DB_CHAINING OFF 
GO
ALTER DATABASE [tipsv2] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [tipsv2] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [tipsv2] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [tipsv2] SET QUERY_STORE = OFF
GO
GO
USE [tipsv2]
GO
/****** Object:  Sequence [dbo].[Seq_Employee]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE SEQUENCE [dbo].[Seq_Employee] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tipsv2]
GO
/****** Object:  Sequence [dbo].[Seq_Manager]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE SEQUENCE [dbo].[Seq_Manager] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tipsv2]
GO
/****** Object:  Sequence [dbo].[Seq_Payment]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE SEQUENCE [dbo].[Seq_Payment] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tipsv2]
GO
/****** Object:  Sequence [dbo].[Seq_Place]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE SEQUENCE [dbo].[Seq_Place] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tipsv2]
GO
/****** Object:  Sequence [dbo].[Seq_PlaceGroup]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE SEQUENCE [dbo].[Seq_PlaceGroup] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tipsv2]
GO
/****** Object:  Sequence [dbo].[Seq_SmsMessage]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE SEQUENCE [dbo].[Seq_SmsMessage] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tipsv2]
GO
/****** Object:  Sequence [dbo].[Seq_YandexKassaInvoice]    Script Date: 11/12/2018 7:45:48 PM ******/
CREATE SEQUENCE [dbo].[Seq_YandexKassaInvoice] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
/****** Object:  Table [dbo].[Employee]    Script Date: 11/12/2018 7:45:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employee](
	[EmployeeId] [int] NOT NULL,
	[Phone] [char](10) NOT NULL,
	[PlaceId] [int] NOT NULL,
	[GroupId] [int] NULL,
	[RegisterDateTime] [datetimeoffset](7) NOT NULL,
	[Balance] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeAuth]    Script Date: 11/12/2018 7:45:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeAuth](
	[EmployeeId] [int] NOT NULL,
	[PinCode] [char](4) NOT NULL,
	[PermanentKey] [binary](16) NULL,
	[SecuredKey] [binary](16) NULL,
	[SecuredKeyLastAccessDt] [datetimeoffset](7) NULL,
	[FailedAuthCount] [tinyint] NOT NULL,
	[LastFailedAuthDt] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_EmployeeAuth] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeBalanceLog]    Script Date: 11/12/2018 7:45:48 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeBalanceLog](
	[EmployeeId] [int] NOT NULL,
	[SeqNum] [bigint] NOT NULL,
	[LogDateTime] [datetimeoffset](7) NOT NULL,
	[OperationType] [tinyint] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[PaymentId] [bigint] NOT NULL,
 CONSTRAINT [PK_EmployeeBalanceLog] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[SeqNum] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeMembership]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeMembership](
	[EmployeeId] [int] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[BeginDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NULL,
	[IsFired] [bit] NOT NULL,
	[IsManager] [bit] NOT NULL,
	[IsOwner] [bit] NOT NULL,
 CONSTRAINT [PK_EmployeeMembership] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeMembershipHistory]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeMembershipHistory](
	[EmployeeId] [int] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[BeginDateTime] [datetime2](7) NOT NULL,
	[EndDateTime] [datetime2](7) NULL,
	[IsFired] [bit] NOT NULL,
	[IsManager] [bit] NOT NULL,
	[IsOwner] [bit] NOT NULL,
	[OpenedBy] [nchar](10) NULL
) ON [PRIMARY]
GO
/****** Object:  Index [IX_EmployeeMembershipHistory_EmployeeId_BeginDateTime]    Script Date: 11/12/2018 7:45:49 PM ******/
CREATE CLUSTERED INDEX [IX_EmployeeMembershipHistory_EmployeeId_BeginDateTime] ON [dbo].[EmployeeMembershipHistory]
(
	[EmployeeId] ASC,
	[BeginDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeePayoutData]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeePayoutData](
	[EmployeeId] [int] NOT NULL,
	[PayoutToken] [varchar](20) NULL,
	[MaskedCardNumber] [varchar](20) NULL,
 CONSTRAINT [PK_EmployeePayoutData] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeePersonalData]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeePersonalData](
	[EmployeeId] [int] NOT NULL,
	[BeginDateTime] [datetimeoffset](7) NOT NULL,
	[EndDateTime] [datetimeoffset](7) NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[PassportNum] [char](10) NULL,
	[PassportScanFile] [varchar](50) NULL,
 CONSTRAINT [PK_EmployeePersonalData] PRIMARY KEY NONCLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_EmployeePersonalData_EmployeeId_BeginDateTime]    Script Date: 11/12/2018 7:45:49 PM ******/
CREATE CLUSTERED INDEX [IX_EmployeePersonalData_EmployeeId_BeginDateTime] ON [dbo].[EmployeePersonalData]
(
	[EmployeeId] ASC,
	[BeginDateTime] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeePersonalDataHistory]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeePersonalDataHistory](
	[EmployeeId] [int] NOT NULL,
	[BeginDateTime] [datetimeoffset](7) NOT NULL,
	[EndDateTime] [datetimeoffset](7) NULL,
	[FirstName] [nvarchar](50) NOT NULL,
	[LastName] [nvarchar](50) NOT NULL,
	[PassportNum] [char](10) NULL,
	[PassportScanFile] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Index [PK_EmployeePersonalDataHistory]    Script Date: 11/12/2018 7:45:49 PM ******/
CREATE UNIQUE CLUSTERED INDEX [PK_EmployeePersonalDataHistory] ON [dbo].[EmployeePersonalDataHistory]
(
	[EmployeeId] ASC,
	[EndDateTime] ASC,
	[BeginDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeRegistrationLink]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EmployeeRegistrationLink](
	[LinkParameter] [uniqueidentifier] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[CreateDateTime] [datetimeoffset](7) NOT NULL,
	[ExpireDateTime] [datetimeoffset](7) NULL,
	[IsManager] [bit] NOT NULL,
	[IsOwner] [bit] NOT NULL,
	[Phone] [char](10) NULL,
 CONSTRAINT [PK_EmployeeRegistrationLink] PRIMARY KEY CLUSTERED 
(
	[LinkParameter] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[PaymentId] [bigint] NOT NULL,
	[PlaceId] [int] NOT NULL,
	[EmployeeId] [int] NULL,
	[PaymentType] [tinyint] NOT NULL,
	[OriginalAmount] [decimal](18, 2) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[CommissionAmount] [decimal](18, 2) NOT NULL,
	[IncomeAmount] [decimal](18, 2) NOT NULL,
	[PayoutAmount] [decimal](18, 2) NOT NULL,
	[PaymentDateTime] [datetimeoffset](7) NOT NULL,
	[ArrivalDateTime] [datetimeoffset](7) NOT NULL,
	[ReturnDateTime] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentExternalData]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentExternalData](
	[PaymentId] [bigint] NOT NULL,
	[ExternalId] [varchar](50) NOT NULL,
	[PayerName] [nvarchar](100) NULL,
	[PayerAddress] [nvarchar](200) NULL,
	[PaymentPurpose] [nvarchar](200) NULL,
 CONSTRAINT [PK_PaymentExternalData] PRIMARY KEY CLUSTERED 
(
	[ExternalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentShare]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentShare](
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
/****** Object:  Table [dbo].[Place]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Place](
	[PlaceID] [int] NOT NULL,
	[RegisterDateTime] [datetimeoffset](7) NOT NULL,
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
/****** Object:  Table [dbo].[PlaceGroup]    Script Date: 11/12/2018 7:45:49 PM ******/
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
/****** Object:  Table [dbo].[PpAcceptance]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PpAcceptance](
	[EmployeeId] [int] NOT NULL,
	[PrivacyPolicyId] [int] NOT NULL,
	[AcceptDateTime] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_PpAcceptance] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[PrivacyPolicyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PrivacyPolicy]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrivacyPolicy](
	[PrivacyPolicyId] [int] NOT NULL,
	[Version] [nvarchar](10) NOT NULL,
	[Url] [varchar](200) NOT NULL,
	[ExposureDateTime] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_PrivacyPolicy] PRIMARY KEY CLUSTERED 
(
	[PrivacyPolicyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShareScheme]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShareScheme](
	[PlaceId] [int] NOT NULL,
	[PersonalShare] [tinyint] NOT NULL,
	[BeginDateTime] [datetimeoffset](7) NOT NULL,
	[EndDateTime] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_ShareScheme] PRIMARY KEY CLUSTERED 
(
	[PlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShareSchemeHistory]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShareSchemeHistory](
	[PlaceId] [int] NOT NULL,
	[PersonalShare] [tinyint] NOT NULL,
	[BeginDateTime] [datetimeoffset](7) NOT NULL,
	[EndDateTime] [datetimeoffset](7) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Index [ix_ShareSchemeHistory]    Script Date: 11/12/2018 7:45:49 PM ******/
CREATE CLUSTERED INDEX [ix_ShareSchemeHistory] ON [dbo].[ShareSchemeHistory]
(
	[PlaceId] ASC,
	[EndDateTime] ASC,
	[BeginDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SmsMessage]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SmsMessage](
	[MessageId] [bigint] NOT NULL,
	[MessageType] [tinyint] NOT NULL,
	[CreateDateTime] [datetimeoffset](7) NOT NULL,
	[SendDateTime] [datetimeoffset](7) NULL,
	[MessageParams] [nvarchar](max) NULL,
 CONSTRAINT [PK_SmsMessage] PRIMARY KEY CLUSTERED 
(
	[MessageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SsGroupShare]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SsGroupShare](
	[PlaceId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[GroupWeight] [tinyint] NOT NULL,
	[BeginDateTime] [datetimeoffset](7) NOT NULL,
	[EndDateTime] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_SsGroupShare] PRIMARY KEY CLUSTERED 
(
	[PlaceId] ASC,
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SsGroupShareHistory]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SsGroupShareHistory](
	[PlaceId] [int] NOT NULL,
	[GroupId] [int] NOT NULL,
	[GroupWeight] [tinyint] NOT NULL,
	[BeginDateTime] [datetimeoffset](7) NOT NULL,
	[EndDateTime] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_SsGroupShareHistory] PRIMARY KEY CLUSTERED 
(
	[PlaceId] ASC,
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TelegramUser]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TelegramUser](
	[TelegramId] [bigint] NOT NULL,
	[Phone] [char](10) NOT NULL,
	[QrCodeFileId] [char](56) NULL,
 CONSTRAINT [PK_TelegramUser] PRIMARY KEY CLUSTERED 
(
	[TelegramId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UaAcceptance]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UaAcceptance](
	[EmployeeId] [int] NOT NULL,
	[UserAgreementId] [int] NOT NULL,
	[AcceptDateTime] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_UaAcceptance] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC,
	[UserAgreementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAgreement]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAgreement](
	[UserAgreementId] [int] NOT NULL,
	[Version] [nvarchar](10) NOT NULL,
	[Url] [varchar](200) NOT NULL,
	[ExposureDateTime] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_UserAgreement] PRIMARY KEY CLUSTERED 
(
	[UserAgreementId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VerificationCode]    Script Date: 11/12/2018 7:45:49 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VerificationCode](
	[VerificationId] [uniqueidentifier] NOT NULL,
	[Code] [char](4) NOT NULL,
	[GenerationDateTime] [datetimeoffset](7) NOT NULL,
	[ExpireDateTime] [datetimeoffset](7) NULL,
 CONSTRAINT [PK_VerificationCode] PRIMARY KEY CLUSTERED 
(
	[VerificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_EmployeeAuth_EmployeeId]    Script Date: 11/12/2018 7:45:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_EmployeeAuth_EmployeeId] ON [dbo].[EmployeeAuth]
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_EmployeeAuth_PermanentKey]    Script Date: 11/12/2018 7:45:50 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_EmployeeAuth_PermanentKey] ON [dbo].[EmployeeAuth]
(
	[PermanentKey] ASC
)
WHERE ([PermanentKey] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_EmployeeId]    Script Date: 11/12/2018 7:45:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_EmployeeId] ON [dbo].[Payment]
(
	[EmployeeId] ASC,
	[PaymentDateTime] ASC
)
WHERE ([EmployeeId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_PlaceId]    Script Date: 11/12/2018 7:45:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_Payment_PlaceId] ON [dbo].[Payment]
(
	[PlaceId] ASC,
	[PaymentDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_PaymentExternalData_PaymentId]    Script Date: 11/12/2018 7:45:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_PaymentExternalData_PaymentId] ON [dbo].[PaymentExternalData]
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_PaymentShare_EmployeeId_PaymentId]    Script Date: 11/12/2018 7:45:50 PM ******/
CREATE NONCLUSTERED INDEX [IX_PaymentShare_EmployeeId_PaymentId] ON [dbo].[PaymentShare]
(
	[EmployeeId] ASC,
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_ShareSchemeHistory2]    Script Date: 11/12/2018 7:45:50 PM ******/
CREATE NONCLUSTERED INDEX [ix_ShareSchemeHistory2] ON [dbo].[ShareSchemeHistory]
(
	[EndDateTime] ASC,
	[BeginDateTime] ASC,
	[PlaceId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF__Employee__EmployeeId]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_Employee]) FOR [EmployeeId]
GO
ALTER TABLE [dbo].[Employee] ADD  CONSTRAINT [DF_Employee_Balance]  DEFAULT ((0)) FOR [Balance]
GO
ALTER TABLE [dbo].[EmployeeAuth] ADD  CONSTRAINT [DF__EmployeeA__FailedAuthCount]  DEFAULT ((0)) FOR [FailedAuthCount]
GO
ALTER TABLE [dbo].[EmployeeMembership] ADD  CONSTRAINT [DF_EmployeeMembership_IsFired]  DEFAULT ((0)) FOR [IsFired]
GO
ALTER TABLE [dbo].[EmployeeMembership] ADD  CONSTRAINT [DF_EmployeeMembership_IsManager]  DEFAULT ((0)) FOR [IsManager]
GO
ALTER TABLE [dbo].[EmployeeMembership] ADD  CONSTRAINT [DF_EmployeeMembership_IsOwner]  DEFAULT ((0)) FOR [IsOwner]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_IsFired]  DEFAULT ((0)) FOR [IsFired]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_IsManager]  DEFAULT ((0)) FOR [IsManager]
GO
ALTER TABLE [dbo].[EmployeeMembershipHistory] ADD  CONSTRAINT [DF_EmployeeMembershipHistory_IsOwner]  DEFAULT ((0)) FOR [IsOwner]
GO
ALTER TABLE [dbo].[EmployeeRegistrationLink] ADD  CONSTRAINT [DF_EmployeeRegistrationLink_LinkParameter]  DEFAULT (newid()) FOR [LinkParameter]
GO
ALTER TABLE [dbo].[EmployeeRegistrationLink] ADD  CONSTRAINT [DF_EmployeeRegistrationLink_IsManager]  DEFAULT ((0)) FOR [IsManager]
GO
ALTER TABLE [dbo].[EmployeeRegistrationLink] ADD  CONSTRAINT [DF_EmployeeRegistrationLink_IsOwner]  DEFAULT ((0)) FOR [IsOwner]
GO
ALTER TABLE [dbo].[Payment] ADD  CONSTRAINT [DF__Payment__PaymentId]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_Payment]) FOR [PaymentId]
GO
ALTER TABLE [dbo].[Place] ADD  CONSTRAINT [DF__Place__PlaceID]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_Place]) FOR [PlaceID]
GO
ALTER TABLE [dbo].[Place] ADD  CONSTRAINT [DF_Place_RegisterDateTime]  DEFAULT (sysdatetimeoffset()) FOR [RegisterDateTime]
GO
ALTER TABLE [dbo].[Place] ADD  CONSTRAINT [DF_Place_TimeZoneId]  DEFAULT ('RTZ2') FOR [TimeZoneId]
GO
ALTER TABLE [dbo].[PlaceGroup] ADD  CONSTRAINT [DF__PlaceGrou__GroupId]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_PlaceGroup]) FOR [GroupId]
GO
ALTER TABLE [dbo].[SmsMessage] ADD  CONSTRAINT [DF__SmsMessag__MessageId]  DEFAULT (NEXT VALUE FOR [dbo].[Seq_SmsMessage]) FOR [MessageId]
GO
ALTER TABLE [dbo].[UserAgreement] ADD  CONSTRAINT [DF_UserAgreement_ExposureDateTime]  DEFAULT (getdate()) FOR [ExposureDateTime]
GO
ALTER TABLE [dbo].[VerificationCode] ADD  CONSTRAINT [DF_VerificationCode_VerificationId]  DEFAULT (newid()) FOR [VerificationId]
GO
/****** Object:  StoredProcedure [dbo].[Employee_CheckEmployeeStatus]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_CheckVerificationCode]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_EnterSecuredSession]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_ExitSecuredSession]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_FollowRegistrationLink]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_JoinPlace]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_KeepSecuredSessionAlive]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_Login]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_Logout]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_Register]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Employee_RegisterVerificationCode]    Script Date: 11/12/2018 7:45:50 PM ******/
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
/****** Object:  StoredProcedure [dbo].[Telegram_CreateAndGetUserRecord]    Script Date: 11/12/2018 7:45:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Telegram_CreateAndGetUserRecord]
	@UserId bigint,
	@Phone char(10)
AS
BEGIN
	BEGIN TRANSACTION
	BEGIN TRY
		IF (NOT EXISTS(SELECT * FROM dbo.TelegramUser with (updlock, serializable) WHERE TelegramId = @UserId))
			INSERT INTO dbo.TelegramUser (TelegramId, Phone) VALUES (@UserId, @Phone);
		ELSE
			UPDATE dbo.TelegramUser SET Phone = @Phone WHERE TelegramId = @UserId;

		COMMIT TRANSACTION;

		EXECUTE dbo.Telegram_GetUserRecordByUserId @UserId = @UserId;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[Telegram_GetBalance]    Script Date: 11/12/2018 7:45:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Telegram_GetBalance]
	@EmployeeId bigint
AS
BEGIN
	SELECT TOP(5) bl.LogDateTime, bl.OperationType, bl.Amount, bl.PaymentId, p.OriginalAmount
		FROM dbo.EmployeeBalanceLog bl
		LEFT JOIN dbo.Payment p ON p.PaymentId = bl.PaymentId
		WHERE bl.EmployeeId = @EmployeeId
		ORDER BY bl.SeqNum DESC
END;
GO
/****** Object:  StoredProcedure [dbo].[Telegram_GetEmployeeReport]    Script Date: 11/12/2018 7:45:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[Telegram_GetEmployeeReport]
	@PlaceId int
AS
BEGIN
	DECLARE @dayTimeLimit datetimeoffset(7) = DATEADD(day, -30, SYSDATETIMEOFFSET());

	SELECT *
		FROM
		(
			SELECT epd.FirstName, epd.LastName, em.GroupId, g.Name GroupName,
					(SELECT ISNULL(SUM(Amount), 0)
						FROM dbo.EmployeeBalanceLog ebl
						WHERE ebl.EmployeeId = em.EmployeeId AND ebl.OperationType IN (1, 3) AND ebl.LogDateTime >= @dayTimeLimit) Amount
				FROM dbo.EmployeeMembership em
				INNER JOIN dbo.EmployeePersonalData epd ON epd.EmployeeId = em.EmployeeId
				INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
				WHERE em.PlaceId = @PlaceId AND em.IsFired = 0
		) t
		ORDER BY Amount DESC, CONCAT(FirstName, ' ', LastName);
END;
GO
/****** Object:  StoredProcedure [dbo].[Telegram_GetUserRecordByUserId]    Script Date: 11/12/2018 7:45:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Telegram_GetUserRecordByUserId]
	@UserId bigint
AS
BEGIN
	SELECT e.EmployeeId, e.Phone, pd.FirstName, pd.LastName, e.Balance, em.PlaceId, p.DisplayName PlaceName, p.Address PlaceAddress, p.City PlaceCity,
			em.GroupId, g.Name GroupName, em.IsFired, em.IsManager, em.IsOwner, t.QrCodeFileId
		FROM dbo.TelegramUser t
		INNER JOIN dbo.Employee e ON e.Phone = t.Phone
		INNER JOIN dbo.EmployeeMembership em ON e.EmployeeId = em.EmployeeId
		INNER JOIN dbo.Place p ON p.PlaceId = em.PlaceId
		INNER JOIN dbo.PlaceGroup g ON g.GroupId = em.GroupId
		INNER JOIN dbo.EmployeePersonalData pd ON pd.EmployeeId = e.EmployeeId
		WHERE t.TelegramId = @UserId;
END;
GO
/****** Object:  StoredProcedure [dbo].[Telegram_UpdateQrCodeFileId]    Script Date: 11/12/2018 7:45:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[Telegram_UpdateQrCodeFileId]
	@UserId bigint,
	@QrCodeFileId char(56)
AS
BEGIN
	UPDATE dbo.TelegramUser SET QrCodeFileId = @QrCodeFileId WHERE TelegramId = @UserId;
END;
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Номер телефона (логин)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'Phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Заведение' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Группа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Employee', @level2type=N'COLUMN',@level2name=N'GroupId'
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
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Порядковый номер изменения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'SeqNum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время изменения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'LogDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Тип операции' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'OperationType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Величина изменения баланса' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeBalanceLog', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор заведения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'GroupId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время начала членства в группе' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время окончания членства в группе' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'EndDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "уволен"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'IsFired'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "менеджер"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'IsManager'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Флаг "владелец"' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeeMembership', @level2type=N'COLUMN',@level2name=N'IsOwner'
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
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePayoutData', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Токен сохранённой банковской карты' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePayoutData', @level2type=N'COLUMN',@level2name=N'PayoutToken'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Маскированный номер банковской карты' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePayoutData', @level2type=N'COLUMN',@level2name=N'MaskedCardNumber'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalData', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВП/C. Дата и время начала действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalData', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВП/C. Дата и время окончания действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalData', @level2type=N'COLUMN',@level2name=N'EndDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Имя' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalData', @level2type=N'COLUMN',@level2name=N'FirstName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Фамилия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalData', @level2type=N'COLUMN',@level2name=N'LastName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Серия и номер паспорта' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalData', @level2type=N'COLUMN',@level2name=N'PassportNum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Файл со сканом паспорта' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EmployeePersonalData', @level2type=N'COLUMN',@level2name=N'PassportScanFile'
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
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Синтетический ключ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор заведения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника для персонального платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Тип платежа (0 - Сбербанк.Онлайн)' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PaymentType'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Исходная сумма платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'OriginalAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма, полученая нами' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'Amount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма комиссии платёжной системы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'CommissionAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма предполагаемого дохода до уплаты комиссии на вывод' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'IncomeAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма к выплате сотрудникам заведения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PayoutAmount'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВНО. Дата и время совершения платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'PaymentDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВС. Дата и время внесения данных в систему' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Payment', @level2type=N'COLUMN',@level2name=N'ArrivalDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Внешний идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'ExternalId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ФИО, прописанное в платеже' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'PayerName'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Адрес платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'PayerAddress'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Назначение платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentExternalData', @level2type=N'COLUMN',@level2name=N'PaymentPurpose'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentShare', @level2type=N'COLUMN',@level2name=N'PaymentId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор сотрудника' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentShare', @level2type=N'COLUMN',@level2name=N'EmployeeId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Сумма, полагающая к выплате сотруднику по итогам конкретного платежа' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'PaymentShare', @level2type=N'COLUMN',@level2name=N'Amount'
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
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор заведения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareScheme', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Индивидуальная доля' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareScheme', @level2type=N'COLUMN',@level2name=N'PersonalShare'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время начала действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareScheme', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время окончания действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ShareScheme', @level2type=N'COLUMN',@level2name=N'EndDateTime'
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
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор схемы распределения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShare', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShare', @level2type=N'COLUMN',@level2name=N'GroupId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Вес группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShare', @level2type=N'COLUMN',@level2name=N'GroupWeight'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время начала действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShare', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время окончания действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShare', @level2type=N'COLUMN',@level2name=N'EndDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор схемы распределения' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShareHistory', @level2type=N'COLUMN',@level2name=N'PlaceId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Идентификатор группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShareHistory', @level2type=N'COLUMN',@level2name=N'GroupId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Вес группы' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShareHistory', @level2type=N'COLUMN',@level2name=N'GroupWeight'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время начала действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShareHistory', @level2type=N'COLUMN',@level2name=N'BeginDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'ВЗ. Дата и время окончания действия' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SsGroupShareHistory', @level2type=N'COLUMN',@level2name=N'EndDateTime'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Id пользователя в телеграм' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TelegramUser', @level2type=N'COLUMN',@level2name=N'TelegramId'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Телефон пользователя' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'TelegramUser', @level2type=N'COLUMN',@level2name=N'Phone'
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
USE [master]
GO
ALTER DATABASE [tipsv2] SET  READ_WRITE 
GO
