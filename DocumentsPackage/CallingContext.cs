using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentsPackage
{
    public class CallingContext
    {
        public object RecordId { get; set; }

        public Guid UserId { get; set; }

        public string OrgName { get; set; }

        public string PackageName { get; set; }
    }
}
