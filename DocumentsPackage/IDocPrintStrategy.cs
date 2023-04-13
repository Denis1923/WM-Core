using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsPackage
{
    public interface IDocPrintStrategy
    {
        PrintableDocument PrintDocument(string templateId);
    }
}
