using System.ComponentModel;
using System.Security.Principal;
using System.Windows;

namespace WordMergeUtil_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load()
        {
            Top = Settings.Default.Top;
            Left = Settings.Default.Left;
            Height = Settings.Default.Height;
            Width = Settings.Default.Width;

            if (Settings.Default.WindowState == WindowState.Maximized)
                WindowState = WindowState.Maximized;

            userLabel.Text += WindowsIdentity.GetCurrent().Name;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
            }
            else
            {
                Settings.Default.Top = Top;
                Settings.Default.Left = Left;
                Settings.Default.Height = Height;
                Settings.Default.Width = Width;
                Settings.Default.WindowState = WindowState;
            }

            Settings.Default.Save();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Load();
        }
    }
}
