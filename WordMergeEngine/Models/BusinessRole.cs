using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class BusinessRole : INotifyPropertyChanged
{
    public Guid Businessroleid { get; set; }

    public int Businessrolecode { get; set; }

    public string Rolename { get; set; } = null!;

    public virtual ObservableCollection<BusinessRoleReport> BusinessRoleReports { get; } = new ObservableCollection<BusinessRoleReport>();

    public event PropertyChangedEventHandler PropertyChanged;
}
