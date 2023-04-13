using System.Data.Common;

namespace WordMergeEngine.Models
{
    public partial class DataModel
    {
        public DataModel(DbConnection connection) : base(connection, true)
        {

        }
    }
}
