using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WordMergeEngine;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core.GlobalSource
{
    public partial class GlobalSourceManager : UserControl
    {
        private DataModel _context { get; set; }

        public GlobalSourceManager()
        {
            InitializeComponent();
            _context = ((App)Application.Current).GetDBContext;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDataSourceList();
        }

        private void EditSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditSourceHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void SourceDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditSourceHandler();
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
                LoadDataSourceList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SourceDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите источник из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Выбранный источник данных будет безвозвратно удален. Продолжить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                ReportExtension.DeleteAllDataSource((DataSource)SourceDataGrid.SelectedItem, _context);
                _context.SaveChanges();

                LoadDataSourceList();

                MessageBox.Show("Источник был успешно удален", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void CopySource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SourceDataGrid.SelectedItems == null || SourceDataGrid.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Необходимо выбрать источник для копирования");
                    return;
                }

                var oldSourcesToCopy = SourceDataGrid.SelectedItems.Cast<DataSource>().ToList();
                var newSources = new List<DataSource>();

                foreach (var oldSource in oldSourcesToCopy)
                {
                    var oldToNewMap = new Dictionary<Guid, DataSource>();
                    var start = true;
                    var oldSources = new List<DataSource> { oldSource };

                    DataSource root = null;

                    while (oldSources.Any())
                    {
                        var nextOldRoot = oldSources.FirstOrDefault();
                        DataSource nextNewRoot;

                        if (start)
                        {
                            root = ReportExtension.CreateNewSource(nextOldRoot, oldToNewMap);
                            oldToNewMap.Add(nextOldRoot.Datasourceid, root);
                            start = false;
                        }
                        else
                        {
                            nextNewRoot = ReportExtension.CreateNewSource(nextOldRoot, oldToNewMap);
                            oldToNewMap.Add(nextOldRoot.Datasourceid, nextNewRoot);
                        }

                        oldSources.AddRange(nextOldRoot.InverseParentDataSource);
                        oldSources.RemoveAt(0);
                    }

                    _context.DataSources.Add(root);
                    _context.SaveChanges();

                    newSources.Add(root);
                }

                MessageBox.Show("Копирование завершено успешно", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);

                if (newSources.Any())
                    LoadDataSourceList(newSources.First().Datasourceid);
                else
                    LoadDataSourceList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void AddSource_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataSource = new DataSource();
                dataSource.Datasourceid = Guid.NewGuid();
                dataSource.Datasourcename = "Новый источник данных";
                dataSource.Createdon = DateTime.Now;
                dataSource.Createdby = WindowsIdentity.GetCurrent().Name;
                dataSource.Modifiedon = DateTime.Now;
                dataSource.Modifiedby = WindowsIdentity.GetCurrent().Name;
                dataSource.Position = 0;
                dataSource.Isglobal = true;
                _context.DataSources.Add(dataSource);
                _context.SaveChanges();

                LoadDataSourceList(dataSource.Datasourceid);
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
                LoadDataSourceList(ImportExport.ImportDataSource());
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
                if (SourceDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Необходимо выбрать источник для экспорта");
                    return;
                }

                var dataSource = (DataSource)SourceDataGrid.SelectedItem;

                ExtractDataEngine.LoadNestedDataSources(dataSource);

                ImportExport.Export(dataSource);

                LoadDataSourceList();
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
                var dataSources = _context.DataSources.Where(d => d.Isglobal == true).ToList();

                dataSources.ForEach(ExtractDataEngine.LoadNestedDataSources);

                ImportExport.Export(dataSources);

                LoadDataSourceList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void LoadDataSourceList(Guid? selectedSourceId = null)
        {
            IQueryable<DataSource> dataSources;

            if (string.IsNullOrEmpty(SearchTextBox.Text))
                dataSources = from p in _context.DataSources where (bool)p.Isglobal orderby p.Datasourcename select p;
            else
            {
                var searchCriteria = SearchTextBox.Text;
                dataSources = from p in _context.DataSources where (bool)p.Isglobal && (p.Datasourcename.ToLower().Contains(searchCriteria.ToLower()) || p.Keyfieldname.ToLower().Contains(searchCriteria.ToLower())) orderby p.Datasourcename select p;
            }

            SourceDataGrid.ItemsSource = dataSources.ToList();

            if (selectedSourceId != null && selectedSourceId.HasValue)
            {
                var selectedItem = (from r in dataSources where r.Datasourceid == selectedSourceId.Value select r).FirstOrDefault();

                if (selectedItem != null)
                    SourceDataGrid.SelectedItem = selectedItem;
            }
        }

        private void EditSourceHandler()
        {
            if (SourceDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите источник для редактирования из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          ((((Parent as TabItem).Parent as TabControl).Parent as Grid).Parent as Page).NavigationService.Navigate(new EditReport((DataSource)SourceDataGrid.SelectedItem));
        }
    }
}
