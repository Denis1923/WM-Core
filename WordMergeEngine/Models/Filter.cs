using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class Filter : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public Guid? ParentFilterId { get; set; }

    public string? ParentConditionOperator { get; set; }

    public string? DisplayName { get; set; }

    public int? Order { get; set; }

    public string? Type { get; set; }

    public string? Query { get; set; }

    public string? VisibleQuery { get; set; }

    public string? VisibleConditionOperator { get; set; }

    public decimal? VisibleRecordCount { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? ModifiedBy { get; set; }

    public virtual ObservableCollection<FilterParentFilter> FilterParentFilterFilters { get; } = new ObservableCollection<FilterParentFilter>();

    public virtual ObservableCollection<FilterParentFilter> FilterParentFilterParentFilters { get; } = new ObservableCollection<FilterParentFilter>();

    public virtual ObservableCollection<ReportFilter> ReportFilters { get; } = new ObservableCollection<ReportFilter>();

    public event PropertyChangedEventHandler PropertyChanged;
}
