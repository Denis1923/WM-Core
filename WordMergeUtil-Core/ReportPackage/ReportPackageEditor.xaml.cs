using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;
using RReportPackage = WordMergeEngine.Models.ReportPackage;

namespace WordMergeUtil_Core
{
    public partial class ReportPackageEditorWindow : Window
    {
        public RReportPackage CurrentPackage { get; set; }

        public ReportPackageEditorWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentPackage == null)
                    CurrentPackage = new RReportPackage();

                var ctx = ((App)Application.Current).GetDBContext;

                DataContext = CurrentPackage;

                if (CurrentPackage.ReportPackageEntries.Count > 0)
                {
                    listDocuments.ItemsSource = CurrentPackage.ReportPackageEntries;
                    listDocuments.DisplayMemberPath = "Name";
                }

                ReloadList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ReloadList()
        {
            var ctx = ((App)Application.Current).GetDBContext;

            var entries = (from en in ctx.ReportPackageEntries where en.ReportPackage.ReportPackageId == CurrentPackage.ReportPackageId orderby en.NumberPosition ascending select en).ToList();

            listDocuments.ItemsSource = entries;
            listDocuments.DisplayMemberPath = "Report.reportname";

            if (CurrentPackage.IsShow == null)
                CurrentPackage.IsShow = true;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addEntryWindow = new ReportPackageEntryEditor
                {
                    CurrentPackage = CurrentPackage,
                    CurrentPackageEntry = null
                };

                addEntryWindow.ShowDialog();
                ReloadList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listDocuments.SelectedItem == null)
                    return;

                var addEntryWindow = new ReportPackageEntryEditor
                {
                    CurrentPackage = CurrentPackage,
                    CurrentPackageEntry = listDocuments.SelectedItem as ReportPackageEntry
                };

                addEntryWindow.ShowDialog();
                ReloadList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (listDocuments.SelectedItem == null)
                    return;

                var item = (ReportPackageEntry)listDocuments.SelectedItem;

                if (MessageBox.Show("Вы уверены, что хотите убрать документ из комплекта?", "Вы уверены?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                var ctx = ((App)Application.Current).GetDBContext;
                ctx.ReportPackageEntries.Remove(item);
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void listDocuments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEdit_Click(sender, e);
        }

        private void cmdTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var testWindow = new TestComWin();
                testWindow.cbxPackades.SelectedItem = CurrentPackage;
                testWindow.txtGUID.Text = tbxTestId.Text;
                testWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void btnclose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
