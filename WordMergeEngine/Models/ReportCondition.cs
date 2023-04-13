using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class ReportCondition : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public Guid Reportid { get; set; }

    public Guid Conditionid { get; set; }

    public virtual Condition Condition { get; set; } = null!;

    public virtual Report Report { get; set; } = null!;
}
