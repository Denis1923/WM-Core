using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class ParameterCondition : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public Guid? ParameterId { get; set; }

    public string? Name { get; set; }

    public string? Query { get; set; }

    public string? ConditionOperator { get; set; }

    public decimal? RecordCount { get; set; }

    public virtual Parameter? Parameter { get; set; }
}
