USE [master]
GO
/****** Object:  Database [tips]    Script Date: 06.11.2018 17:42:57 ******/
CREATE DATABASE [tips]
GO
ALTER DATABASE [tips] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [tips].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [tips] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [tips] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [tips] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [tips] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [tips] SET ARITHABORT OFF 
GO
ALTER DATABASE [tips] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [tips] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [tips] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [tips] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [tips] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [tips] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [tips] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [tips] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [tips] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [tips] SET  DISABLE_BROKER 
GO
ALTER DATABASE [tips] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [tips] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [tips] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [tips] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [tips] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [tips] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [tips] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [tips] SET RECOVERY FULL 
GO
ALTER DATABASE [tips] SET  MULTI_USER 
GO
ALTER DATABASE [tips] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [tips] SET DB_CHAINING OFF 
GO
ALTER DATABASE [tips] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [tips] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [tips] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [tips] SET QUERY_STORE = OFF
GO
USE [tips]
GO
ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [tips]
GO
/****** Object:  Sequence [dbo].[Seq_Employee]    Script Date: 06.11.2018 17:42:57 ******/
CREATE SEQUENCE [dbo].[Seq_Employee] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tips]
GO
/****** Object:  Sequence [dbo].[Seq_Payment]    Script Date: 06.11.2018 17:42:57 ******/
CREATE SEQUENCE [dbo].[Seq_Payment] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tips]
GO
/****** Object:  Sequence [dbo].[Seq_Place]    Script Date: 06.11.2018 17:42:57 ******/
CREATE SEQUENCE [dbo].[Seq_Place] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tips]
GO
/****** Object:  Sequence [dbo].[Seq_PlaceGroup]    Script Date: 06.11.2018 17:42:57 ******/
CREATE SEQUENCE [dbo].[Seq_PlaceGroup] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tips]
GO
/****** Object:  Sequence [dbo].[Seq_SmsMessage]    Script Date: 06.11.2018 17:42:57 ******/
CREATE SEQUENCE [dbo].[Seq_SmsMessage] 
 AS [int]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -2147483648
 MAXVALUE 2147483647
 CACHE  1000 
GO
USE [tips]
GO
/****** Object:  Table [dbo].[Employee]    Script Date: 06.11.2018 17:42:57 ******/
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
/****** Object:  Table [dbo].[EmployeeAuth]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Table [dbo].[EmployeeBalanceLog]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Table [dbo].[EmployeeMembership]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Table [dbo].[EmployeeMembershipHistory]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Index [IX_EmployeeMembershipHistory_EmployeeId_BeginDateTime]    Script Date: 06.11.2018 17:42:58 ******/
CREATE CLUSTERED INDEX [IX_EmployeeMembershipHistory_EmployeeId_BeginDateTime] ON [dbo].[EmployeeMembershipHistory]
(
	[EmployeeId] ASC,
	[BeginDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeePayoutData]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Table [dbo].[EmployeePersonalData]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Index [IX_EmployeePersonalData_EmployeeId_BeginDateTime]    Script Date: 06.11.2018 17:42:58 ******/
CREATE CLUSTERED INDEX [IX_EmployeePersonalData_EmployeeId_BeginDateTime] ON [dbo].[EmployeePersonalData]
(
	[EmployeeId] ASC,
	[BeginDateTime] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeePersonalDataHistory]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Index [PK_EmployeePersonalDataHistory]    Script Date: 06.11.2018 17:42:58 ******/
CREATE UNIQUE CLUSTERED INDEX [PK_EmployeePersonalDataHistory] ON [dbo].[EmployeePersonalDataHistory]
(
	[EmployeeId] ASC,
	[EndDateTime] ASC,
	[BeginDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EmployeeRegistrationLink]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Table [dbo].[Payment]    Script Date: 06.11.2018 17:42:58 ******/
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
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentExternalData]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Table [dbo].[PaymentShare]    Script Date: 06.11.2018 17:42:58 ******/
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
/****** Object:  Table [dbo].[Place]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[PlaceGroup]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[PpAcceptance]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[PrivacyPolicy]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[ShareScheme]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[ShareSchemeHistory]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Index [ix_ShareSchemeHistory]    Script Date: 06.11.2018 17:42:59 ******/
CREATE CLUSTERED INDEX [ix_ShareSchemeHistory] ON [dbo].[ShareSchemeHistory]
(
	[PlaceId] ASC,
	[EndDateTime] ASC,
	[BeginDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SmsMessage]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[SsGroupShare]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[SsGroupShareHistory]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[UaAcceptance]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[UserAgreement]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Table [dbo].[VerificationCode]    Script Date: 06.11.2018 17:42:59 ******/
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
/****** Object:  Index [IX_EmployeeAuth_EmployeeId]    Script Date: 06.11.2018 17:42:59 ******/
CREATE NONCLUSTERED INDEX [IX_EmployeeAuth_EmployeeId] ON [dbo].[EmployeeAuth]
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_EmployeeAuth_PermanentKey]    Script Date: 06.11.2018 17:42:59 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_EmployeeAuth_PermanentKey] ON [dbo].[EmployeeAuth]
(
	[PermanentKey] ASC
)
WHERE ([PermanentKey] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_EmployeeId]    Script Date: 06.11.2018 17:42:59 ******/
CREATE NONCLUSTERED INDEX [IX_Payment_EmployeeId] ON [dbo].[Payment]
(
	[EmployeeId] ASC,
	[PaymentDateTime] ASC
)
WHERE ([EmployeeId] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Payment_PlaceId]    Script Date: 06.11.2018 17:42:59 ******/
CREATE NONCLUSTERED INDEX [IX_Payment_PlaceId] ON [dbo].[Payment]
(
	[PlaceId] ASC,
	[PaymentDateTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_PaymentExternalData_PaymentId]    Script Date: 06.11.2018 17:42:59 ******/
CREATE NONCLUSTERED INDEX [IX_PaymentExternalData_PaymentId] ON [dbo].[PaymentExternalData]
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_PaymentShare_EmployeeId_PaymentId]    Script Date: 06.11.2018 17:42:59 ******/
CREATE NONCLUSTERED INDEX [IX_PaymentShare_EmployeeId_PaymentId] ON [dbo].[PaymentShare]
(
	[EmployeeId] ASC,
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [ix_ShareSchemeHistory2]    Script Date: 06.11.2018 17:42:59 ******/
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
ALTER DATABASE [tips] SET  READ_WRITE 
GO
