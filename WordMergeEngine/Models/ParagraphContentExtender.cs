using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordMergeEngine.Models
{
    public partial class ParagraphContent
    {
        [NotMapped]
        public bool PassConditions { get; set; }

        [NotMapped]
        public bool DiffSource { get; set; }

        [NotMapped]
        public bool DiffTarget { get; set; }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                var s = Name;

                if (DefaultVersion == true)
                {
                    s += "*";
                }
                else
                {
                    if (!string.IsNullOrEmpty(Condition)) s += "◘";
                }

                if (DiffSource == true) s += " (Исходный)";
                if (DiffTarget == true) s += " (Целевой)";

                return s;
            }
        }
    }
}
