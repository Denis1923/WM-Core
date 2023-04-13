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
    public partial class GlobalSourceFilters : UserControl
    {
        private DataModel _context { get; set; }

        public GlobalSourceFilters()
        {
            InitializeComponent();
            _context = ((App)Application.Current).GetDBContext;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilterList();
        }

        private void EditFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditFilterHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void FilterDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditFilterHandler();
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
                LoadFilterList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FilterDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите фильтр из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var filter = (Filter)FilterDataGrid.SelectedItem;

                var msg = "Выбранный фильтр будет безвозвратно удален";

                if (filter.ReportFilters.Any())
                    msg = $"Выбранный фильтр связан с отчетами {string.Join(", ", filter.ReportFilters.Select(x => x.Report.Reportname))}";

                if (MessageBox.Show($"{msg}. Хотите удалить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                foreach (var reportFilter in filter.ReportFilters)
                    _context.ReportFilters.Remove(reportFilter);

                _context.SaveChanges();
                _context.Filters.Remove(filter);
                _context.SaveChanges();

                LoadFilterList();

                MessageBox.Show("Фильтр был успешно удален", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void CopyFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FilterDataGrid.SelectedItems == null || FilterDataGrid.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Необходимо выбрать фильтр для копирования");
                    return;
                }

                var oldFiltersToCopy = FilterDataGrid.SelectedItems.Cast<Filter>().ToList();
                var newFilters = new List<Filter>();

                foreach (var oldFilter in oldFiltersToCopy)
                {
                    var filter = new Filter
                    {
                        Id = Guid.NewGuid(),
                        Name = oldFilter.Name,
                        Type = oldFilter.Type,
                        DisplayName = oldFilter.DisplayName,
                        Order = oldFilter.Order,
                        Query = oldFilter.Query,
                        CreatedOn = DateTime.Now,
                        ModifiedOn = DateTime.Now,
                        CreatedBy = WindowsIdentity.GetCurrent().Name,
                        ModifiedBy = WindowsIdentity.GetCurrent().Name
                    };

                    _context.Filters.Add(filter);
                    _context.SaveChanges();

                    LoadFilterList();

                    newFilters.Add(filter);
                }

                MessageBox.Show("Копирование завершено успешно", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);

                if (newFilters.Any())
                    LoadFilterList(newFilters.First().Id);
                else
                    LoadFilterList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void AddFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filter = new Filter
                {
                    Id = Guid.NewGuid(),
                    Name = "Новый фильтр",
                    Type = "String",
                    CreatedOn = DateTime.Now,
                    ModifiedOn = DateTime.Now,
                    CreatedBy = WindowsIdentity.GetCurrent().Name,
                    ModifiedBy = WindowsIdentity.GetCurrent().Name,
                };

                _context.Filters.Add(filter);
                _context.SaveChanges();

                LoadFilterList(filter.Id);
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
                LoadFilterList(ImportExport.ImportFilters());
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
                if (FilterDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Необходимо выбрать параметр для экспорта");
                    return;
                }

                ImportExport.Export((Filter)FilterDataGrid.SelectedItem);

                LoadFilterList();
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
                ImportExport.Export(_context.Filters.ToList());

                LoadFilterList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void LoadFilterList(Guid? selectedFilterId = null)
        {
            IQueryable<Filter> filters;

            if (string.IsNullOrEmpty(SearchTextBox.Text))
                filters = from p in _context.Filters orderby p.Name select p;
            else
                filters = from p in _context.Filters where (p.Name.ToLower().Contains(SearchTextBox.Text.ToLower())) orderby p.Name select p;

            FilterDataGrid.ItemsSource = filters.ToList();

            if (selectedFilterId != null && selectedFilterId.HasValue)
            {
                var selectedItem = (from r in filters where r.Id == selectedFilterId.Value select r).FirstOrDefault();

                if (selectedItem != null)
                    FilterDataGrid.SelectedItem = selectedItem;
            }
        }

        private void EditFilterHandler()
        {
            if (FilterDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите фильтр для редактирования из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          ((((Parent as TabItem).Parent as TabControl).Parent as Grid).Parent as Page).NavigationService.Navigate(new EditReport((Filter)FilterDataGrid.SelectedItem));
        }
    }
}
