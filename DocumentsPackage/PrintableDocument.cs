using System.Diagnostics;

namespace DocumentsPackage
{
    public class PrintableDocument
    {
        [DebuggerDisplay("TemplateId = {TemplateId}, FileName = {FileName}")]
        public string TemplateId { get; set; }

        public string FileName { get; set; }

        public byte[] Data { get; set; }

        public List<string> PrintErrors { get; private set; }

        public bool NeedSaveToSharePoint { get; set; }

        public string SpFolder { get; set; }

        public PrintableDocument()
        {
            PrintErrors = new List<string>();
        }

        public PrintableDocument(string templateId)
            : this()
        {
            TemplateId = templateId;
        }
    }
}
