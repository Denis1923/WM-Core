using Npgsql;
using System.Data.Common;
using System.Data.SqlClient;
using WordMergeEngine.Models;
using WordMergeEngine.Models.Contexts;
using WordMergeEngine.Models.Helpers;

namespace WordMergeEngine
{
    public partial class ExtractDataEngine
    {
        public static string GetNpgsql(string aServerName, string aDatabaseName = null, string username = null, string password = null)
        {
            var psqlBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = aServerName,
                Port = 5432,
                Database = aDatabaseName,
                Username = username,
                Password = password
            };

            return psqlBuilder.ToString();
        }

        public static DataModel GetContext(ConnectionData connection)
        {
            if (connection.ConnectionType == ConnectionType.PostgresDbConnection)
            {
                var providerString = GetNpgsql(connection.ServerName, connection.DatabaseName, connection.UserName, connection.Password);

                using (var sqlConn = new NpgsqlConnection(providerString))
                    sqlConn.Open();

                return new PsSqlDataModel(new NpgsqlConnection(providerString));
            }
            else
            {
                var sqlBuilder = new SqlConnectionStringBuilder
                {
                    DataSource = connection.ServerName,
                    InitialCatalog = connection.DatabaseName,
                    IntegratedSecurity = true,
                    MultipleActiveResultSets = true
                };

                return new MsSqlDataModel(new SqlConnection(sqlBuilder.ToString()));
            }
        }

        public static DbConnection GetConnection(ConnectionData connection, int? timeout = null)
        {
            if (connection.ConnectionType == ConnectionType.PostgresDbConnection)
            {
                return new NpgsqlConnection(GetNpgsql(connection.ServerName, connection.DatabaseName, connection.UserName, connection.Password));
            }

            var sqlConnection = new SqlConnectionStringBuilder
            {
                DataSource = connection.ServerName,
                IntegratedSecurity = true
            };

            if (!string.IsNullOrEmpty(connection.DatabaseName))
                sqlConnection.InitialCatalog = connection.DatabaseName;

            if (timeout != null)
                sqlConnection.ConnectTimeout = timeout.Value;

            return new SqlConnection(sqlConnection.ToString());
        }

        public static DbCommand GetCommand(DbConnection connection, string cmdText)
        {
            if (connection is NpgsqlConnection npgsqlConnection)
                return new NpgsqlCommand(cmdText, npgsqlConnection);

            if (connection is SqlConnection sqlConnection)
                return new SqlCommand(cmdText, sqlConnection);

            throw new ApplicationException("Неопознанный тип подключения");
        }

        public static DbDataAdapter GetAdapter(DbCommand command)
        {
            if (command is NpgsqlCommand npgsqlCommand)
                return new NpgsqlDataAdapter(npgsqlCommand);

            if (command is SqlCommand sqlCommand)
                return new SqlDataAdapter(sqlCommand);

            throw new ApplicationException("Неопознанный тип команды");
        }
    }
}
