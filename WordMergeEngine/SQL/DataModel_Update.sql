USE [Temp_WordMergerDB]
GO

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsFixNumeration' AND Object_ID = Object_ID(N'Paragraph'))
BEGIN
    ALTER TABLE Paragraph
	ADD IsFixNumeration bit NOT NULL DEFAULT(0)
	PRINT N'Столбец IsFixNumeration (Paragraph) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsFixNumeration (Paragraph) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'Comment' AND Object_ID = Object_ID(N'ParagraphContent'))
BEGIN
    ALTER TABLE ParagraphContent
	ADD Comment nvarchar(max) NULL
	PRINT N'Столбец Comment (ParagraphContent) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец Comment (ParagraphContent) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsLocked' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IsLocked bit NOT NULL DEFAULT(0)
	PRINT N'Столбец IsLocked (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsLocked (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'LockedBy' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD LockedBy nvarchar(max) NULL
	PRINT N'Столбец LockedBy (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец LockedBy (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsLocked' AND Object_ID = Object_ID(N'Document'))
BEGIN
    ALTER TABLE Document
	ADD IsLocked bit NOT NULL DEFAULT(0)
	PRINT N'Столбец IsLocked (Document) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsLocked (Document) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'LockedBy' AND Object_ID = Object_ID(N'Document'))
BEGIN
    ALTER TABLE Document
	ADD LockedBy nvarchar(max) NULL
	PRINT N'Столбец LockedBy (Document) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец LockedBy (Document) уже добавлен'
END

IF (Object_ID(N'Audit') is null)
BEGIN
CREATE TABLE [dbo].[Audit](
	[Id] [uniqueidentifier] NOT NULL,
	[ReportCode] [nvarchar](max) NULL,
	[Template] [ntext] NULL,
	[RowId] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[Parameters] [nvarchar](max) NULL,
	[DocumentLink] [nvarchar](max) NULL,
	[BarCode] [nvarchar](max) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	PRINT N'Таблица Audit успешно добавлена'
END
ELSE
BEGIN
	PRINT N'Таблица Audit уже добавлена'
END

IF (Object_ID(N'ParameterCondition') is null)
BEGIN
CREATE TABLE [dbo].[ParameterCondition](
	[Id] [uniqueidentifier] NOT NULL,
	[ParameterId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Query] [nvarchar](max) NULL,
	[ConditionOperator] [varchar](50) NULL,
	[RecordCount] [decimal](18, 0) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
ALTER TABLE [dbo].[ParameterCondition]  WITH CHECK ADD  CONSTRAINT [FK_ParameterCondition_ToParameter] FOREIGN KEY([ParameterId])
REFERENCES [dbo].[Parameter] ([id])
	PRINT N'Таблица ParameterCondition успешно добавлена'
END
ELSE
BEGIN
	PRINT N'Таблица ParameterCondition уже добавлена'
END

IF (Object_ID(N'Filter') is null)
BEGIN
CREATE TABLE [dbo].[Filter](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[ParentFilterId] [uniqueidentifier] NULL,
	[ParentConditionOperator] [nvarchar](max) NULL,
	[DisplayName] [nvarchar](max) NULL,
	[Order] [int] NULL,
	[Type] [nvarchar](max) NULL,
	[Query] [nvarchar](max) NULL,
	[VisibleQuery] [nvarchar](max) NULL,
	[VisibleConditionOperator] [nvarchar](max) NULL,
	[VisibleRecordCount] [decimal](18,0) NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [nvarchar](max) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
	PRINT N'Таблица Filter успешно добавлена'
END
ELSE
BEGIN
	PRINT N'Таблица Filter уже добавлена'
END

IF (Object_ID(N'FilterParentFilter') is null)
BEGIN
CREATE TABLE [dbo].[FilterParentFilter](
	[Id] [uniqueidentifier] NOT NULL,
	[FilterId] [uniqueidentifier] NOT NULL,
	[ParentFilterId] [uniqueidentifier] NOT NULL,
	[Value] [nvarchar](max) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
ALTER TABLE [dbo].[FilterParentFilter]  WITH CHECK ADD  CONSTRAINT [FK_FilterParentFilter_ToFilter] FOREIGN KEY([FilterId])
REFERENCES [dbo].[Filter] ([id])
ALTER TABLE [dbo].[FilterParentFilter]  WITH CHECK ADD  CONSTRAINT [FK_FilterParentFilter_ToParentFilter] FOREIGN KEY([ParentFilterId])
REFERENCES [dbo].[Filter] ([id])
	PRINT N'Таблица FilterParentFilter успешно добавлена'
END
ELSE
BEGIN
	PRINT N'Таблица FilterParentFilter уже добавлена'
END

IF (Object_ID(N'ReportFilter') is null)
BEGIN
CREATE TABLE [dbo].[ReportFilter](
	[Id] [uniqueidentifier] NOT NULL,
	[ReportId] [uniqueidentifier] NOT NULL,
	[FilterId] [uniqueidentifier] NOT NULL,
	[Value] [nvarchar](max) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
ALTER TABLE [dbo].[ReportFilter]  WITH CHECK ADD  CONSTRAINT [FK_ReportFilter_ToReport] FOREIGN KEY([ReportId])
REFERENCES [dbo].[Report] ([reportid])
ALTER TABLE [dbo].[ReportFilter]  WITH CHECK ADD  CONSTRAINT [FK_ReportFilter_ToFilter] FOREIGN KEY([FilterId])
REFERENCES [dbo].[Filter] ([id])
	PRINT N'Таблица ReportFilter успешно добавлена'
END
ELSE
BEGIN
	PRINT N'Таблица ReportFilter уже добавлена'
END

if ((select top 1 is_nullable from sys.columns c join sys.tables t on c.object_id = t.object_id where c.name = 'Deleted' and t.name = 'ParagraphContent') = 0)
BEGIN
PRINT N'Столбец Deleted (ParagraphContent) уже обновлен'
END
ELSE
BEGIN
UPDATE ParagraphContent SET Deleted = 0 where Deleted is null
ALTER TABLE ParagraphContent ALTER COLUMN Deleted bit NOT NULL
PRINT N'Столбец Deleted (ParagraphContent) успешно обновлен'
END

if ((select top 1 is_nullable from sys.columns c join sys.tables t on c.object_id = t.object_id where c.name = 'Deleted' and t.name = 'Paragraph') = 0)
BEGIN
PRINT N'Столбец Deleted (Paragraph) уже обновлен'
END
ELSE
BEGIN
UPDATE Paragraph SET Deleted = 0 where Deleted is null
ALTER TABLE Paragraph ALTER COLUMN Deleted bit NOT NULL
PRINT N'Столбец Deleted (Paragraph) успешно обновлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'ParentParagraphId' AND Object_ID = Object_ID(N'Paragraph'))
BEGIN
    ALTER TABLE Paragraph
	ADD ParentParagraphId uniqueidentifier NULL
	ALTER TABLE [dbo].[Paragraph]  WITH CHECK ADD  CONSTRAINT [FK_Paragraph_ToParentParagraph] FOREIGN KEY([ParentParagraphId])
REFERENCES [dbo].[Paragraph] ([id])
	PRINT N'Столбец ParentParagraphId (Paragraph) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец ParentParagraphId (Paragraph) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsGlobal' AND Object_ID = Object_ID(N'Paragraph'))
BEGIN
    ALTER TABLE Paragraph
	ADD IsGlobal bit NOT NULL DEFAULT(0)
	PRINT N'Столбец IsGlobal (Paragraph) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsGlobal (Paragraph) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsChooseFormat' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IsChooseFormat bit NOT NULL DEFAULT(0)
	PRINT N'Столбец IsChooseFormat (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsChooseFormat (Report) уже добавлен'
END

IF ((select top 1 CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'reportname' and c.TABLE_NAME = 'Report') = 50)
BEGIN
ALTER TABLE Report ALTER COLUMN reportname nvarchar(100)
PRINT N'Столбец reportname (Report) успешно обновлен'
END
ELSE
BEGIN 
PRINT N'Столбец reportname (Report) уже обновлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'SpLoadType' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD SpLoadType varchar(50)
	PRINT N'Столбец SpLoadType (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец SpLoadType (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'PathKeyFieldName' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD PathKeyFieldName nvarchar(150)
	PRINT N'Столбец PathKeyFieldName (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец PathKeyFieldName (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'NamePostfix' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD NamePostfix varchar(50)
	PRINT N'Столбец NamePostfix (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец NamePostfix (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'setdate' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD setdate bit NOT NULL DEFAULT(1)
	PRINT N'Столбец setdate (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец setdate (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsSetDate' AND Object_ID = Object_ID(N'ReportPackage'))
BEGIN
    ALTER TABLE ReportPackage
	ADD IsSetDate bit NOT NULL DEFAULT(1)
	PRINT N'Столбец IsSetDate (ReportPackage) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsSetDate (ReportPackage) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsIntegrationRequest' AND Object_ID = Object_ID(N'DataSource'))
BEGIN
    ALTER TABLE DataSource
	ADD IsIntegrationRequest bit NOT NULL DEFAULT(0)
	PRINT N'Столбец IsIntegrationRequest (DataSource) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsIntegrationRequest (DataSource) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsSendRequest' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IsSendRequest bit NOT NULL DEFAULT(0)
	PRINT N'Столбец IsSendRequest (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IsSendRequest (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationSourceId' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IntegrationSourceId uniqueidentifier NULL
	ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_Report_ToIntegrationSource] FOREIGN KEY([IntegrationSourceId])
REFERENCES [dbo].[DataSource] ([datasourceid])
	PRINT N'Столбец IntegrationSourceId (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IntegrationSourceId (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'ExternalSystemCode' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD ExternalSystemCode nvarchar(150)
	PRINT N'Столбец ExternalSystemCode (Report) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец ExternalSystemCode (Report) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationDocumentId' AND Object_ID = Object_ID(N'Audit'))
BEGIN
    ALTER TABLE [Audit]
	ADD IntegrationDocumentId nvarchar(max)
	PRINT N'Столбец IntegrationDocumentId (Audit) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IntegrationDocumentId (Audit) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationDocumentLocation' AND Object_ID = Object_ID(N'Audit'))
BEGIN
    ALTER TABLE [Audit]
	ADD IntegrationDocumentLocation nvarchar(max)
	PRINT N'Столбец IntegrationDocumentLocation (Audit) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IntegrationDocumentLocation (Audit) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationExternalId' AND Object_ID = Object_ID(N'Audit'))
BEGIN
    ALTER TABLE [Audit]
	ADD IntegrationExternalId uniqueidentifier
	PRINT N'Столбец IntegrationExternalId (Audit) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец IntegrationExternalId (Audit) уже добавлен'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'OrderNo' AND Object_ID = Object_ID(N'Condition'))
BEGIN
    ALTER TABLE Condition
	ADD OrderNo int
	PRINT N'Столбец OrderNo (Condition) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец OrderNo (Condition) уже добавлен'
END


IF (Object_ID(N'DocumentContent') is null)
BEGIN
CREATE TABLE [dbo].[DocumentContent](
	[Id] [uniqueidentifier] NOT NULL,
	[DocumentId] [uniqueidentifier] NOT NULL,
	[Template] [ntext] NULL,
	[Data] [ntext] NULL,
	[Version] [int] NOT NULL,
	[DefaultVersion] [bit] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](max) NULL
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
ALTER TABLE [dbo].[DocumentContent]  WITH CHECK ADD  CONSTRAINT [FK_DocumentContent_ToDocument] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Document] ([Id])

INSERT INTO [dbo].[DocumentContent]
(
	Id,
	DocumentId,
	Template,
	Data,
	Version,
	DefaultVersion,
	CreatedOn
)
SELECT 
	NEWID() AS Id,
	Id AS DocumentId,
	Template as Template,
	Data as Data,
	1 as Version,
	cast(1 as bit) as DefaultVersion,
	GETDATE() AS CreatedOn
FROM [dbo].[Document]

ALTER TABLE [dbo].[Document]
DROP COLUMN Template, Data

	PRINT N'Таблица DocumentContent успешно добавлена'
END
ELSE
BEGIN
	PRINT N'Таблица DocumentContent уже добавлена'
END

IF EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'DocumentId' AND Object_ID = Object_ID(N'Paragraph'))
BEGIN

ALTER TABLE [dbo].[Paragraph]
ADD DocumentContentId uniqueidentifier 
ALTER TABLE [dbo].[Paragraph]  WITH CHECK ADD  CONSTRAINT [FK_Paragraph_ToDocumentContent] FOREIGN KEY([DocumentContentId])
REFERENCES [dbo].[DocumentContent] ([Id])

UPDATE [dbo].[Paragraph] 
SET DocumentContentId = dc.Id
FROM [dbo].[Paragraph] p
JOIN [dbo].[DocumentContent] dc ON dc.DocumentId = p.DocumentId

ALTER TABLE [dbo].[Paragraph]
DROP CONSTRAINT FK_Paragraph_ToDocument
ALTER TABLE [dbo].[Paragraph]
DROP COLUMN DocumentId

	PRINT N'Связь Paragraph и Document перенастроена на Paragraph и DocumentContent'
END
ELSE
BEGIN
	PRINT N'Связь Paragraph и Document уже была перенастроена'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'TrackChanges' AND Object_ID = Object_ID(N'Document'))
BEGIN
    ALTER TABLE [Document]
	ADD TrackChanges bit NOT NULL DEFAULT(0)
	PRINT N'Столбец TrackChanges (Document) успешно добавлен'
END
ELSE
BEGIN
	PRINT N'Столбец TrackChanges (Document) уже добавлен'
END