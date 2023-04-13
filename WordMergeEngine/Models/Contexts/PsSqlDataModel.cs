using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WordMergeEngine.Models.Configurations;

namespace WordMergeEngine.Models.Contexts
{
    [DbConfigurationType(typeof(PostgreSqlConfiguration))] //System.Data.Entity.
    public class PsSqlDataModel : DataModel
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) //System.Data.Entity.
        {
            modelBuilder.HasDefaultSchema("public");

            base.OnModelCreating(modelBuilder);
        }

        public PsSqlDataModel(DbConnection connection) : base(connection)
        {
        }
    }
}
