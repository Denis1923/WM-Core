using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WordMergeEngine.Models.Helpers;
using WordMergeEngine;
using WordMergeUtil_Core.Assets;
using WordMergeEngine.Models;

namespace WordMergeUtil_Core
{
    public partial class GlobalSettinsPage : UserControl
    {
        private GlobalSetting _settings;
        public GlobalSettinsPage()
        {
            InitializeComponent();
            _settings = GetSettings();
            DataContext = _settings;

            var colors = new List<ComboBoxItem>
            {
                new ComboBoxItem { Tag = "#FF000000", Content = "Черный" },
                new ComboBoxItem { Tag = "#FFFF0000", Content = "Красный" },
                new ComboBoxItem { Tag = "#FF00FF00", Content = "Зеленый" },
                new ComboBoxItem { Tag = "#FF0000FF", Content = "Синий" },
                new ComboBoxItem { Tag = "", Content = "Другой" },
            };

            DefaultColorText.Visibility = Visibility.Collapsed;

            if (!string.IsNullOrEmpty(_settings.DefaultColorText))
                DefaultColorRect.Fill = GetBrush(_settings.DefaultColorText);

            DefaultColorComboBox.ItemsSource = colors;
            DefaultColorComboBox.SelectedItem = colors.FirstOrDefault(x => x.Tag.ToString() == _settings.DefaultColorText) ?? colors.FirstOrDefault(x => string.IsNullOrEmpty(x.Tag.ToString()));

            var serverName = _settings.ServerName;
            var dbName = _settings.DbName;

            if (!string.IsNullOrEmpty(_settings.ServerName))
            {
                InitDataBaseList(_settings.ServerName, _settings.DbName);
            }
        }

        private static GlobalSetting GetSettings()
        {
            try
            {
                GlobalSetting gs;

                var ctx = ((App)Application.Current).GetDBContext;

                var gss = (from p in ctx.GlobalSettings select p);

                if (gss.Count() > 0)
                    gs = gss.First();
                else
                {
                    gs = new GlobalSetting() { Id = Guid.NewGuid() };
                    ctx.GlobalSettings.Add(gs);
                }

                return gs;
            }
            catch
            {
                return new GlobalSetting();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var ctx = ((App)App.Current).GetDBContext;
                _settings.DefaultColorText = DefaultColorText.Text;
                ctx.SaveChanges();
            }
            catch
            {
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ctx = ((App)Application.Current).GetDBContext;
                _settings.DefaultColorText = DefaultColorText.Text;
                ctx.SaveChanges();
                MessageBox.Show("Настройки успешно сохранены", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DefaultColorText_TextChanged(object sender, TextChangedEventArgs e)
        {
            DefaultColorRect.Fill = GetBrush(DefaultColorText.Text);
        }

        private SolidColorBrush GetBrush(string hexString)
        {
            var hex = hexString.Replace("#", string.Empty);

            if (string.IsNullOrEmpty(hex))
                return Brushes.White;

            var colorInt = int.Parse(hex, NumberStyles.HexNumber);

            var a = (byte)(colorInt >> 24);
            var r = (byte)(colorInt >> 16);
            var g = (byte)(colorInt >> 8);
            var b = (byte)colorInt;

            var color = Color.FromArgb(a, r, g, b);

            return new SolidColorBrush(color);
        }

        private void DefaultColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = DefaultColorComboBox.SelectedItem as ComboBoxItem;

            if (!string.IsNullOrEmpty(selected.Tag.ToString()))
                DefaultColorText.Text = selected.Tag.ToString();

            DefaultColorText.Visibility = string.IsNullOrEmpty(selected.Tag.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool TestForServer(string address)
        {
            var timeout = 500;
            var result = false;
            var port = ((App)Application.Current).GetConnection.ConnectionType == ConnectionType.PostgresDbConnection ? 5432 : 1433;

            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    var asyncResult = socket.BeginConnect(address, port, null, null);
                    result = asyncResult.AsyncWaitHandle.WaitOne(timeout, true);
                    socket.Close();
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        private void InitDataBaseList(string serverName, string selectedValue)
        {
            if (!TestForServer(serverName))
                return;

            var databases = new Dictionary<string, string>();
            var sqlConn = ExtractDataEngine.GetConnection(((App)Application.Current).GetConnection.CreateConnection(serverName, null));

            sqlConn.Open();

            var tblDatabases = sqlConn.GetSchema("Databases");

            sqlConn.Close();

            foreach (DataRow row in tblDatabases.Rows)
            {
                var strDatabaseName = row["database_name"].ToString();

                if (!strDatabaseName.ToUpper().Contains("_WORDMERGERDB") && !strDatabaseName.ToUpper().Contains("TEMPLATE"))
                    databases.Add(strDatabaseName, strDatabaseName);
            }

            BaseTextComboBox.ItemsSource = databases.OrderBy(i => i.Key);

            if (!string.IsNullOrEmpty(selectedValue))
                BaseTextComboBox.SelectedValue = selectedValue;
        }

        private void ServerDbNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var serverName = (sender as TextBox).Text;
            var selectedValue = string.Empty;

            if (BaseTextComboBox.SelectedValue != null)
                selectedValue = BaseTextComboBox.SelectedValue.ToString();

            BaseTextComboBox.ItemsSource = null;
            InitDataBaseList(serverName, selectedValue);
        }
    }
}
