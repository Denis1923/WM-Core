using System.Collections.Generic;

namespace WordMergeUtil_Core.AgreementTool.DataContracts
{
    public class TagItem
    {
        public string Tag { get; set; }

        public List<TagParagraphItem> ParagraphItems { get; set; }
    }
}
