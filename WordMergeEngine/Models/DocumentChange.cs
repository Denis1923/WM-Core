using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class DocumentChange : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public string? ReportCode { get; set; }

    public Guid? RowId { get; set; }

    public int Version { get; set; }

    public bool? IsApplication { get; set; }

    public bool? IsChapter { get; set; }

    public string? Order { get; set; }

    public string? Content { get; set; }
}
