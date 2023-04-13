using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core
{
    public partial class ReportPackageEntryEditor : Window
    {
        public ReportPackage CurrentPackage { get; set; }

        public ReportPackageEntry CurrentPackageEntry { get; set; }

        public ReportPackageEntryEditor()
        {
            InitializeComponent();
            CusomizeSQLEditor(SQLQueryNumberOfCopies);
        }

        private void RefreshConditionList()
        {
            var ctx = ((App)Application.Current).GetDBContext;
            var items = (from item in ctx.Condition where item.ReportPackageEntry.ReportPackageEntryId == CurrentPackageEntry.ReportPackageEntryId select item).ToList();

            lbxConditions.ItemsSource = CurrentPackageEntry.Condition;
            lbxConditions.DisplayMemberPath = "conditionname";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var ctx = ((App)Application.Current).GetDBContext;

                if (CurrentPackage == null && CurrentPackageEntry == null)
                    throw new ApplicationException("Не указан ни комплект документов, ни конкретный документ-элемент комплекта!");

                if (CurrentPackageEntry == null)
                {
                    CurrentPackageEntry = new ReportPackageEntry();
                    CurrentPackageEntry.ReportPackageEntryId = Guid.NewGuid();
                    CurrentPackageEntry.Condition = new ObservableCollection<WordMergeEngine.Model.Condition>();
                }
                else
                {
                    CurrentPackage = CurrentPackageEntry.ReportPackage;
                }

                DataContext = CurrentPackageEntry;

                var reports = (from r in ctx.Report where r.entityname == CurrentPackage.EntityName orderby r.reportname select r).ToList();

                cbxReportName.ItemsSource = reports;
                cbxReportName.DisplayMemberPath = "reportname";
                cbxReportName.SelectedItem = CurrentPackageEntry.Report;

                lblPackageName.Content = CurrentPackage.DisplayName;
                SQLQueryNumberOfCopies.Text = CurrentPackageEntry.SqlQueryCopyNumber;

                RefreshConditionList();
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void SavePackageEntry()
        {
            if (cbxReportName.SelectedItem == null)
            {
                MessageBox.Show("Не указан документ!", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CurrentPackageEntry.Report = (Report)cbxReportName.SelectedItem;
            CurrentPackageEntry.ReportPackage = CurrentPackage;
            CurrentPackageEntry.NumberOfCopies = int.Parse(tbxQuantity.Text);
            CurrentPackageEntry.IsObligatory = (bool)chkIsObligatory.IsChecked;
            CurrentPackageEntry.NumberPosition = int.Parse(tbxPosition.Text);
            CurrentPackageEntry.SqlQueryCopyNumber = SQLQueryNumberOfCopies.Text;

            var ctx = ((App)Application.Current).GetDBContext;

            if (ctx.ReportPackageEntry.FirstOrDefault(rpe => rpe.ReportPackageEntryId == CurrentPackageEntry.ReportPackageEntryId) == null)
                ctx.ReportPackageEntry.Add(CurrentPackageEntry);

            ctx.SaveChanges();
        }

        private void ShowConditionEditor()
        {
            try
            {
                SavePackageEntry();

                var conditionEditorWindow = new ConditionEditor(CurrentPackageEntry);

                if ((bool)conditionEditorWindow.ShowDialog())
                {
                    var ctx = ((App)Application.Current).GetDBContext;
                    ctx.SaveChanges();
                    RefreshConditionList();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void btsConditions_Click(object sender, RoutedEventArgs e)
        {
            ShowConditionEditor();
        }

        private void lbxConditions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowConditionEditor();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SavePackageEntry();
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }

            Close();
        }

        private void CusomizeSQLEditor(TextEditor editor)
        {
            try
            {
                editor.Options.EnableEmailHyperlinks = true;
                editor.Options.ConvertTabsToSpaces = true;
                editor.ShowLineNumbers = false;

                var scheme = new MemoryStream((byte[])Properties.Resources.ResourceManager.GetObject("tsql"));

                using (var reader = new XmlTextReader(scheme))
                    editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }
    }
}
