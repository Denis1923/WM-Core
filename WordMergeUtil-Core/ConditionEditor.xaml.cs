using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core
{
    public partial class ConditionEditor : Window
    {
        public ReportPackageEntry CurrentPackageEntry { get; set; }

        public ConditionEditor(ReportPackageEntry currententry)
        {
            CurrentPackageEntry = currententry;

            InitializeComponent();

            CusomizeSQLEditor(SQLConditionQueryEditor2);
        }

        private void ReloadConditions()
        {
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

            var ctx = ((App)Application.Current).GetDBContext;
            ctx.SaveChanges();
        }

        private void tbtAddCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var context = ((App)Application.Current).GetDBContext;

                var condition = new WordMergeEngine.Models.Condition();
                condition.Conditionid = Guid.NewGuid();
                condition.Conditionname = "Новая проверка";
                condition.Conditionoperator = "=";
                condition.Dataquery = "select 1 as pk";
                condition.Recordcount = 1;
                condition.Conditiontype = 1;

                CurrentPackageEntry.Conditions.Add(condition);

                var reportCondition = new ReportCondition();
                reportCondition.Id = Guid.NewGuid();
                reportCondition.Condition = condition;
                reportCondition.Report = CurrentPackageEntry.ReportReport;

                condition.ReportConditions.Add(reportCondition);
                context.ReportConditions.Add(reportCondition);

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadConditions();
        }

        private void lbxCondtionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (lbxCondtionList.SelectedItem != null)
                {
                    DataContext = lbxCondtionList.SelectedItem;

                    if (lbxCondtionList.SelectedItem is WordMergeEngine.Models.Condition)
                    {
                        var condition = lbxCondtionList.SelectedItem as WordMergeEngine.Models.Condition;
                        SQLConditionQueryEditor2.Text = condition.dataquery;
                        SQLConditionQueryEditor2.IsEnabled = true;
                        ConditionNameTextBox2.IsEnabled = true;
                        RecordCountTextBox2.IsEnabled = true;
                        SQLConditionQueryEditor2.Select(0, 0);
                    }
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

                var s = new MemoryStream((byte[])Properties.Resources.ResourceManager.GetObject("tsql"));

                using (var reader = new XmlTextReader(s))
                    editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveData();
        }
    }
}
