using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace WordMergeEngine.Models.Configurations
{
    public class MsSqlConfiguration : DbConfiguration //System.Data.Entity.
    {
        private const string MSSqlProvider = "System.Data.SqlClient";

        public MsSqlConfiguration()
        {
            this.SetDefaultConnectionFactory(new SqlConnectionFactory());
            this.SetProviderServices(MSSqlProvider, System.Data.Entity.SqlServer.SqlProviderServices.Instance);
            this.SetProviderFactory(MSSqlProvider, SqlClientFactory.Instance);
            this.SetDefaultConnectionFactory(new SqlConnectionFactory());
        }
    }
}
