using System;
using WordMergeEngine.Models;

namespace WordMergeUtil_Core.AgreementTool.DataContracts
{
    public class CompareParagraph
    {
        public int Number { get; set; }

        public Document Document { get; }

        public string DocumentName => Document?.Name;

        public Paragraph Paragraph { get; }

        public Guid? Id => Paragraph.Id;

        public string ParagraphName => Paragraph.Name;

        public ParagraphContent Version { get; }

        public string VersionName => Version?.Name;

        public double Similarity { get; set; }

        public CompareParagraph(Document document, Paragraph paragraph, ParagraphContent content)
        {
            Document = document;
            Paragraph = paragraph;
            Version = content;
        }

        public CompareParagraph(Paragraph paragraph, ParagraphContent content)
        {
            Paragraph = paragraph;
            Version = content;
        }
    }
}
