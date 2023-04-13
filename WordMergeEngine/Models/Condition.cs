using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class Condition : INotifyPropertyChanged
{
    public Guid Conditionid { get; set; }

    public string Conditionname { get; set; } = null!;

    public string? Dataquery { get; set; }

    public string? Conditionoperator { get; set; }

    public decimal? Recordcount { get; set; }

    public string? Errormessage { get; set; }

    public int? Conditiontype { get; set; }

    public Guid? ReportPackageEntryId { get; set; }

    public bool? Precondition { get; set; }

    public bool? Isglobal { get; set; }

    public DateTime? Createdon { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Modifiedon { get; set; }

    public string? Modifiedby { get; set; }

    public int? OrderNo { get; set; }

    public virtual ObservableCollection<ReportCondition> ReportConditions { get; } = new ObservableCollection<ReportCondition>();

    public virtual ReportPackageEntry? ReportPackageEntry { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}
