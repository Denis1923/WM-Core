using System.Collections.Generic;
using WordMergeEngine.Models;

namespace WordMergeUtil_Core.AgreementTool.DataContracts
{
    public class TagParagraphItem
    {
        public int Number { get; set; }

        public string DocumentName { get; set; }

        public Document Document { get; set; }

        public string ParagraphName { get; set; }

        public List<ParagraphContent> Contents { get; set; }
    }
}
