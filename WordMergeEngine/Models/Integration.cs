using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class Integration : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public Guid? ReportId { get; set; }

    public string? SystemCode { get; set; }

    public Guid? SourceId { get; set; }

    public int Order { get; set; }

    public bool BeforeAction { get; set; }

    public virtual Report? Report { get; set; }

    public virtual DataSource? Source { get; set; }
}
