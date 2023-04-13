using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class ReportPackage : INotifyPropertyChanged
{
    public Guid ReportPackageId { get; set; }

    public string Name { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? EntityName { get; set; }

    public string? TestId { get; set; }

    public bool? IsShow { get; set; }

    public string? TestUserId { get; set; }

    public int? Sequencenumber { get; set; }

    public string? Sqlqueryfilename { get; set; }

    public bool? IsSetDate { get; set; }

    public virtual ObservableCollection<ReportPackageEntry> ReportPackageEntries { get; } = new ObservableCollection<ReportPackageEntry>();

    public event PropertyChangedEventHandler PropertyChanged;
}
