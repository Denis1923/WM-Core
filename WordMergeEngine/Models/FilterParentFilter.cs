using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class FilterParentFilter : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public Guid? FilterId { get; set; }

    public Guid? ParentFilterId { get; set; }

    public string? Value { get; set; }

    public virtual Filter? Filter { get; set; }

    public virtual Filter? ParentFilter { get; set; }
}
