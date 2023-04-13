USE [Temp_WordMergerDB]
GO

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'DbVersion' AND Object_ID = Object_ID(N'GlobalSettings'))
BEGIN
    ALTER TABLE GlobalSettings
	ADD DbVersion nvarchar(max) NULL
	PRINT N'Столбец DbVersion (GlobalSettings) успешно добавлен'
END

declare @newVersion nvarchar(max) = '1.0.0'
declare @currentVersion nvarchar(max)
declare @versionSql nvarchar(max) = 'select top 1 @currentVersion = DbVersion from GlobalSettings'
declare @paramDef nvarchar(max) = '@currentVersion nvarchar(max) OUTPUT'
exec sp_executesql @versionSql, @paramDef, @currentVersion=@currentVersion OUTPUT

IF (@currentVersion is null or @newVersion > @currentVersion) 
BEGIN
	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'Tag' AND Object_ID = Object_ID(N'Paragraph'))
	BEGIN
		ALTER TABLE Paragraph
		ADD Tag nvarchar(max) NULL
		PRINT N'Столбец Tag (Paragraph) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец Tag (Paragraph) уже добавлен'
	END

	IF (Object_ID(N'Integration') is null)
	BEGIN
	CREATE TABLE [dbo].[Integration](
		[Id] [uniqueidentifier] NOT NULL,
		[ReportId] [uniqueidentifier] NOT NULL,
		[SystemCode] [nvarchar](max) NULL,
		[SourceId] [uniqueidentifier] NULL,
		[Order] [int] NOT NULL DEFAULT(99),
		[BeforeAction] [bit] NOT NULL DEFAULT(0)
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	ALTER TABLE [dbo].[Integration]  WITH CHECK ADD  CONSTRAINT [FK_Integration_ToReport] FOREIGN KEY([ReportId])
	REFERENCES [dbo].[Report] ([reportid])
	ALTER TABLE [dbo].[Integration]  WITH CHECK ADD  CONSTRAINT [FK_Integration_ToDataSource] FOREIGN KEY([SourceId])
	REFERENCES [dbo].[DataSource] ([datasourceid])

	declare @sql nvarchar(max) = 'INSERT INTO [dbo].[Integration] 
	(
		Id,
		ReportId,
		SystemCode,
		SourceId,
		[Order],
		BeforeAction
	)
	SELECT 
		NEWID() AS Id,
		reportid as ReportId,
		ExternalSystemCode as SystemCode,
		IntegrationSourceId as SourceId,
		1 as [Order],
		1 as BeforeAction
	FROM
		Report
	WHERE IsSendRequest = 1'

	exec sp_executesql @sql 

	ALTER TABLE [dbo].[Report]
		DROP CONSTRAINT FK_Report_ToIntegrationSource

	ALTER TABLE [dbo].[Report]
	DROP COLUMN ExternalSystemCode, IntegrationSourceId

		PRINT N'Таблица Integration успешно добавлена'
	END
	ELSE
	BEGIN
		PRINT N'Таблица Integration уже добавлена'
	END

	IF (Object_ID(N'DocumentChanges') is null)
	BEGIN
		CREATE TABLE [dbo].[DocumentChanges](
		[Id] [uniqueidentifier] NOT NULL,	
		[ReportCode] [nvarchar](max) NULL,
		[RowId] [uniqueidentifier] NULL,	
		[Version] [int] NOT NULL DEFAULT(1),
		[IsApplication] [bit] NULL,
		[IsChapter] [bit] NULL,
		[Order] [nvarchar](max) NULL,
		[Content] [nvarchar](max) NULL,
	
	PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
		PRINT N'Таблица DocumentChanges успешно добавлена'
	END
	ELSE
	BEGIN
		PRINT N'Таблица DocumentChanges уже добавлена'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'IsAttachment' AND Object_ID = Object_ID(N'Paragraph'))
	BEGIN
		ALTER TABLE Paragraph
		ADD IsAttachment bit NOT NULL DEFAULT(0)
		PRINT N'Столбец IsAttachment (Paragraph) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец IsAttachment (Paragraph) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'IsDS' AND Object_ID = Object_ID(N'Report'))
	BEGIN
		ALTER TABLE Report
		ADD IsDS bit NOT NULL DEFAULT(0)
		PRINT N'Столбец IsDS (Report) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец IsDS (Report) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'SqlMainReportCode' AND Object_ID = Object_ID(N'Report'))
	BEGIN
		ALTER TABLE Report
		ADD SqlMainReportCode [nvarchar](max) NULL
		PRINT N'Столбец SqlMainReportCode (Report) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец SqlMainReportCode (Report) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'SqlMainRowId' AND Object_ID = Object_ID(N'Report'))
	BEGIN
		ALTER TABLE Report
		ADD SqlMainRowId [nvarchar](max) NULL
		PRINT N'Столбец SqlMainRowId (Report) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец SqlMainRowId (Report) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'SourceDataSet' AND Object_ID = Object_ID(N'Audit'))
	BEGIN
		ALTER TABLE Audit
		ADD SourceDataSet [nvarchar](max) NULL
		PRINT N'Столбец SourceDataSet (Audit) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец SourceDataSet (Audit) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'IsRemoveHighlight' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		ADD IsRemoveHighlight bit NOT NULL DEFAULT(0)
		PRINT N'Столбец IsRemoveHighlight (GlobalSettings) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец IsRemoveHighlight (GlobalSettings) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'DefaultColorText' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		ADD DefaultColorText nvarchar(9) NULL
		PRINT N'Столбец DefaultColorText (GlobalSettings) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец DefaultColorText (GlobalSettings) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'IsChangeColor' AND Object_ID = Object_ID(N'Report'))
	BEGIN
		ALTER TABLE Report
		ADD IsChangeColor bit NOT NULL DEFAULT(1)
		PRINT N'Столбец IsChangeColor (Report) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец IsChangeColor (Report) уже добавлен'
	END

	IF ((select top 1 CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'errormessage' and c.TABLE_NAME = 'Condition') < 5000)
	BEGIN
	ALTER TABLE Condition ALTER COLUMN errormessage varchar(5000)
	PRINT N'Столбец errormessage (Condition) успешно обновлен'
	END
	ELSE
	BEGIN 
	PRINT N'Столбец errormessage (Condition) уже обновлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'ServerName' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		ADD ServerName nvarchar(max) NULL
		PRINT N'Столбец ServerName (GlobalSettings) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец ServerName (GlobalSettings) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'DbName' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		ADD DbName nvarchar(max) NULL
		PRINT N'Столбец DbName (GlobalSettings) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец DbName (GlobalSettings) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'GlobalDataSourcePath' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		ADD GlobalDataSourcePath nvarchar(max) NULL
		PRINT N'Столбец GlobalDataSourcePath (GlobalSettings) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец GlobalDataSourcePath (GlobalSettings) уже добавлен'
	END

	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'GlobalConditionPath' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		ADD GlobalConditionPath nvarchar(max) NULL
		PRINT N'Столбец GlobalConditionPath (GlobalSettings) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец GlobalConditionPath (GlobalSettings) уже добавлен'
	END

	IF EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'CrmServer' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		DROP Column CrmServer 
		PRINT N'Столбец CrmServer (GlobalSettings) успешно удален'
	END
	ELSE
	BEGIN
		PRINT N'Столбец CrmServer (GlobalSettings) уже удален'
	END

	IF EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'CrmOrganization' AND Object_ID = Object_ID(N'GlobalSettings'))
	BEGIN
		ALTER TABLE GlobalSettings
		DROP Column CrmOrganization 
		PRINT N'Столбец CrmOrganization (GlobalSettings) успешно удален'
	END
	ELSE
	BEGIN
		PRINT N'Столбец CrmOrganization (GlobalSettings) уже удален'
	END
END

set @newVersion = '1.0.220404'

IF (@currentVersion is null or @newVersion > @currentVersion) 
BEGIN
	IF NOT EXISTS(SELECT * FROM sys.columns 
		WHERE Name = N'SqlQueryUseCondition' AND Object_ID = Object_ID(N'Report'))
	BEGIN
		ALTER TABLE Report
		ADD SqlQueryUseCondition nvarchar(max) NULL
		PRINT N'Столбец SqlQueryUseCondition (Report) успешно добавлен'
	END
	ELSE
	BEGIN
		PRINT N'Столбец SqlQueryUseCondition (Report) уже добавлен'
	END
END

set @newVersion = '1.0.220920'

IF (@currentVersion is null or @newVersion > @currentVersion) 
BEGIN
	IF ((select top 1 DATA_TYPE from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'Template' and c.TABLE_NAME = 'Audit') = 'ntext')
	BEGIN
	ALTER TABLE Audit ALTER COLUMN Template nvarchar(max)
	PRINT N'Столбец Template (Audit) успешно обновлен'
	END
	ELSE
	BEGIN 
	PRINT N'Столбец Template (Audit) уже обновлен'
	END
	IF ((select top 1 DATA_TYPE from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'Template' and c.TABLE_NAME = 'DocumentContent') = 'ntext')
	BEGIN
	ALTER TABLE DocumentContent ALTER COLUMN Template nvarchar(max)
	PRINT N'Столбец Template (DocumentContent) успешно обновлен'
	END
	ELSE
	BEGIN 
	PRINT N'Столбец Template (DocumentContent) уже обновлен'
	END
	IF ((select top 1 DATA_TYPE from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'Data' and c.TABLE_NAME = 'DocumentContent') = 'ntext')
	BEGIN
	ALTER TABLE DocumentContent ALTER COLUMN Data nvarchar(max)
	PRINT N'Столбец Data (DocumentContent) успешно обновлен'
	END
	ELSE
	BEGIN 
	PRINT N'Столбец Data (DocumentContent) уже обновлен'
	END
	IF ((select top 1 DATA_TYPE from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'Condition' and c.TABLE_NAME = 'Paragraph') = 'ntext')
	BEGIN
	ALTER TABLE Paragraph ALTER COLUMN Condition nvarchar(max)
	PRINT N'Столбец Condition (Paragraph) успешно обновлен'
	END
	ELSE
	BEGIN 
	PRINT N'Столбец Condition (Paragraph) уже обновлен'
	END
	IF ((select top 1 DATA_TYPE from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'Content' and c.TABLE_NAME = 'ParagraphContent') = 'ntext')
	BEGIN
	ALTER TABLE ParagraphContent ALTER COLUMN Content nvarchar(max)
	PRINT N'Столбец Content (ParagraphContent) успешно обновлен'
	END
	ELSE
	BEGIN 
	PRINT N'Столбец Content (ParagraphContent) уже обновлен'
	END
	IF ((select top 1 DATA_TYPE from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'Condition' and c.TABLE_NAME = 'ParagraphContent') = 'ntext')
	BEGIN
	ALTER TABLE ParagraphContent ALTER COLUMN Condition nvarchar(max)
	PRINT N'Столбец Condition (ParagraphContent) успешно обновлен'
	END
	ELSE
	BEGIN 
	PRINT N'Столбец Condition (ParagraphContent) уже обновлен'
	END
END

--всегда оставлять в конце--
IF (@currentVersion is null or @newVersion > @currentVersion) 
BEGIN
declare @updateVersionSql nvarchar(max) = 'UPDATE GlobalSettings SET DbVersion = @newVersion'
declare @paramDef2 nvarchar(max) = '@newVersion nvarchar(max)'
exec sp_executesql @updateVersionSql, @paramDef2, @newVersion = @newVersion
PRINT N'Версия БД (GlobalSettings) успешно обновлена до ' + @newVersion
END
ELSE
BEGIN
PRINT N'Версия БД (GlobalSettings) имеет такую же или более актуальную версию'
END