using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core.GlobalSource
{
    public partial class GlobalSourceCheck : UserControl
    {
        private DataModel _context { get; set; }

        public GlobalSourceCheck()
        {
            InitializeComponent();
            _context = ((App)Application.Current).GetDBContext;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConditionsList();
        }

        private void EditCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditConditionHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ConditionDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditConditionHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void SearchTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            try
            {
                LoadConditionsList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConditionDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите проверку из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var condition = (WordMergeEngine.Models.Condition)ConditionDataGrid.SelectedItem;

                if (MessageBox.Show("Выбранная проверка будет безвозвратно удалена. Продолжить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                var reportConditions = condition.ReportConditions.ToList();

                foreach (var reportCondition in reportConditions)
                    _context.ReportConditions.Remove(reportCondition);

                _context.SaveChanges();
                _context.Conditions.Remove(condition);
                _context.SaveChanges();

                LoadConditionsList();

                MessageBox.Show("Проверка был успешно удалена", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void CopyCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConditionDataGrid.SelectedItems == null || ConditionDataGrid.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Необходимо выбрать проверку для копирования");
                    return;
                }

                var oldConditionsToCopy = ConditionDataGrid.SelectedItems.Cast<WordMergeEngine.Models.Condition>().ToList();
                var newConditions = new List<WordMergeEngine.Models.Condition>();

                foreach (var oldCondition in oldConditionsToCopy)
                {
                    var condition = new WordMergeEngine.Models.Condition
                    {
                        Conditionid = Guid.NewGuid(),
                        Conditionoperator = oldCondition.Conditionoperator,
                        Dataquery = oldCondition.Dataquery,
                        Recordcount = oldCondition.Recordcount,
                        Conditiontype = oldCondition.Conditiontype,
                        Errormessage = oldCondition.Errormessage,
                        Conditionname = oldCondition.Conditionname,
                        Isglobal = oldCondition.Isglobal,
                        Createdon = DateTime.Now,
                        Modifiedon = DateTime.Now,
                        Createdby = WindowsIdentity.GetCurrent().Name,
                        Modifiedby = WindowsIdentity.GetCurrent().Name,
                    };

                    _context.Conditions.Add(condition);
                    _context.SaveChanges();

                    LoadConditionsList();

                    newConditions.Add(condition);
                }

                MessageBox.Show("Копирование завершено успешно", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);

                if (newConditions.Any())
                    LoadConditionsList(newConditions.FirstOrDefault().Conditionid);
                else
                    LoadConditionsList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void AddCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var condition = new WordMergeEngine.Models.Condition
                {
                    Conditionid = Guid.NewGuid(),
                    Conditionname = "Новая проверка",
                    Conditionoperator = "=",
                    Dataquery = "select 1 as pk",
                    Recordcount = 1,
                    Conditiontype = null,
                    Errormessage = string.Empty,
                    Isglobal = true,
                    Createdon = DateTime.Now,
                    Modifiedon = DateTime.Now,
                    Createdby = WindowsIdentity.GetCurrent().Name,
                    Modifiedby = WindowsIdentity.GetCurrent().Name
                };

                _context.Conditions.Add(condition);
                _context.SaveChanges();

                LoadConditionsList(condition.Conditionid);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DoImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadConditionsList(ImportExport.ImportConditions());
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DoExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConditionDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Необходимо выбрать проверки отчета для экспорта");
                    return;
                }

                ImportExport.Export((WordMergeEngine.Models.Condition)ConditionDataGrid.SelectedItem);

                LoadConditionsList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DoExportAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ImportExport.Export(_context.Conditions.Where(d => d.isglobal == true).ToList());

                LoadConditionsList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void LoadConditionsList(Guid? selectedConditionId = null)
        {
            IQueryable<WordMergeEngine.Models.Condition> conditions;

            if (string.IsNullOrEmpty(SearchTextBox.Text))
                conditions = from p in _context.Conditions where (bool)p.Isglobal orderby p.Conditionname select p;
            else
            {
                var searchCriteria = SearchTextBox.Text;
                conditions = from p in _context.Conditions where (bool)p.Isglobal && (p.Conditionname.ToLower().Contains(searchCriteria.ToLower())) orderby p.conditionname select p;
            }

            ConditionDataGrid.ItemsSource = conditions.ToList();

            if (selectedConditionId != null && selectedConditionId.HasValue)
            {
                var selectedItem = (from r in conditions where r.conditionid == selectedConditionId.Value select r).FirstOrDefault();

                if (selectedItem != null)
                    ConditionDataGrid.SelectedItem = selectedItem;
            }
        }

        private void EditConditionHandler()
        {
            if (ConditionDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите проверку отчета для редактирования из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          ((((Parent as TabItem).Parent as TabControl).Parent as Grid).Parent as Page).NavigationService.Navigate(new EditReport((WordMergeEngine.Model.Condition)ConditionDataGrid.SelectedItem));
        }
    }
}
