--/*SET SCHEMA 'postgre_WordMergerDB';*/

--DROP TABLE IF EXISTS public."GlobalSettings";
--DROP TABLE IF EXISTS public."ReportParameter";
--DROP TABLE IF EXISTS public."ParameterCondition";
--DROP TABLE IF EXISTS public."Parameter";
--DROP TABLE IF EXISTS public."BusinessRoleReport";
--DROP TABLE IF EXISTS public."BusinessRole";
--DROP TABLE IF EXISTS public."ReportCondition";
--DROP TABLE IF EXISTS public."Condition";
--DROP TABLE IF EXISTS public."ReportPackageEntry";
--DROP TABLE IF EXISTS public."ReportPackage";
--DROP TABLE IF EXISTS public."Integration";
--DROP TABLE IF EXISTS public."ReportFilter";
--DROP TABLE IF EXISTS public."Report";
--DROP TABLE IF EXISTS public."DataSource";
--DROP TABLE IF EXISTS public."ParagraphContent";
--DROP TABLE IF EXISTS public."Paragraph";
--DROP TABLE IF EXISTS public."DocumentContent";
--DROP TABLE IF EXISTS public."Document";
--DROP TABLE IF EXISTS public."DocumentChanges";
--DROP TABLE IF EXISTS public."FilterParentFilter";
--DROP TABLE IF EXISTS public."Filter";
--DROP TABLE IF EXISTS public."Audit";


CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

--DataSource
CREATE TABLE IF NOT EXISTS public."DataSource"
(
    datasourceid uuid NOT NULL DEFAULT uuid_generate_v4(),
    datasourcename character varying(50) COLLATE pg_catalog."default" NOT NULL,
    dataquery text COLLATE pg_catalog."default",
    keyfieldname character varying(150) COLLATE pg_catalog."default",
    foreignkeyfieldname character varying(150) COLLATE pg_catalog."default",
    assingleline boolean,
    "ParentDataSourceId" uuid,
    "position" integer,
    dataqueryxml text COLLATE pg_catalog."default",
    usequerybuilder boolean NOT NULL,
    isglobal boolean,
    createdon timestamp(3) without time zone,
    createdby text COLLATE pg_catalog."default",
    modifiedon timestamp(3) without time zone,
    modifiedby text COLLATE pg_catalog."default",
    "IsIntegrationRequest" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_DataSource" PRIMARY KEY (datasourceid),
    CONSTRAINT "FK_DataSource_DataSource" FOREIGN KEY ("ParentDataSourceId")
        REFERENCES public."DataSource" (datasourceid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."DataSource"
    OWNER to postgres;

--Parameter
CREATE TABLE IF NOT EXISTS public."Parameter"
(
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    name character varying(50) COLLATE pg_catalog."default",
    datatype character varying(50) COLLATE pg_catalog."default",
    nullable character(10) COLLATE pg_catalog."default",
    query text COLLATE pg_catalog."default",
    errormessage text COLLATE pg_catalog."default",
    testval text COLLATE pg_catalog."default",
    displayname text COLLATE pg_catalog."default",
    displayorder integer,
    isglobal boolean,
    createdon timestamp(3) without time zone,
    createdby text COLLATE pg_catalog."default",
    modifiedon timestamp(3) without time zone,
    modifiedby text COLLATE pg_catalog."default",
    CONSTRAINT "PK_Parameter" PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Parameter"
    OWNER to postgres;

--BusinessRole
CREATE TABLE IF NOT EXISTS public."BusinessRole"
(
    businessroleid uuid NOT NULL DEFAULT uuid_generate_v4(),
    businessrolecode integer NOT NULL,
    rolename character varying(150) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT PK_BusinessRole PRIMARY KEY (businessroleid)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."BusinessRole"
    OWNER to postgres;

--Document
CREATE TABLE IF NOT EXISTS public."Document"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "Name" character varying(50) COLLATE pg_catalog."default",
    "IsLocked" boolean NOT NULL DEFAULT false,
    "LockedBy" text COLLATE pg_catalog."default",
    "TrackChanges" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_Document" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Document"
    OWNER to postgres;

--Report
CREATE TABLE IF NOT EXISTS public."Report"
(
    reportid uuid NOT NULL DEFAULT uuid_generate_v4(),
    reportname character varying(50) COLLATE pg_catalog."default",
    reportpath character varying(254) COLLATE pg_catalog."default",
    defaultdatabase character varying(254) COLLATE pg_catalog."default",
    reportcode character varying(100) COLLATE pg_catalog."default",
    servername character varying(254) COLLATE pg_catalog."default",
    testid character varying(254) COLLATE pg_catalog."default",
    removeemptyregions boolean,
    securitymanagement boolean,
    replacefieldswithstatictext boolean,
    reportformat character varying(50) COLLATE pg_catalog."default",
    entityname character varying(254) COLLATE pg_catalog."default",
    reporttype character varying(50) COLLATE pg_catalog."default",
    sharepoint_dosave boolean,
    sharepoint_groupkey text COLLATE pg_catalog."default",
    sharepoint_intcode text COLLATE pg_catalog."default",
    datasourceid uuid,
    testuserid character varying(254) COLLATE pg_catalog."default",
    "IsShow" boolean DEFAULT true,
    sqlqueryfilename text COLLATE pg_catalog."default",
    subjectemail text COLLATE pg_catalog."default",
    createdon timestamp(3) without time zone,
    modifiedon timestamp(3) without time zone,
    activeon timestamp(3) without time zone,
    deactiveon timestamp(3) without time zone,
    deactivateon timestamp(3) without time zone,
    sequencenumber integer,
    createdby text COLLATE pg_catalog."default",
    modifiedby text COLLATE pg_catalog."default",
    sharepoint_relativepath character varying(254) COLLATE pg_catalog."default",
    sharepoint_fieldnameurl character varying(254) COLLATE pg_catalog."default",
    useglobal boolean,
    setdate boolean NOT NULL DEFAULT true,
    documentid uuid,
    "isCustomTemplate" boolean NOT NULL DEFAULT false,
    "IsLocked" boolean NOT NULL DEFAULT false,
    "LockedBy" text COLLATE pg_catalog."default",
    "IsChooseFormat" boolean NOT NULL DEFAULT false,
    "SpLoadType" character varying(50) COLLATE pg_catalog."default",
    "PathKeyFieldName" character varying(150) COLLATE pg_catalog."default",
    "NamePostfix" character varying(50) COLLATE pg_catalog."default",
    "IsSendRequest" boolean NOT NULL DEFAULT false,
    "IsChangeColor" boolean NOT NULL DEFAULT true,
    "SqlQueryUseCondition" text COLLATE pg_catalog."default",
    "IsDS" boolean NOT NULL DEFAULT false,
    "SqlMainReportCode" text COLLATE pg_catalog."default",
    "SqlMainRowId" text COLLATE pg_catalog."default",
    CONSTRAINT "PK_Report" PRIMARY KEY (reportid),
    CONSTRAINT "FK_Report_DataSource" FOREIGN KEY (datasourceid)
        REFERENCES public."DataSource" (datasourceid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "FK_Report_Document" FOREIGN KEY (documentid)
        REFERENCES public."Document" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Report"
    OWNER to postgres;

-- ReportPackage
CREATE TABLE IF NOT EXISTS public."ReportPackage"
(
    "ReportPackageId" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "Name" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "DisplayName" character varying(254) COLLATE pg_catalog."default" NOT NULL,
    "EntityName" character varying(254) COLLATE pg_catalog."default",
    "TestId" character varying(254) COLLATE pg_catalog."default",
    "IsShow" boolean DEFAULT true,
    "TestUserId" character(254) COLLATE pg_catalog."default",
    sequencenumber integer,
    sqlqueryfilename text COLLATE pg_catalog."default",
    "IsSetDate" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_ReportPackage" PRIMARY KEY ("ReportPackageId")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ReportPackage"
    OWNER to postgres;

--ReportPackageEntry
CREATE TABLE IF NOT EXISTS public."ReportPackageEntry"
(
    "ReportPackageEntryId" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "IsObligatory" boolean NOT NULL,
    "NumberOfCopies" integer,
    "Report_reportid" uuid NOT NULL,
    "ReportPackageId" uuid NOT NULL,
    "NumberPosition" integer NOT NULL DEFAULT 0,
    "SqlQueryCopyNumber" text COLLATE pg_catalog."default",
    CONSTRAINT "PK_ReportPackageEntry" PRIMARY KEY ("ReportPackageEntryId"),
    CONSTRAINT "FK_ReportPackageReportPackageEntry" FOREIGN KEY ("ReportPackageId")
        REFERENCES public."ReportPackage" ("ReportPackageId") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "FK_ReportReportPackageEntry" FOREIGN KEY ("Report_reportid")
        REFERENCES public."Report" (reportid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ReportPackageEntry"
    OWNER to postgres;

--Condition
CREATE TABLE IF NOT EXISTS public."Condition"
(
    conditionid uuid NOT NULL DEFAULT uuid_generate_v4(),
    conditionname character varying(50) COLLATE pg_catalog."default" NOT NULL,
    dataquery text COLLATE pg_catalog."default",
    conditionoperator character varying(50) COLLATE pg_catalog."default",
    recordcount numeric(18,0),
    errormessage character varying(250) COLLATE pg_catalog."default",
    conditiontype integer,
    "ReportPackageEntryId" uuid,
    precondition boolean,
    isglobal boolean,
    createdon timestamp(3) without time zone,
    createdby text COLLATE pg_catalog."default",
    modifiedon timestamp(3) without time zone,
    modifiedby text COLLATE pg_catalog."default",
    "OrderNo" integer,
    CONSTRAINT "PK_Condition" PRIMARY KEY (conditionid),
    CONSTRAINT "FK_ReportPackageEntryCondition" FOREIGN KEY ("ReportPackageEntryId")
        REFERENCES public."ReportPackageEntry" ("ReportPackageEntryId") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Condition"
    OWNER to postgres;

--ReportCondition
CREATE TABLE IF NOT EXISTS public."ReportCondition"
(
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    reportid uuid NOT NULL,
    conditionid uuid NOT NULL,
    CONSTRAINT reportcondition_pkey PRIMARY KEY (id),
    CONSTRAINT "FK_ReportCondition_conditionid" FOREIGN KEY (conditionid)
        REFERENCES public."Condition" (conditionid) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT "FK_ReportCondition_reportid" FOREIGN KEY (reportid)
        REFERENCES public."Report" (reportid) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ReportCondition"
    OWNER to postgres;

--ReportParameter
CREATE TABLE IF NOT EXISTS public."ReportParameter"
(
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    reportid uuid NOT NULL,
    parameterid uuid NOT NULL,
    CONSTRAINT reportparameter_pkey PRIMARY KEY (id),
    CONSTRAINT "FK_ReportParameter_parameterid" FOREIGN KEY (parameterid)
        REFERENCES public."Parameter" (id) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT "FK_ReportParameter_reportid" FOREIGN KEY (reportid)
        REFERENCES public."Report" (reportid) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ReportParameter"
    OWNER to postgres; 

--BusinessRoleReport
CREATE TABLE IF NOT EXISTS public."BusinessRoleReport"
(
    "BusinessRoleReportID" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "BusinessRoleId" uuid NOT NULL,
    "ReportId" uuid NOT NULL,
    CONSTRAINT businessrolereport_pkey PRIMARY KEY ("BusinessRoleReportID"),
    CONSTRAINT FK_BusinessRoleReport_BusinessRoleReport FOREIGN KEY ("BusinessRoleId")
        REFERENCES public."BusinessRole" (businessroleid) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT FK_BusinessRoleReport_Report FOREIGN KEY ("ReportId")
        REFERENCES public."Report" (reportid) MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."BusinessRoleReport"
    OWNER to postgres;

--GlobalSettings
CREATE TABLE IF NOT EXISTS public."GlobalSettings"
(
    "ServerName" text COLLATE pg_catalog."default",
    "DbName" text COLLATE pg_catalog."default",
    "DbVersion" text COLLATE pg_catalog."default",
    "IsRemoveHighlight" boolean NOT NULL DEFAULT false,
    "DefaultColorText" character varying(9) COLLATE pg_catalog."default",
    "GlobalDataSourcePath" text COLLATE pg_catalog."default",
    "GlobalConditionPath" text COLLATE pg_catalog."default",
    id uuid NOT NULL DEFAULT uuid_generate_v4(),
    CONSTRAINT "PK_GlobalSettings" PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."GlobalSettings"
    OWNER to postgres;

--Audit
CREATE TABLE IF NOT EXISTS public."Audit"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "ReportCode" text COLLATE pg_catalog."default",
    "Template" text COLLATE pg_catalog."default",
    "RowId" uuid,
    "CreatedOn" timestamp(3) without time zone,
    "CreatedBy" uuid,
    "Parameters" text COLLATE pg_catalog."default",
    "DocumentLink" text COLLATE pg_catalog."default",
    "BarCode" text COLLATE pg_catalog."default",
    "IntegrationDocumentId" text COLLATE pg_catalog."default",
    "IntegrationDocumentLocation" text COLLATE pg_catalog."default",
    "IntegrationExternalId" uuid,
    "SourceDataSet" text COLLATE pg_catalog."default",
    CONSTRAINT "PK_Audit" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Audit"
    OWNER to postgres;

--DocumentChanges
CREATE TABLE IF NOT EXISTS public."DocumentChanges"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "ReportCode" text COLLATE pg_catalog."default",
    "RowId" uuid,
    "Version" integer NOT NULL DEFAULT 1,
    "IsApplication" boolean NULL,
    "IsChapter" boolean NULL,
    "Order" text COLLATE pg_catalog."default",
    "Content" text COLLATE pg_catalog."default",
    CONSTRAINT "PK_DocumentChanges" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."DocumentChanges"
    OWNER to postgres;

--DocumentContent
CREATE TABLE IF NOT EXISTS public."DocumentContent"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "DocumentId" uuid NOT NULL,
    "Template" text COLLATE pg_catalog."default",
    "Data" text COLLATE pg_catalog."default",
    "Version" integer NOT NULL,
    "DefaultVersion" boolean NOT NULL,
    "CreatedOn" timestamp(3) without time zone,
    "CreatedBy" text COLLATE pg_catalog."default",    
    CONSTRAINT "PK_DocumentContent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_DocumentContent_ToDocument" FOREIGN KEY ("DocumentId")
        REFERENCES public."Document" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."DocumentContent"
    OWNER to postgres;

--Filter
CREATE TABLE IF NOT EXISTS public."Filter"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "Name" text COLLATE pg_catalog."default",
    "ParentFilterId" uuid,
    "ParentConditionOperator" text COLLATE pg_catalog."default",
    "DisplayName" text COLLATE pg_catalog."default",    
    "Order" integer,
    "Type" text COLLATE pg_catalog."default",        
    "Query" text COLLATE pg_catalog."default",    
    "VisibleQuery" text COLLATE pg_catalog."default",    
    "VisibleConditionOperator" text COLLATE pg_catalog."default",    
    "VisibleRecordCount" numeric(18,0),
    "CreatedOn" timestamp(3) without time zone,
    "CreatedBy" text COLLATE pg_catalog."default",    
    "ModifiedOn" timestamp(3) without time zone,
    "ModifiedBy" text COLLATE pg_catalog."default",    
    CONSTRAINT "PK_Filter" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Filter"
    OWNER to postgres;

--FilterParentFilter
CREATE TABLE IF NOT EXISTS public."FilterParentFilter"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "FilterId" uuid,
    "ParentFilterId" uuid,
    "Value" text COLLATE pg_catalog."default",
    CONSTRAINT "PK_FilterParentFilter" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FilterParentFilter_ToFilter" FOREIGN KEY ("FilterId")
        REFERENCES public."Filter" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "FK_FilterParentFilter_ToParentFilter" FOREIGN KEY ("ParentFilterId")
        REFERENCES public."Filter" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION

)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."FilterParentFilter"
    OWNER to postgres;


--Integration
CREATE TABLE IF NOT EXISTS public."Integration"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "ReportId" uuid,
    "SystemCode" text COLLATE pg_catalog."default",
    "SourceId" uuid,
    "Order" integer NOT NULL DEFAULT 99,
    "BeforeAction" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_Integration" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Integration_ToDataSource" FOREIGN KEY ("SourceId")
        REFERENCES public."DataSource" ("datasourceid") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "FK_Integration_ToReport" FOREIGN KEY ("ReportId")
        REFERENCES public."Report" ("reportid") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION

)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Integration"
    OWNER to postgres;

--Paragraph
CREATE TABLE IF NOT EXISTS public."Paragraph"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),    
    "Name" character varying(250) COLLATE pg_catalog."default",    
    "Numerable" boolean NULL,
    "Level" integer NULL,
    "NewPage" boolean NULL,
    "OrderNo" integer NULL,
    "Condition" text COLLATE pg_catalog."default",
    "Deleted" boolean NOT NULL DEFAULT false,
    "ReferenceName" character varying(50) COLLATE pg_catalog."default",    
    "ActiveFrom" timestamp(3) without time zone,
    "ActiveTill" timestamp(3) without time zone,
    "Tooltip" character varying(1000) COLLATE pg_catalog."default",    
    "IsFixNumeration" boolean NOT NULL DEFAULT false,
    "ParentParagraphId" uuid,
    "IsGlobal" boolean NOT NULL DEFAULT false,
    "DocumentContentId" uuid,
    "Tag" text COLLATE pg_catalog."default",
    "IsAttachment" boolean NOT NULL DEFAULT false,
    "IsCustomNumbering" boolean NOT NULL DEFAULT false,
    "StartNumberingFrom" character varying(1000) COLLATE pg_catalog."default",
    "CustomNo" character varying(1000) COLLATE pg_catalog."default",
    "CustomLevel" integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_Paragraph" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Paragraph_ToDocumentContent" FOREIGN KEY ("DocumentContentId")
        REFERENCES public."DocumentContent" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "FK_Paragraph_ToParentParagraph" FOREIGN KEY ("ParentParagraphId")
        REFERENCES public."Paragraph" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION

)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Paragraph"
    OWNER to postgres;

--ParagraphContent
CREATE TABLE IF NOT EXISTS public."ParagraphContent"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),    
    "Name" character varying(50) COLLATE pg_catalog."default",    
    "Content" text COLLATE pg_catalog."default",
    "ParagraphId" uuid,
    "Condition" text COLLATE pg_catalog."default",
    "Deleted" boolean NOT NULL DEFAULT false,
    "DefaultVersion" boolean NULL DEFAULT false,
    "ActiveFrom" timestamp(3) without time zone,
    "ActiveTill" timestamp(3) without time zone,
    "Tooltip" character varying(1000) COLLATE pg_catalog."default",
    "CreatedOn" timestamp(3) without time zone,
    "Approved" boolean NULL,
    "Comment" text COLLATE pg_catalog."default",
    CONSTRAINT "PK_ParagraphContent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ParagraphContent_ToParagraph" FOREIGN KEY ("ParagraphId")
        REFERENCES public."Paragraph" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ParagraphContent"
    OWNER to postgres;

--ParameterCondition
CREATE TABLE IF NOT EXISTS public."ParameterCondition"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),   
    "ParameterId" uuid,  
    "Name" text COLLATE pg_catalog."default",
    "Query" text COLLATE pg_catalog."default",
    "ConditionOperator" character varying(50) COLLATE pg_catalog."default",
    "RecordCount" numeric(18,0),
    CONSTRAINT "PK_ParameterCondition" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ParameterCondition_ToParameter" FOREIGN KEY ("ParameterId")
        REFERENCES public."Parameter" ("id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ParameterCondition"
    OWNER to postgres;

--ReportFilter
CREATE TABLE IF NOT EXISTS public."ReportFilter"
(
    "Id" uuid NOT NULL DEFAULT uuid_generate_v4(),
    "ReportId" uuid,
    "FilterId" uuid,
    "Value" text COLLATE pg_catalog."default",
    CONSTRAINT "PK_ReportFilter" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ReportFilter_ToFilter" FOREIGN KEY ("FilterId")
        REFERENCES public."Filter" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT "FK_ReportFilter_ToReport" FOREIGN KEY ("ReportId")
        REFERENCES public."Report" (reportid) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."ReportFilter"
    OWNER to postgres;

INSERT INTO public."GlobalSettings" ("DbVersion") VALUES ('1.0.220404')