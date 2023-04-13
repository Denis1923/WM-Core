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
    public partial class FilterVisibleEditor : Window
    {
        public Filter Filter { get; }

        private string _typeTemplate = "({0})";

        public ParameterCondition CurrentCondition { get; set; }

        public FilterVisibleEditor(Filter filter, bool isEnabled)
        {
            InitializeComponent();
            Filter = filter;

            DataContext = Filter;

            ReloadParentValues();

            CusomizeSQLEditor(SQLVisibleQueryEditor);

            SQLVisibleQueryEditor.Text = Filter.VisibleQuery;

            var context = ((App)Application.Current).GetDBContext;
            var filtersMap = new Dictionary<Guid, string>();
            var parentFilters = context.Filter.Where(x => x.Id != Filter.Id && (x.ParentFilterId == null || x.ParentFilterId != Filter.Id)).ToList();

            foreach (var item in parentFilters)
                filtersMap.Add(item.Id, item.Name);

            ParentFilter.ItemsSource = filtersMap;
            ParentFilter.SelectedValue = Filter.ParentFilterId;

            SQLVisibleQueryEditor.IsEnabled = isEnabled;
            ConditionOperatorComboBox.IsEnabled = isEnabled;
            RecordCountTextBox.IsEnabled = isEnabled;
            ParentFilter.IsEnabled = isEnabled;
            ParentOperatorComboBox.IsEnabled = isEnabled;
            ParentValueToolBar.IsEnabled = isEnabled;
            toolBarFilter.IsEnabled = isEnabled;
            ParentValueListBox.IsEnabled = isEnabled;
            ParentValueTextBox.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            ParentValueLabel.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;
            ParentTypeLabel.Visibility = isEnabled ? Visibility.Visible : Visibility.Collapsed;

            SetParentTypeLabel();
        }

        private void CusomizeSQLEditor(TextEditor editor)
        {
            editor.Options.EnableEmailHyperlinks = true;
            editor.Options.ConvertTabsToSpaces = true;
            editor.ShowLineNumbers = false;

            var scheme = new MemoryStream((byte[])Properties.Resources.ResourceManager.GetObject("tsql"));

            using (XmlTextReader reader = new XmlTextReader(scheme))
                editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

            editor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(editor.TextArea));
        }

        private void SaveData()
        {
            Filter.VisibleQuery = SQLVisibleQueryEditor.Text;

            var ctx = ((App)Application.Current).GetDBContext;
            ctx.SaveChanges();
        }

        private void SaveParam_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void AddParentValueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Filter.ParentFilterId == null)
                    return;

                var context = ((App)Application.Current).GetDBContext;

                var newValue = new FilterParentFilter
                {
                    Id = Guid.NewGuid(),
                    ChildFilter = Filter,
                    ParentFilter = context.Filter.FirstOrDefault(x => x.Id == Filter.ParentFilterId),
                    Value = "-"
                };

                Filter.ParentFilters.Add(newValue);

                ReloadParentValues();

                ParentValueListBox.SelectedItem = newValue;
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveParentValueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ParentValueListBox.SelectedItem == null)
                    return;

                if (MessageBox.Show("Вы уверены что хотите удалить значение?", "Вы уверены?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;

                var ctx = ((App)Application.Current).GetDBContext;
                ctx.FilterParentFilter.Remove((FilterParentFilter)ParentValueListBox.SelectedItem);

                ReloadParentValues();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }


        private void ReloadParentValues()
        {
            ParentValueListBox.ItemsSource = Filter.ParentFilters.Where(x => x.ParentFilter.Id == Filter.ParentFilterId).ToList();
            ParentValueListBox.DisplayMemberPath = "Value";
        }

        private void ParentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadParentValues();
            SetParentTypeLabel();
        }

        private void ParentValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ParentValueListBox.SelectedItem == null)
                return;

            var filter = ParentValueListBox.SelectedItem as FilterParentFilter;
            filter.Value = ((TextBox)sender).Text;

            ReloadParentValues();
        }

        private void ParentValueListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ParentValueTextBox.Text = null;

            if (ParentValueListBox.SelectedItem == null)
                return;

            var filter = ParentValueListBox.SelectedItem as FilterParentFilter;
            ParentValueTextBox.Text = filter.Value;
        }

        private void ClearParentFilter_Click(object sender, RoutedEventArgs e)
        {
            ParentFilter.SelectedItem = null;
            ReloadParentValues();
            SetParentTypeLabel();
        }

        private void SetParentTypeLabel()
        {
            var context = ((App)Application.Current).GetDBContext;

            ParentTypeLabel.Content = Filter.ParentFilterId != null ? string.Format(_typeTemplate, context.Filter.FirstOrDefault(x => x.Id == Filter.ParentFilterId)?.Type) : string.Empty;
        }
    }
}
