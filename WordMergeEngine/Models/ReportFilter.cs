using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class ReportFilter : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public Guid? ReportId { get; set; }

    public Guid? FilterId { get; set; }

    public string? Value { get; set; }

    public virtual Filter? Filter { get; set; }

    public virtual Report? Report { get; set; }
}
