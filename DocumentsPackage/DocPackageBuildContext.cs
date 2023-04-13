using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsPackage
{
    public class DocPackageBuildContext
    {
        public IDocPackageBuildStrategy CurrentBuildStrategy { get; private set; }

        public IDocPrintStrategy CurrentDocPrintStrategy { get; private set; }

        public CallingContext Context { get; private set; }

        public DocumentPackage DocPackage { get; private set; }

        public DocPackageBuildContext(CallingContext context, IDocPackageBuildStrategy buildStrategy, IDocPrintStrategy docPrintStrategy)
        {
            Context = context;
            CurrentBuildStrategy = buildStrategy;
            CurrentDocPrintStrategy = docPrintStrategy;
            DocPackage = new DocumentPackage(CurrentBuildStrategy.GetTemplateList(Context), CurrentDocPrintStrategy);
        }

        public DocumentPackage BuildDocPackage()
        {
            DocPackage.PrintAll();
            return DocPackage;
        }
    }
}
