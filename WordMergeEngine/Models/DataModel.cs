using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WordMergeEngine.Models;

public partial class DataModel : DbContext
{
    public DataModel() : base("name=DataModel")
    {
    }

    public DataModel(DbContextOptions<DataModel> options)
        : base(options)
    {
    }

    public virtual DbSet<Audit> Audits { get; set; }

    public virtual DbSet<BusinessRole> BusinessRoles { get; set; }

    public virtual DbSet<BusinessRoleReport> BusinessRoleReports { get; set; }

    public virtual DbSet<Condition> Conditions { get; set; }

    public virtual DbSet<DataSource> DataSources { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentChange> DocumentChanges { get; set; }

    public virtual DbSet<DocumentContent> DocumentContents { get; set; }

    public virtual DbSet<Filter> Filters { get; set; }

    public virtual DbSet<FilterParentFilter> FilterParentFilters { get; set; }

    public virtual DbSet<GlobalSetting> GlobalSettings { get; set; }

    public virtual DbSet<Integration> Integrations { get; set; }

    public virtual DbSet<Paragraph> Paragraphs { get; set; }

    public virtual DbSet<ParagraphContent> ParagraphContents { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<ParameterCondition> ParameterConditions { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<ReportCondition> ReportConditions { get; set; }

    public virtual DbSet<ReportFilter> ReportFilters { get; set; }

    public virtual DbSet<ReportPackage> ReportPackages { get; set; }

    public virtual DbSet<ReportPackageEntry> ReportPackageEntries { get; set; }

    public virtual DbSet<ReportParameter> ReportParameters { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=10.1.1.67;Database=postgre_AT_WordMergerDB;Username=wm_user;Password=zjAZv6m6AjdIGvQ9");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Audit>(entity =>
        {
            entity.ToTable("Audit");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.CreatedOn).HasColumnType("timestamp(3) without time zone");
        });

        modelBuilder.Entity<BusinessRole>(entity =>
        {
            entity.HasKey(e => e.Businessroleid).HasName("pk_businessrole");

            entity.ToTable("BusinessRole");

            entity.Property(e => e.Businessroleid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("businessroleid");
            entity.Property(e => e.Businessrolecode).HasColumnName("businessrolecode");
            entity.Property(e => e.Rolename)
                .HasMaxLength(150)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<BusinessRoleReport>(entity =>
        {
            entity.HasKey(e => e.BusinessRoleReportId).HasName("businessrolereport_pkey");

            entity.ToTable("BusinessRoleReport");

            entity.Property(e => e.BusinessRoleReportId)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("BusinessRoleReportID");

            entity.HasOne(d => d.BusinessRole).WithMany(p => p.BusinessRoleReports)
                .HasForeignKey(d => d.BusinessRoleId)
                .HasConstraintName("fk_businessrolereport_businessrolereport");

            entity.HasOne(d => d.Report).WithMany(p => p.BusinessRoleReports)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("fk_businessrolereport_report");
        });

        modelBuilder.Entity<Condition>(entity =>
        {
            entity.ToTable("Condition");

            entity.Property(e => e.Conditionid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("conditionid");
            entity.Property(e => e.Conditionname)
                .HasMaxLength(50)
                .HasColumnName("conditionname");
            entity.Property(e => e.Conditionoperator)
                .HasMaxLength(50)
                .HasColumnName("conditionoperator");
            entity.Property(e => e.Conditiontype).HasColumnName("conditiontype");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Dataquery).HasColumnName("dataquery");
            entity.Property(e => e.Errormessage)
                .HasMaxLength(250)
                .HasColumnName("errormessage");
            entity.Property(e => e.Isglobal).HasColumnName("isglobal");
            entity.Property(e => e.Modifiedby).HasColumnName("modifiedby");
            entity.Property(e => e.Modifiedon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("modifiedon");
            entity.Property(e => e.Precondition).HasColumnName("precondition");
            entity.Property(e => e.Recordcount)
                .HasPrecision(18)
                .HasColumnName("recordcount");

            entity.HasOne(d => d.ReportPackageEntry).WithMany(p => p.Conditions)
                .HasForeignKey(d => d.ReportPackageEntryId)
                .HasConstraintName("FK_ReportPackageEntryCondition");
        });

        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.ToTable("DataSource");

            entity.Property(e => e.Datasourceid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("datasourceid");
            entity.Property(e => e.Assingleline).HasColumnName("assingleline");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Dataquery).HasColumnName("dataquery");
            entity.Property(e => e.Dataqueryxml).HasColumnName("dataqueryxml");
            entity.Property(e => e.Datasourcename)
                .HasMaxLength(50)
                .HasColumnName("datasourcename");
            entity.Property(e => e.Foreignkeyfieldname)
                .HasMaxLength(150)
                .HasColumnName("foreignkeyfieldname");
            entity.Property(e => e.Isglobal).HasColumnName("isglobal");
            entity.Property(e => e.Keyfieldname)
                .HasMaxLength(150)
                .HasColumnName("keyfieldname");
            entity.Property(e => e.Modifiedby).HasColumnName("modifiedby");
            entity.Property(e => e.Modifiedon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("modifiedon");
            entity.Property(e => e.Position).HasColumnName("position");
            entity.Property(e => e.Usequerybuilder).HasColumnName("usequerybuilder");

            entity.HasOne(d => d.ParentDataSource).WithMany(p => p.InverseParentDataSource)
                .HasForeignKey(d => d.ParentDataSourceId)
                .HasConstraintName("FK_DataSource_DataSource");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<DocumentChange>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Version).HasDefaultValueSql("1");
        });

        modelBuilder.Entity<DocumentContent>(entity =>
        {
            entity.ToTable("DocumentContent");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.CreatedOn).HasColumnType("timestamp(3) without time zone");

            entity.HasOne(d => d.Document).WithMany(p => p.DocumentContents)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentContent_ToDocument");
        });

        modelBuilder.Entity<Filter>(entity =>
        {
            entity.ToTable("Filter");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.CreatedOn).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ModifiedOn).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.VisibleRecordCount).HasPrecision(18);
        });

        modelBuilder.Entity<FilterParentFilter>(entity =>
        {
            entity.ToTable("FilterParentFilter");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");

            entity.HasOne(d => d.Filter).WithMany(p => p.FilterParentFilterFilters)
                .HasForeignKey(d => d.FilterId)
                .HasConstraintName("FK_FilterParentFilter_ToFilter");

            entity.HasOne(d => d.ParentFilter).WithMany(p => p.FilterParentFilterParentFilters)
                .HasForeignKey(d => d.ParentFilterId)
                .HasConstraintName("FK_FilterParentFilter_ToParentFilter");
        });

        modelBuilder.Entity<GlobalSetting>(entity =>
        {
            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.DefaultColorText).HasMaxLength(9);
        });

        modelBuilder.Entity<Integration>(entity =>
        {
            entity.ToTable("Integration");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.Order).HasDefaultValueSql("99");

            entity.HasOne(d => d.Report).WithMany(p => p.Integrations)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK_Integration_ToReport");

            entity.HasOne(d => d.Source).WithMany(p => p.Integrations)
                .HasForeignKey(d => d.SourceId)
                .HasConstraintName("FK_Integration_ToDataSource");
        });

        modelBuilder.Entity<Paragraph>(entity =>
        {
            entity.ToTable("Paragraph");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ActiveFrom).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ActiveTill).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CustomNo).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.ReferenceName).HasMaxLength(50);
            entity.Property(e => e.StartNumberingFrom).HasMaxLength(1000);
            entity.Property(e => e.Tooltip).HasMaxLength(1000);

            entity.HasOne(d => d.DocumentContent).WithMany(p => p.Paragraphs)
                .HasForeignKey(d => d.DocumentContentId)
                .HasConstraintName("FK_Paragraph_ToDocumentContent");

            entity.HasOne(d => d.ParentParagraph).WithMany(p => p.InverseParentParagraph)
                .HasForeignKey(d => d.ParentParagraphId)
                .HasConstraintName("FK_Paragraph_ToParentParagraph");
        });

        modelBuilder.Entity<ParagraphContent>(entity =>
        {
            entity.ToTable("ParagraphContent");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ActiveFrom).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.ActiveTill).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.CreatedOn).HasColumnType("timestamp(3) without time zone");
            entity.Property(e => e.DefaultVersion).HasDefaultValueSql("false");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Tooltip).HasMaxLength(1000);

            entity.HasOne(d => d.Paragraph).WithMany(p => p.ParagraphContents)
                .HasForeignKey(d => d.ParagraphId)
                .HasConstraintName("FK_ParagraphContent_ToParagraph");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.ToTable("Parameter");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Datatype)
                .HasMaxLength(50)
                .HasColumnName("datatype");
            entity.Property(e => e.Displayname).HasColumnName("displayname");
            entity.Property(e => e.Displayorder).HasColumnName("displayorder");
            entity.Property(e => e.Errormessage).HasColumnName("errormessage");
            entity.Property(e => e.Isglobal).HasColumnName("isglobal");
            entity.Property(e => e.Modifiedby).HasColumnName("modifiedby");
            entity.Property(e => e.Modifiedon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("modifiedon");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Nullable)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("nullable");
            entity.Property(e => e.Query).HasColumnName("query");
            entity.Property(e => e.Testval).HasColumnName("testval");
        });

        modelBuilder.Entity<ParameterCondition>(entity =>
        {
            entity.ToTable("ParameterCondition");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ConditionOperator).HasMaxLength(50);
            entity.Property(e => e.RecordCount).HasPrecision(18);

            entity.HasOne(d => d.Parameter).WithMany(p => p.ParameterConditions)
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK_ParameterCondition_ToParameter");
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Report");

            entity.Property(e => e.Reportid)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("reportid");
            entity.Property(e => e.Activeon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("activeon");
            entity.Property(e => e.Createdby).HasColumnName("createdby");
            entity.Property(e => e.Createdon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.Datasourceid).HasColumnName("datasourceid");
            entity.Property(e => e.Deactivateon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("deactivateon");
            entity.Property(e => e.Deactiveon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("deactiveon");
            entity.Property(e => e.Defaultdatabase)
                .HasMaxLength(254)
                .HasColumnName("defaultdatabase");
            entity.Property(e => e.Documentid).HasColumnName("documentid");
            entity.Property(e => e.Entityname)
                .HasMaxLength(254)
                .HasColumnName("entityname");
            entity.Property(e => e.IsChangeColor)
                .IsRequired()
                .HasDefaultValueSql("true");
            entity.Property(e => e.IsCustomTemplate).HasColumnName("isCustomTemplate");
            entity.Property(e => e.IsDs).HasColumnName("IsDS");
            entity.Property(e => e.IsShow).HasDefaultValueSql("true");
            entity.Property(e => e.Modifiedby).HasColumnName("modifiedby");
            entity.Property(e => e.Modifiedon)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("modifiedon");
            entity.Property(e => e.NamePostfix).HasMaxLength(50);
            entity.Property(e => e.PathKeyFieldName).HasMaxLength(150);
            entity.Property(e => e.Removeemptyregions).HasColumnName("removeemptyregions");
            entity.Property(e => e.Replacefieldswithstatictext).HasColumnName("replacefieldswithstatictext");
            entity.Property(e => e.Reportcode)
                .HasMaxLength(100)
                .HasColumnName("reportcode");
            entity.Property(e => e.Reportformat)
                .HasMaxLength(50)
                .HasColumnName("reportformat");
            entity.Property(e => e.Reportname)
                .HasMaxLength(50)
                .HasColumnName("reportname");
            entity.Property(e => e.Reportpath)
                .HasMaxLength(254)
                .HasColumnName("reportpath");
            entity.Property(e => e.Reporttype)
                .HasMaxLength(50)
                .HasColumnName("reporttype");
            entity.Property(e => e.Securitymanagement).HasColumnName("securitymanagement");
            entity.Property(e => e.Sequencenumber).HasColumnName("sequencenumber");
            entity.Property(e => e.Servername)
                .HasMaxLength(254)
                .HasColumnName("servername");
            entity.Property(e => e.Setdate)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("setdate");
            entity.Property(e => e.SharepointDosave).HasColumnName("sharepoint_dosave");
            entity.Property(e => e.SharepointFieldnameurl)
                .HasMaxLength(254)
                .HasColumnName("sharepoint_fieldnameurl");
            entity.Property(e => e.SharepointGroupkey).HasColumnName("sharepoint_groupkey");
            entity.Property(e => e.SharepointIntcode).HasColumnName("sharepoint_intcode");
            entity.Property(e => e.SharepointRelativepath)
                .HasMaxLength(254)
                .HasColumnName("sharepoint_relativepath");
            entity.Property(e => e.SpLoadType).HasMaxLength(50);
            entity.Property(e => e.Sqlqueryfilename).HasColumnName("sqlqueryfilename");
            entity.Property(e => e.Subjectemail).HasColumnName("subjectemail");
            entity.Property(e => e.Testid)
                .HasMaxLength(254)
                .HasColumnName("testid");
            entity.Property(e => e.Testuserid)
                .HasMaxLength(254)
                .HasColumnName("testuserid");
            entity.Property(e => e.Useglobal).HasColumnName("useglobal");

            entity.HasOne(d => d.Datasource).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Datasourceid)
                .HasConstraintName("FK_Report_DataSource");

            entity.HasOne(d => d.Document).WithMany(p => p.Reports)
                .HasForeignKey(d => d.Documentid)
                .HasConstraintName("FK_Report_Document");
        });

        modelBuilder.Entity<ReportCondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reportcondition_pkey");

            entity.ToTable("ReportCondition");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Conditionid).HasColumnName("conditionid");
            entity.Property(e => e.Reportid).HasColumnName("reportid");

            entity.HasOne(d => d.Condition).WithMany(p => p.ReportConditions)
                .HasForeignKey(d => d.Conditionid)
                .HasConstraintName("FK_ReportCondition_conditionid");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportConditions)
                .HasForeignKey(d => d.Reportid)
                .HasConstraintName("FK_ReportCondition_reportid");
        });

        modelBuilder.Entity<ReportFilter>(entity =>
        {
            entity.ToTable("ReportFilter");

            entity.Property(e => e.Id).HasDefaultValueSql("uuid_generate_v4()");

            entity.HasOne(d => d.Filter).WithMany(p => p.ReportFilters)
                .HasForeignKey(d => d.FilterId)
                .HasConstraintName("FK_ReportFilter_ToFilter");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportFilters)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK_ReportFilter_ToReport");
        });

        modelBuilder.Entity<ReportPackage>(entity =>
        {
            entity.ToTable("ReportPackage");

            entity.Property(e => e.ReportPackageId).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.DisplayName).HasMaxLength(254);
            entity.Property(e => e.EntityName).HasMaxLength(254);
            entity.Property(e => e.IsSetDate)
                .IsRequired()
                .HasDefaultValueSql("true");
            entity.Property(e => e.IsShow).HasDefaultValueSql("true");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Sequencenumber).HasColumnName("sequencenumber");
            entity.Property(e => e.Sqlqueryfilename).HasColumnName("sqlqueryfilename");
            entity.Property(e => e.TestId).HasMaxLength(254);
            entity.Property(e => e.TestUserId)
                .HasMaxLength(254)
                .IsFixedLength();
        });

        modelBuilder.Entity<ReportPackageEntry>(entity =>
        {
            entity.ToTable("ReportPackageEntry");

            entity.Property(e => e.ReportPackageEntryId).HasDefaultValueSql("uuid_generate_v4()");
            entity.Property(e => e.ReportReportid).HasColumnName("Report_reportid");

            entity.HasOne(d => d.ReportPackage).WithMany(p => p.ReportPackageEntries)
                .HasForeignKey(d => d.ReportPackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReportPackageReportPackageEntry");

            entity.HasOne(d => d.ReportReport).WithMany(p => p.ReportPackageEntries)
                .HasForeignKey(d => d.ReportReportid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReportReportPackageEntry");
        });

        modelBuilder.Entity<ReportParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reportparameter_pkey");

            entity.ToTable("ReportParameter");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Parameterid).HasColumnName("parameterid");
            entity.Property(e => e.Reportid).HasColumnName("reportid");

            entity.HasOne(d => d.Parameter).WithMany(p => p.ReportParameters)
                .HasForeignKey(d => d.Parameterid)
                .HasConstraintName("FK_ReportParameter_parameterid");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportParameters)
                .HasForeignKey(d => d.Reportid)
                .HasConstraintName("FK_ReportParameter_reportid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
