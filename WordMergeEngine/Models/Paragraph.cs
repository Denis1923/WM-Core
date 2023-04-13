using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordMergeEngine.Models;

public partial class Paragraph
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public bool? Numerable { get; set; }

    public bool? NewPage { get; set; }

    public int? OrderNo { get; set; }

    public string? Condition { get; set; }

    public bool Deleted { get; set; }

    public string? ReferenceName { get; set; }

    public DateTime? ActiveFrom { get; set; }

    public DateTime? ActiveTill { get; set; }

    public string? Tooltip { get; set; }

    public bool IsFixNumeration { get; set; }

    public Guid? ParentParagraphId { get; set; }

    public bool IsGlobal { get; set; }

    public Guid? DocumentContentId { get; set; }

    public string? Tag { get; set; }

    public bool IsAttachment { get; set; }

    public int? Level { get; set; }

    public string? CustomNo { get; set; }

    public int CustomLevel { get; set; }

    public bool IsCustomNumbering { get; set; }

    public string? StartNumberingFrom { get; set; }

    public virtual DocumentContent? DocumentContent { get; set; }

    public virtual ObservableCollection<Paragraph> InverseParentParagraph { get; } = new ObservableCollection<Paragraph>();

    public virtual ObservableCollection<ParagraphContent> ParagraphContents { get; } = new ObservableCollection<ParagraphContent>();

    public virtual Paragraph? ParentParagraph { get; set; }
}
