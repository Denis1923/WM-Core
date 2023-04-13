using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using WordMergeEngine.Models.Helpers;
using WordMergeEngine;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core
{
    public partial class LoginPage : Page
    {
        private Dictionary<DataSourceType, string> dataSourceTypes = new Dictionary<DataSourceType, string>()
        {
            { DataSourceType.Undefined, "Не указан" },
            { DataSourceType.MSSQL, "Microsoft SQL" },
            { DataSourceType.PostgreSQL, "PostgreSQL" },
        };

        private const string progressText = "Загрузка...";
        private bool isFirstTypeLoad = true;

        public LoginPage()
        {
            InitializeComponent();
        }

        private void ConnectWithServer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ServerNameTextBox.Text))
            {
                MessageBox.Show("Должно быть указано имя сервера", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DataBaseList.Text == progressText)
            {
                MessageBox.Show("Список баз данных еще грузится, просьба подождать", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DataBaseList.SelectedItem == null)
            {
                MessageBox.Show("Должно быть указано имя базы данных", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsPostgreSQL && string.IsNullOrEmpty(UsernameTextBox.Text))
            {
                MessageBox.Show("Должно быть указан логин пользователя БД", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsPostgreSQL && string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Должно быть указан пароль пользователя БД", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ((App)Application.Current).ConnectToMetaDB(ServerNameTextBox.Text, DataBaseList.Text, UsernameTextBox.Text, PasswordBox.Password);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex, "Не удалось соединиться с базой данных");
                return;
            }

            try
            {
                Settings.Default.ServerName = ServerNameTextBox.Text;
                Settings.Default.DatabaseName = DataBaseList.Text;
                Settings.Default.ShowSaveSuccessMessage = ShowSaveSuccessMessage.IsChecked.Value;
                Settings.Default.ShowStackTrace = ShowStackTrace.IsChecked.Value;
                Settings.Default.DataSourceType = ((KeyValuePair<DataSourceType, string>)cbxDataSourceType.SelectedItem).Key.ToString();
                Settings.Default.Username = UsernameTextBox.Text;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex, "Ошибка при сохранении настроек пользователя");
                return;
            }

            try
            {
                NavigationService.Navigate(new Reports());
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex, "Ошибка при переходе к окну выбора отчета");
                return;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ServerNameTextBox.Focus();
                ServerNameTextBox.Text = Settings.Default.ServerName;
                UsernameTextBox.Text = Settings.Default.Username;

                if (!string.IsNullOrEmpty(ServerNameTextBox.Text))
                {
                    if (!string.IsNullOrEmpty(Settings.Default.DatabaseName))
                        InitDataBaseList(ServerNameTextBox.Text, Settings.Default.DatabaseName);
                    else
                        InitDataBaseList(ServerNameTextBox.Text, null);
                }

                ShowStackTrace.IsChecked = Settings.Default.ShowStackTrace;
                ShowSaveSuccessMessage.IsChecked = Settings.Default.ShowSaveSuccessMessage;

                cbxDataSourceType.DataContext = dataSourceTypes;

                if (!string.IsNullOrEmpty(Settings.Default.DataSourceType))
                {
                    cbxDataSourceType.SelectedValue = Settings.Default.DataSourceType;
                    cbxDataSourceType.SelectedItem = dataSourceTypes.FirstOrDefault(x => x.Key.ToString() == Settings.Default.DataSourceType);
                }

                PostgreSqlFieldVisibility();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex, $"Ошибка при чтении настроек пользователя: {ex.Message}");
                return;
            }
        }

        private void PressEnter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ConnectWithServer_Click(sender, e);
        }

        private void ServerNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var selectedValue = DataBaseList.SelectedValue?.ToString();

            InitDataBaseList(ServerNameTextBox.Text, selectedValue);
        }

        private void InitDataBaseList(string serverName, string selectedValue)
        {
            var connectionData = new ConnectionData(serverName, IsPostgreSQL ? ConnectionType.PostgresDbConnection : ConnectionType.MsSqlConnection, string.Empty, UsernameTextBox.Text, PasswordBox.Password);
            new Thread(() =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        DataBaseList.ItemsSource = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Загрузка...", "Загрузка...") };
                        DataBaseList.Text = progressText;
                        ConnectWithServer.IsEnabled = false;
                    });

                    var databases = LoadWordMergerDatabases(connectionData);

                    Dispatcher.Invoke(() =>
                    {
                        DataBaseList.ItemsSource = databases.Select(i => new KeyValuePair<string, string>(i, i)).OrderBy(i => i.Key);

                        if (!string.IsNullOrEmpty(selectedValue) && databases.Contains(selectedValue))
                            DataBaseList.SelectedValue = selectedValue;
                        else
                            DataBaseList.SelectedValue = databases.FirstOrDefault();

                        ConnectWithServer.IsEnabled = true;
                    });
                }
                catch (Exception) { }
            }).Start();
        }

        private List<string> LoadWordMergerDatabases(WordMergeEngine.Models.Helpers.ConnectionData connectionData)
        {
            var databases = new List<string>();

            var msg = connectionData.CheckCorrectData();

            if (!string.IsNullOrEmpty(msg))
            {
                return databases;
            }

            using (var sqlConn = ExtractDataEngine.GetConnection(connectionData, LoadConnectionTimeout()))
            {
                try
                {
                    sqlConn.Open();

                    var tblDatabases = sqlConn.GetSchema("Databases");

                    foreach (DataRow row in tblDatabases.Rows)
                    {
                        var databaseName = row["database_name"].ToString();

                        if (databaseName.ToUpper().Contains("WORDMERGER"))
                            databases.Add(databaseName);
                    }
                }
                catch (SqlException se)
                {
                    ShowMessage.Warning($"Не удалось подключиться к {sqlConn?.DataSource}: {se?.Message}");
                }
                catch (NpgsqlException pe)
                {
                    ShowMessage.Warning($"Не удалось подключиться к {sqlConn?.DataSource}: {pe?.Message}");
                }
            }

            return databases;
        }

        private int LoadConnectionTimeout()
        {
            var timeoutStr = ConfigurationManager.AppSettings["ConnectionTimeout"];

            if (int.TryParse(timeoutStr, out int timeout))
                return timeout;
            else
                throw new InvalidCastException("Некорректно задан параметр ConnectionTimeout в конфигурационном файле!");
        }

        private void cbxDataSourceType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isFirstTypeLoad)
            {
                ServerNameTextBox.Text = string.Empty;
                DataBaseList.SelectedItem = null;
            }

            isFirstTypeLoad = false;

            Settings.Default.DataSourceType = ((KeyValuePair<DataSourceType, string>)cbxDataSourceType.SelectedItem).Key.ToString();
            PostgreSqlFieldVisibility();
        }

        private void PostgreSqlFieldVisibility()
        {
            var visibility = IsPostgreSQL ? Visibility.Visible : Visibility.Collapsed;
            UsernameLabel.Visibility = visibility;
            UsernameTextBox.Visibility = visibility;
            PasswordLabel.Visibility = visibility;
            PasswordBox.Visibility = visibility;
        }

        private bool IsPostgreSQL => Settings.Default.DataSourceType == DataSourceType.PostgreSQL.ToString();

        private void DebugCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;

            UsernameTextBox.IsEnabled = checkbox.IsChecked != true;
            PasswordBox.IsEnabled = checkbox.IsChecked != true;
        }
    }
}
