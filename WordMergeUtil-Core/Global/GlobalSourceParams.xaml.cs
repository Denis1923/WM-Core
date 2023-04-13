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
    public partial class GlobalSourceParams : UserControl
    {
        private DataModel _context { get; set; }

        public GlobalSourceParams()
        {
            InitializeComponent();
            _context = ((App)Application.Current).GetDBContext;
        }

        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            LoadParameterList();
        }

        private void EditParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditParameterHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ParameterDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditParameterHandler();
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
                LoadParameterList();
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
                if (ParameterDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Выберите параметр из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var parameter = (Parameter)ParameterDataGrid.SelectedItem;

                if (MessageBox.Show("Выбранный параметр будет безвозвратно удален. Продолжить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;
                
                foreach (var reportParameter in parameter.ReportParameters)
                    _context.ReportParameters.Remove(reportParameter);

                _context.SaveChanges();
                _context.Parameters.Remove(parameter);
                _context.SaveChanges();

                LoadParameterList();

                MessageBox.Show("Параметр был успешно удален", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void CopyParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ParameterDataGrid.SelectedItems == null || ParameterDataGrid.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Необходимо выбрать параметр для копирования");
                    return;
                }

                var oldParametersToCopy = ParameterDataGrid.SelectedItems.Cast<Parameter>().ToList();
                var newParameters = new List<Parameter>();

                foreach (var oldParameter in oldParametersToCopy)
                {
                    var parameter = new Parameter
                    {
                        Id = Guid.NewGuid(),
                        Name = oldParameter.Name,
                        Datatype = oldParameter.Datatype,
                        Displayname = oldParameter.Displayname,
                        Displayorder = oldParameter.Displayorder,
                        Nullable = oldParameter.Nullable,
                        Query = oldParameter.Query,
                        Testval = oldParameter.Testval,
                        Errormessage = oldParameter.Errormessage,
                        Isglobal = oldParameter.Isglobal,
                        Createdon = DateTime.Now,
                        Modifiedon = DateTime.Now,
                        Createdby = WindowsIdentity.GetCurrent().Name,
                        Modifiedby = WindowsIdentity.GetCurrent().Name,
                    };

                    _context.Parameters.Add(parameter);
                    _context.SaveChanges();

                    LoadParameterList();

                    newParameters.Add(parameter);
                }

                MessageBox.Show("Копирование завершено успешно", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);

                if (newParameters.Any())
                    LoadParameterList(newParameters.First().Id);
                else
                    LoadParameterList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void AddParameter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parameter = new Parameter
                {
                    Id = Guid.NewGuid(),
                    Name = "Новый параметр",
                    Datatype = "String",
                    Isglobal = true,
                    Createdon = DateTime.Now,
                    Modifiedon = DateTime.Now,
                    Createdby = WindowsIdentity.GetCurrent().Name,
                    Modifiedby = WindowsIdentity.GetCurrent().Name,
                };

                _context.Parameters.Add(parameter);
                _context.SaveChanges();
                
                LoadParameterList(parameter.Id);
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
                LoadParameterList(ImportExport.ImportParameters());
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
                if (ParameterDataGrid.SelectedItem == null)
                {
                    MessageBox.Show("Необходимо выбрать параметр для экспорта");
                    return;
                }

                ImportExport.Export((Parameter)ParameterDataGrid.SelectedItem);

                LoadParameterList();
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
                ImportExport.Export(_context.Parameters.Where(d => d.Isglobal == true).ToList());

                LoadParameterList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void LoadParameterList(Guid? selectedParameterId = null)
        {
            IQueryable<Parameter> parameters;

            if (string.IsNullOrEmpty(SearchTextBox.Text))
                parameters = from p in _context.Parameters where (bool)p.Isglobal orderby p.Name select p;
            else
                parameters = from p in _context.Parameters where (bool)p.Isglobal && (p.Name.ToLower().Contains(SearchTextBox.Text.ToLower())) orderby p.Name select p;

            ParameterDataGrid.ItemsSource = parameters.ToList();

            if (selectedParameterId != null && selectedParameterId.HasValue)
            {
                var selectedItem = (from r in parameters where r.Id == selectedParameterId.Value select r).FirstOrDefault();

                if (selectedItem != null)
                    ParameterDataGrid.SelectedItem = selectedItem;
            }
        }

        private void EditParameterHandler()
        {
            if (ParameterDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите параметр для редактирования из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          ((((Parent as TabItem).Parent as TabControl).Parent as Grid).Parent as Page).NavigationService.Navigate(new EditReport((Parameter)ParameterDataGrid.SelectedItem));
        }
    }
}
