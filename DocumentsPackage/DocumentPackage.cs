using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsPackage
{
    public class DocumentPackage
    {
        public List<string> TemplateList { get; private set; }

        public List<PrintableDocument> Documents { get; set; }

        public IDocPrintStrategy DocPrintStrategy { get; private set; }

        public DocumentPackage(IDocPrintStrategy docPrintStrategy)
        {
            DocPrintStrategy = docPrintStrategy;
            TemplateList = new List<string>();
            Documents = new List<PrintableDocument>();
        }

        public DocumentPackage(List<string> templateList, IDocPrintStrategy docPrintStrategy) : this(docPrintStrategy)
        {
            TemplateList = templateList;
        }

        public DocumentPackage()
        {
        }

        public void PrintAll()
        {
            Documents.Clear();

            if (TemplateList != null)
            {
                foreach (string template in TemplateList)
                    Documents.Add(DocPrintStrategy.PrintDocument(template));
            }
        }

        public byte[] GetMergedDocuments()
        {
            byte[] result = null;

            if (Documents == null || !Documents.Any())
                return result;

            return result;
        }

        public byte[] GetCompressedDocuments(int compressLevel = 5)
        {
            byte[] result = null;

            if (Documents == null || !Documents.Any())
                return result;

            using (var ms = new MemoryStream())
            {
                using (var s = new ZipOutputStream(ms))
                {
                    s.SetLevel(compressLevel);
                    s.UseZip64 = UseZip64.Off;

                    foreach (var doc in Documents)
                    {
                        var entry = new ZipEntry(doc.FileName);
                        entry.DateTime = DateTime.Now;
                        entry.Size = doc.Data.Length;
                        s.PutNextEntry(entry);
                        s.Write(doc.Data, 0, doc.Data.Length);
                    }

                    s.Finish();
                    result = ms.ToArray();
                }

                return result;
            }
        }
    }
}
