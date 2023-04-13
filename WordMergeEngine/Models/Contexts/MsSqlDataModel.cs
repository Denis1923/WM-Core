using WordMergeEngine.Models.Configurations;
using System.Data.Entity;
using System.Data.Common;

namespace WordMergeEngine.Models.Contexts
{
    [DbConfigurationType(typeof(MsSqlConfiguration))] //System.Data.Entity
    public class MsSqlDataModel : DataModel
    {
        public MsSqlDataModel(DbConnection connection) : base(connection)
        {
        }
    }
}
