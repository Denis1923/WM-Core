using System.ComponentModel.DataAnnotations.Schema;


namespace WordMergeEngine.Models
{
    public partial class Document
    {
        [NotMapped]
        public DocumentContent CurrentVersion
        {
            get
            {
                return DocumentContents?.OrderByDescending(x => x.Version).FirstOrDefault(x => x.DefaultVersion);
            }
        }

        [NotMapped]
        public string Template => CurrentVersion?.Template;

        [NotMapped]
        public string Data => CurrentVersion?.Data;

        [NotMapped]
        public ICollection<Paragraph> Paragraph => CurrentVersion?.Paragraphs;
    }
}
