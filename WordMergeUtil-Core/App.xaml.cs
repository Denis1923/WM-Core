using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using WordMergeEngine.Models.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine;
using WordMergeUtil_Core.Assets;
using Microsoft.EntityFrameworkCore;

namespace WordMergeUtil_Core
{
    public partial class App : Application
    {
        private DataModel _context;

        public App() : base()
        {
            Dispatcher.UnhandledException += App_DispatcherUnhandledException;
        }

        public DataModel GetDBContext
        {
            get
            {
                if (_context == null)
                    throw new ApplicationException("Соединение с базой данных не установлено!");

                return _context;
            }
        }

        private ConnectionData _connection;
        public ConnectionData GetConnection => _connection;

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Произошло необработанное исключение: {e.Exception.Message}\n{e.Exception.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        public void ConnectToMetaDB(string aServerName, string aDatabaseName, string username, string password)
        {
            _connection = new ConnectionData(aServerName, Settings.Default.DataSourceType == DataSourceType.PostgreSQL.ToString() ? ConnectionType.PostgresDbConnection : ConnectionType.MsSqlConnection, aDatabaseName, username, password);
            _context = ExtractDataEngine.GetContext(_connection);
        }

        public DataModel GetOtherTempDBContext(string aServerName, string aDatabaseName, string username, string password)
        {
            return ExtractDataEngine.GetContext(new ConnectionData(aServerName, Settings.Default.DataSourceType == DataSourceType.PostgreSQL.ToString() ? ConnectionType.PostgresDbConnection : ConnectionType.MsSqlConnection, aDatabaseName, username, password));
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_context == null)
                return;

            RollbackChanges();

            var userName =
#if DEBUG
            @"HQMCDSOFT\Test_DEBUG";
#else
            System.Security.Principal.WindowsIdentity.GetCurrent().Name;
#endif

            foreach (var report in _context.Reports.Where(x => x.IsLocked && x.LockedBy == userName))
            {
                report.IsLocked = false;
                report.LockedBy = null;
            }

            foreach (var document in _context.Documents.Where(x => x.IsLocked && x.LockedBy == userName))
            {
                document.IsLocked = false;
                document.LockedBy = null;
            }

            _context.SaveChanges();
        }

        private void RollbackChanges()
        {
            if (_context == null)
                return;

            var collection = _context.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged);

            foreach (var e in collection)
            {
                switch (e.State)
                {
                    case EntityState.Modified:
                        e.CurrentValues.SetValues(e.OriginalValues);
                        e.State = EntityState.Unchanged;
                        break;

                    case EntityState.Added:
                        e.State = EntityState.Detached;
                        break;

                    case EntityState.Deleted:
                        e.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
