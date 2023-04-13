using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class Report : INotifyPropertyChanged
{
    public Guid Reportid { get; set; }

    public string? Reportname { get; set; }

    public string? Reportpath { get; set; }

    public string? Defaultdatabase { get; set; }

    public string? Reportcode { get; set; }

    public string? Servername { get; set; }

    public string? Testid { get; set; }

    public bool? Removeemptyregions { get; set; }

    public bool? Securitymanagement { get; set; }

    public bool? Replacefieldswithstatictext { get; set; }

    public string? Reportformat { get; set; }

    public string? Entityname { get; set; }

    public string? Reporttype { get; set; }

    public bool? SharepointDosave { get; set; }

    public string? SharepointGroupkey { get; set; }

    public string? SharepointIntcode { get; set; }

    public Guid? Datasourceid { get; set; }

    public string? Testuserid { get; set; }

    public bool? IsShow { get; set; }

    public string? Sqlqueryfilename { get; set; }

    public string? Subjectemail { get; set; }

    public DateTime? Createdon { get; set; }

    public DateTime? Modifiedon { get; set; }

    public DateTime? Activeon { get; set; }

    public DateTime? Deactiveon { get; set; }

    public DateTime? Deactivateon { get; set; }

    public int? Sequencenumber { get; set; }

    public string? Createdby { get; set; }

    public string? Modifiedby { get; set; }

    public string? SharepointRelativepath { get; set; }

    public string? SharepointFieldnameurl { get; set; }

    public bool? Useglobal { get; set; }

    public bool? Setdate { get; set; }

    public Guid? Documentid { get; set; }

    public bool IsCustomTemplate { get; set; }

    public bool IsLocked { get; set; }

    public string? LockedBy { get; set; }

    public bool IsChooseFormat { get; set; }

    public string? SpLoadType { get; set; }

    public string? PathKeyFieldName { get; set; }

    public string? NamePostfix { get; set; }

    public bool IsSendRequest { get; set; }

    public bool? IsChangeColor { get; set; }

    public string? SqlQueryUseCondition { get; set; }

    public bool IsDs { get; set; }

    public string? SqlMainReportCode { get; set; }

    public string? SqlMainRowId { get; set; }

    public virtual ObservableCollection<BusinessRoleReport> BusinessRoleReports { get; } = new ObservableCollection<BusinessRoleReport>();

    public virtual DataSource? Datasource { get; set; }

    public virtual Document? Document { get; set; }

    public virtual ObservableCollection<Integration> Integrations { get; } = new ObservableCollection<Integration>();

    public virtual ObservableCollection<ReportCondition> ReportConditions { get; } = new ObservableCollection<ReportCondition>();

    public virtual ObservableCollection<ReportFilter> ReportFilters { get; } = new ObservableCollection<ReportFilter>();

    public virtual ObservableCollection<ReportPackageEntry> ReportPackageEntries { get; } = new ObservableCollection<ReportPackageEntry>();

    public virtual ObservableCollection<ReportParameter> ReportParameters { get; } = new ObservableCollection<ReportParameter>();

    public event PropertyChangedEventHandler PropertyChanged;
}
