using Npgsql;
using System.Data.Entity;

namespace WordMergeEngine.Models.Configurations
{
    public class PostgreSqlConfiguration : DbConfiguration
    {
        private const string PostgreProvider = "Npgsql";

        public PostgreSqlConfiguration()
        {
            this.SetDefaultConnectionFactory(new NpgsqlConnectionFactory());
            this.SetProviderServices(PostgreProvider, NpgsqlServices.Instance);
            this.SetProviderFactory(PostgreProvider, NpgsqlFactory.Instance);
            this.SetDefaultConnectionFactory(new NpgsqlConnectionFactory());
        }
    }
}
