USE [Temp_WordMergerDB]
GO

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsFixNumeration' AND Object_ID = Object_ID(N'Paragraph'))
BEGIN
    ALTER TABLE Paragraph
	ADD IsFixNumeration bit NOT NULL DEFAULT(0)
	PRINT N'������� IsFixNumeration (Paragraph) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsFixNumeration (Paragraph) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'Comment' AND Object_ID = Object_ID(N'ParagraphContent'))
BEGIN
    ALTER TABLE ParagraphContent
	ADD Comment nvarchar(max) NULL
	PRINT N'������� Comment (ParagraphContent) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� Comment (ParagraphContent) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsLocked' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IsLocked bit NOT NULL DEFAULT(0)
	PRINT N'������� IsLocked (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsLocked (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'LockedBy' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD LockedBy nvarchar(max) NULL
	PRINT N'������� LockedBy (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� LockedBy (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsLocked' AND Object_ID = Object_ID(N'Document'))
BEGIN
    ALTER TABLE Document
	ADD IsLocked bit NOT NULL DEFAULT(0)
	PRINT N'������� IsLocked (Document) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsLocked (Document) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'LockedBy' AND Object_ID = Object_ID(N'Document'))
BEGIN
    ALTER TABLE Document
	ADD LockedBy nvarchar(max) NULL
	PRINT N'������� LockedBy (Document) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� LockedBy (Document) ��� ��������'
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
	PRINT N'������� Audit ������� ���������'
END
ELSE
BEGIN
	PRINT N'������� Audit ��� ���������'
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
	PRINT N'������� ParameterCondition ������� ���������'
END
ELSE
BEGIN
	PRINT N'������� ParameterCondition ��� ���������'
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
	PRINT N'������� Filter ������� ���������'
END
ELSE
BEGIN
	PRINT N'������� Filter ��� ���������'
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
	PRINT N'������� FilterParentFilter ������� ���������'
END
ELSE
BEGIN
	PRINT N'������� FilterParentFilter ��� ���������'
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
	PRINT N'������� ReportFilter ������� ���������'
END
ELSE
BEGIN
	PRINT N'������� ReportFilter ��� ���������'
END

if ((select top 1 is_nullable from sys.columns c join sys.tables t on c.object_id = t.object_id where c.name = 'Deleted' and t.name = 'ParagraphContent') = 0)
BEGIN
PRINT N'������� Deleted (ParagraphContent) ��� ��������'
END
ELSE
BEGIN
UPDATE ParagraphContent SET Deleted = 0 where Deleted is null
ALTER TABLE ParagraphContent ALTER COLUMN Deleted bit NOT NULL
PRINT N'������� Deleted (ParagraphContent) ������� ��������'
END

if ((select top 1 is_nullable from sys.columns c join sys.tables t on c.object_id = t.object_id where c.name = 'Deleted' and t.name = 'Paragraph') = 0)
BEGIN
PRINT N'������� Deleted (Paragraph) ��� ��������'
END
ELSE
BEGIN
UPDATE Paragraph SET Deleted = 0 where Deleted is null
ALTER TABLE Paragraph ALTER COLUMN Deleted bit NOT NULL
PRINT N'������� Deleted (Paragraph) ������� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'ParentParagraphId' AND Object_ID = Object_ID(N'Paragraph'))
BEGIN
    ALTER TABLE Paragraph
	ADD ParentParagraphId uniqueidentifier NULL
	ALTER TABLE [dbo].[Paragraph]  WITH CHECK ADD  CONSTRAINT [FK_Paragraph_ToParentParagraph] FOREIGN KEY([ParentParagraphId])
REFERENCES [dbo].[Paragraph] ([id])
	PRINT N'������� ParentParagraphId (Paragraph) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� ParentParagraphId (Paragraph) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsGlobal' AND Object_ID = Object_ID(N'Paragraph'))
BEGIN
    ALTER TABLE Paragraph
	ADD IsGlobal bit NOT NULL DEFAULT(0)
	PRINT N'������� IsGlobal (Paragraph) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsGlobal (Paragraph) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsChooseFormat' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IsChooseFormat bit NOT NULL DEFAULT(0)
	PRINT N'������� IsChooseFormat (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsChooseFormat (Report) ��� ��������'
END

IF ((select top 1 CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.columns c where c.COLUMN_NAME = 'reportname' and c.TABLE_NAME = 'Report') = 50)
BEGIN
ALTER TABLE Report ALTER COLUMN reportname nvarchar(100)
PRINT N'������� reportname (Report) ������� ��������'
END
ELSE
BEGIN 
PRINT N'������� reportname (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'SpLoadType' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD SpLoadType varchar(50)
	PRINT N'������� SpLoadType (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� SpLoadType (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'PathKeyFieldName' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD PathKeyFieldName nvarchar(150)
	PRINT N'������� PathKeyFieldName (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� PathKeyFieldName (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'NamePostfix' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD NamePostfix varchar(50)
	PRINT N'������� NamePostfix (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� NamePostfix (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'setdate' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD setdate bit NOT NULL DEFAULT(1)
	PRINT N'������� setdate (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� setdate (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsSetDate' AND Object_ID = Object_ID(N'ReportPackage'))
BEGIN
    ALTER TABLE ReportPackage
	ADD IsSetDate bit NOT NULL DEFAULT(1)
	PRINT N'������� IsSetDate (ReportPackage) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsSetDate (ReportPackage) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsIntegrationRequest' AND Object_ID = Object_ID(N'DataSource'))
BEGIN
    ALTER TABLE DataSource
	ADD IsIntegrationRequest bit NOT NULL DEFAULT(0)
	PRINT N'������� IsIntegrationRequest (DataSource) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsIntegrationRequest (DataSource) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IsSendRequest' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IsSendRequest bit NOT NULL DEFAULT(0)
	PRINT N'������� IsSendRequest (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IsSendRequest (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationSourceId' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD IntegrationSourceId uniqueidentifier NULL
	ALTER TABLE [dbo].[Report]  WITH CHECK ADD  CONSTRAINT [FK_Report_ToIntegrationSource] FOREIGN KEY([IntegrationSourceId])
REFERENCES [dbo].[DataSource] ([datasourceid])
	PRINT N'������� IntegrationSourceId (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IntegrationSourceId (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'ExternalSystemCode' AND Object_ID = Object_ID(N'Report'))
BEGIN
    ALTER TABLE Report
	ADD ExternalSystemCode nvarchar(150)
	PRINT N'������� ExternalSystemCode (Report) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� ExternalSystemCode (Report) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationDocumentId' AND Object_ID = Object_ID(N'Audit'))
BEGIN
    ALTER TABLE [Audit]
	ADD IntegrationDocumentId nvarchar(max)
	PRINT N'������� IntegrationDocumentId (Audit) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IntegrationDocumentId (Audit) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationDocumentLocation' AND Object_ID = Object_ID(N'Audit'))
BEGIN
    ALTER TABLE [Audit]
	ADD IntegrationDocumentLocation nvarchar(max)
	PRINT N'������� IntegrationDocumentLocation (Audit) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IntegrationDocumentLocation (Audit) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'IntegrationExternalId' AND Object_ID = Object_ID(N'Audit'))
BEGIN
    ALTER TABLE [Audit]
	ADD IntegrationExternalId uniqueidentifier
	PRINT N'������� IntegrationExternalId (Audit) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� IntegrationExternalId (Audit) ��� ��������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'OrderNo' AND Object_ID = Object_ID(N'Condition'))
BEGIN
    ALTER TABLE Condition
	ADD OrderNo int
	PRINT N'������� OrderNo (Condition) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� OrderNo (Condition) ��� ��������'
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

	PRINT N'������� DocumentContent ������� ���������'
END
ELSE
BEGIN
	PRINT N'������� DocumentContent ��� ���������'
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

	PRINT N'����� Paragraph � Document ������������� �� Paragraph � DocumentContent'
END
ELSE
BEGIN
	PRINT N'����� Paragraph � Document ��� ���� �������������'
END

IF NOT EXISTS(SELECT * FROM sys.columns 
	WHERE Name = N'TrackChanges' AND Object_ID = Object_ID(N'Document'))
BEGIN
    ALTER TABLE [Document]
	ADD TrackChanges bit NOT NULL DEFAULT(0)
	PRINT N'������� TrackChanges (Document) ������� ��������'
END
ELSE
BEGIN
	PRINT N'������� TrackChanges (Document) ��� ��������'
END