using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class Document : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public bool IsLocked { get; set; }

    public string? LockedBy { get; set; }

    public bool TrackChanges { get; set; }

    public virtual ObservableCollection<DocumentContent> DocumentContents { get; } = new ObservableCollection<DocumentContent>();

    public virtual ObservableCollection<Report> Reports { get; } = new ObservableCollection<Report>();

    public event PropertyChangedEventHandler PropertyChanged;
}
