using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class Parameter : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Datatype { get; set; }

    public string? Nullable { get; set; }

    public string? Query { get; set; }

    public string? Errormessage { get; set; }

    public string? Testval { get; set; }

    public string? Displayname { get; set; }

    public int? Displayorder { get; set; }

    public bool? Isglobal { get; set; }

    public DateTime? Createdon { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Modifiedon { get; set; }

    public string? Modifiedby { get; set; }

    public virtual ObservableCollection<ParameterCondition> ParameterConditions { get; } = new ObservableCollection<ParameterCondition>();

    public virtual ObservableCollection<ReportParameter> ReportParameters { get; } = new ObservableCollection<ReportParameter>();

    public event PropertyChangedEventHandler PropertyChanged;
}
