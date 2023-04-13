using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class ReportPackageEntry : INotifyPropertyChanged
{
    public Guid ReportPackageEntryId { get; set; }

    public bool IsObligatory { get; set; }

    public int? NumberOfCopies { get; set; }

    public Guid ReportReportid { get; set; }

    public Guid ReportPackageId { get; set; }

    public int NumberPosition { get; set; }

    public string? SqlQueryCopyNumber { get; set; }

    public virtual ObservableCollection<Condition> Conditions { get; } = new ObservableCollection<Condition>();

    public virtual ReportPackage ReportPackage { get; set; } = null!;

    public virtual Report ReportReport { get; set; } = null!;

    public event PropertyChangedEventHandler PropertyChanged;
}
