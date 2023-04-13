using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WordMergeEngine.Models;

public partial class ParagraphContent : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Content { get; set; }

    public Guid? ParagraphId { get; set; }

    public string? Condition { get; set; }

    public bool Deleted { get; set; }

    public bool? DefaultVersion { get; set; }

    public DateTime? ActiveFrom { get; set; }

    public DateTime? ActiveTill { get; set; }

    public string? Tooltip { get; set; }

    public DateTime? CreatedOn { get; set; }

    public bool? Approved { get; set; }

    public string? Comment { get; set; }

    public virtual Paragraph? Paragraph { get; set; }
}
