using System.ComponentModel.DataAnnotations.Schema;

namespace WordMergeEngine.Models
{
    public partial class Condition
    {
        [NotMapped]
        public string ChangedDataQuery { get; set; }

        [NotMapped]
        public string DataQuery => !string.IsNullOrEmpty(ChangedDataQuery) ? ChangedDataQuery : Dataquery;
    }
}
