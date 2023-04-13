using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;

using RReportPackage = WordMergeEngine.Models.ReportPackage;

namespace WordMergeUtil_Core
{
    public partial class ReportPackageEditorPage : Page
    {
        public ReportPackageEditorPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(ReportPackageEditorPage_Loaded);
            CusomizeSQLEditor(SQLConditionQueryEditor2);
            CusomizeSQLEditor(SQLQueryFileName);
        }

        void ReportPackageEditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentPackage == null)
                    CurrentPackage = new RReportPackage();

                var ctx = ((App)Application.Current).GetDBContext;

                DataContext = CurrentPackage;

                if (CurrentPackage.ReportPackageEntry.Count > 0)
                {
                    listDocuments.ItemsSource = CurrentPackage.ReportPackageEntry;
                    listDocuments.DisplayMemberPath = "Name";
                }

                SQLQueryFileName.Text = CurrentPackage.sqlqueryfilename;

                ReloadList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        public RReportPackage CurrentPackage { get; set; }

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

                ReloadList();
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
                testWindow.txtUserId.Text = tbxTestUserId.Text;
                testWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        public ReportPackageEntry CurrentPackageEntry { get; set; }

        private void ReloadConditions()
        {
            if (CurrentPackageEntry == null)
                return;

            lbxCondtionList.ItemsSource = CurrentPackageEntry.Conditions;
            lbxCondtionList.DisplayMemberPath = "conditionname";
        }

        private void SaveData()
        {
            if (lbxCondtionList.SelectedItem != null)
            {
                var cond = lbxCondtionList.SelectedItem as WordMergeEngine.Models.Condition;
                cond.Dataquery = SQLConditionQueryEditor2.Text;
            }

            if (CurrentPackage.Sequencenumber == null)
                CurrentPackage.Sequencenumber = 0;

            CurrentPackage.Sqlqueryfilename = SQLQueryFileName.Text;

            var ctx = ((App)Application.Current).GetDBContext;
            ctx.SaveChanges();
        }

        private void tbtAddCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentPackageEntry?.ReportReport == null)
                    throw new ApplicationException("Сначала необходимо выбрать Документ");

                var condition = new WordMergeEngine.Models.Condition();
                condition.Conditionid = Guid.NewGuid();
                condition.Conditionname = "Новая проверка";
                condition.Conditionoperator = "=";
                condition.Dataquery = "select 1 as pk";
                condition.Recordcount = 1;
                condition.Conditiontype = 1;

                var reportCondition = new ReportCondition();
                reportCondition.Id = Guid.NewGuid();
                reportCondition.Report = CurrentPackageEntry.ReportReport;
                reportCondition.Condition = condition;

                condition.ReportConditions.Add(reportCondition);

                CurrentPackageEntry.Conditions.Add(condition);
                ReloadConditions();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void tbtRemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lbxCondtionList.SelectedItem == null) return;

                if (MessageBox.Show("Вы уверены что хотите удалить условие?", "Вы уверены?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                var ctx = ((App)Application.Current).GetDBContext;
                ctx.Conditions.Remove((WordMergeEngine.Models.Condition)lbxCondtionList.SelectedItem);

                ReloadConditions();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void lbxCondtionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lbxCondtionList.SelectedItem != null)
                {
                    CondGrid.DataContext = lbxCondtionList.SelectedItem;

                    if (lbxCondtionList.SelectedItem is WordMergeEngine.Models.Condition)
                    {
                        var condition = lbxCondtionList.SelectedItem as WordMergeEngine.Models.Condition;
                        SQLConditionQueryEditor2.Text = condition.Dataquery;
                        SQLConditionQueryEditor2.IsEnabled = true;
                        ConditionNameTextBox2.IsEnabled = true;
                        RecordCountTextBox2.IsEnabled = true;
                        SQLConditionQueryEditor2.Select(0, 0);
                    }
                }
                else
                {
                    SQLConditionQueryEditor2.Text = "";
                    ConditionNameTextBox2.Text = "";
                    RecordCountTextBox2.Text = "0";
                    ErrMsg.Text = "";
                }
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
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

        private void listDocuments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPackageEntry = listDocuments.SelectedItem as ReportPackageEntry;
            ReloadConditions();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }
    }
}
