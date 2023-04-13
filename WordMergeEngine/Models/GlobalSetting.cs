using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WordMergeEngine.Models;

public partial class GlobalSetting : INotifyPropertyChanged
{
    public string? ServerName { get; set; }

    public string? DbName { get; set; }

    public string? DbVersion { get; set; }

    public bool IsRemoveHighlight { get; set; }

    public string? DefaultColorText { get; set; }

    public string? GlobalDataSourcePath { get; set; }

    public string? GlobalConditionPath { get; set; }

    public Guid Id { get; set; }
}