using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class DocumentContent : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public string? Template { get; set; }

    public string? Data { get; set; }

    public int Version { get; set; }

    public bool DefaultVersion { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? CreatedBy { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual ObservableCollection<Paragraph> Paragraphs { get; } = new ObservableCollection<Paragraph>();

    public event PropertyChangedEventHandler PropertyChanged;
}
