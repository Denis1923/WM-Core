using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class ReportParameter : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public Guid Reportid { get; set; }

    public Guid Parameterid { get; set; }

    public virtual Parameter Parameter { get; set; } = null!;

    public virtual Report Report { get; set; } = null!;
}
