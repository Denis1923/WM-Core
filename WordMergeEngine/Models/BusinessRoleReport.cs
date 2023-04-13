using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class BusinessRoleReport : INotifyPropertyChanged
{
    public Guid BusinessRoleReportId { get; set; }

    public Guid BusinessRoleId { get; set; }

    public Guid ReportId { get; set; }

    public virtual BusinessRole BusinessRole { get; set; } = null!;

    public virtual Report Report { get; set; } = null!;
}
