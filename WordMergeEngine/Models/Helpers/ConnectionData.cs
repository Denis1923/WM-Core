using System.Data.Common;

namespace WordMergeEngine.Models.Helpers
{
    public class ConnectionData
    {
        public string ServerName { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public string DatabaseName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public ConnectionData(string serverName, bool isPostgreSql)
        {
            ServerName = serverName;
            ConnectionType = isPostgreSql ? ConnectionType.PostgresDbConnection : ConnectionType.MsSqlConnection;
        }

        public ConnectionData(string serverName, ConnectionType connectionType, string databaseName, string userName, string password)
        {
            ServerName = serverName;
            ConnectionType = connectionType;
            DatabaseName = string.IsNullOrEmpty(databaseName) && connectionType == ConnectionType.PostgresDbConnection ? "postgres" : databaseName;
            UserName = userName;
            Password = password;
        }

        public ConnectionData(DbConnection connection)
        {
            if (connection is Npgsql.NpgsqlConnection npConnection)
            {
                ServerName = npConnection.Host;
                ConnectionType = ConnectionType.PostgresDbConnection;
                DatabaseName = npConnection.Database;
                UserName = npConnection.UserName;
                Password = new Npgsql.NpgsqlConnectionStringBuilder(npConnection.ConnectionString).Password;
            }
            else if (connection is System.Data.SqlClient.SqlConnection sqlConnection)
            {
                ServerName = sqlConnection.DataSource;
                ConnectionType = ConnectionType.MsSqlConnection;
                DatabaseName = sqlConnection.Database;
            }
            else
            {
                throw new Exception("Неизвестный тип подключения. Обратитесь к администратору.");
            }
        }

        public override string ToString()
        {
            return $"Server:{ServerName},Database:{DatabaseName},Type:{ConnectionType},Username:{UserName}";
        }

        public ConnectionData CreateConnection(string serverName, string databaseName) => new ConnectionData(serverName, ConnectionType, databaseName, UserName, Password);

        public string CheckCorrectData()
        {
            if (string.IsNullOrEmpty(ServerName))
                return "Не заполнен адрес сервера";

            if (ConnectionType == ConnectionType.PostgresDbConnection && string.IsNullOrEmpty(UserName))
                return "Не заполнено имя пользователя";

            if (ConnectionType == ConnectionType.PostgresDbConnection && string.IsNullOrEmpty(Password))
                return "Не заполнен пароль пользователя";

            return string.Empty;
        }
    }
}
