using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class DataSource : INotifyPropertyChanged
{
    public Guid Datasourceid { get; set; }

    public string Datasourcename { get; set; } = null!;

    public string? Dataquery { get; set; }

    public string? Keyfieldname { get; set; }

    public string? Foreignkeyfieldname { get; set; }

    public bool? Assingleline { get; set; }

    public Guid? ParentDataSourceId { get; set; }

    public int? Position { get; set; }

    public string? Dataqueryxml { get; set; }

    public bool Usequerybuilder { get; set; }

    public bool? Isglobal { get; set; }

    public DateTime? Createdon { get; set; }

    public string? Createdby { get; set; }

    public DateTime? Modifiedon { get; set; }

    public string? Modifiedby { get; set; }

    public bool IsIntegrationRequest { get; set; }

    public virtual ObservableCollection<Integration> Integrations { get; } = new ObservableCollection<Integration>();

    public virtual ObservableCollection<DataSource> InverseParentDataSource { get; } = new ObservableCollection<DataSource>();

    public virtual DataSource? ParentDataSource { get; set; }

    public virtual ObservableCollection<Report> Reports { get; } = new ObservableCollection<Report>();

    public event PropertyChangedEventHandler PropertyChanged;
}
