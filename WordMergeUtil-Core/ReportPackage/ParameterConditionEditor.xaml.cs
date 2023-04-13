using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WordMergeEngine.Models;
using WordMergeEngine;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core
{
    public partial class ParameterConditionEditor : Window
    {
        public Parameter Parameter { get; }

        public ParameterCondition CurrentCondition { get; set; }

        private DbConnection _connection { get; set; }

        private object _rowId { get; set; }

        private Guid _userId { get; set; }

        public ParameterConditionEditor(Parameter parameter, DbConnection connection, object rowId, Guid userId)
        {
            InitializeComponent();
            Parameter = parameter;
            _connection = connection;
            _rowId = rowId;
            _userId = userId;

            CusomizeSQLEditor(SQLConditionQueryEditor);

            ReloadConditions();
        }

        private void AddParamCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var condition = new ParameterCondition();
                condition.Id = Guid.NewGuid();
                condition.Name = "Новая проверка";
                condition.ConditionOperator = "=";
                condition.Query = "select 1 as pk";
                condition.RecordCount = 1;

                var context = ((App)Application.Current).GetDBContext;
                context.SaveChanges();

                Parameter.ParameterConditions.Add(condition);
                ReloadConditions();
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

        private void RemoveParamCondition_Click(object sender, RoutedEventArgs e)
        {
            if (ParamConditionsListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите условие, чтобы удалить его", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            if (MessageBox.Show("Удалить условие?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var context = ((App)Application.Current).GetDBContext;
                var parameterCondition = (ParameterCondition)ParamConditionsListBox.SelectedItem;
                ParamConditionsListBox.SelectedItem = null;

                context.ParameterConditions.Remove(parameterCondition);

                context.SaveChanges();

                ParamConditionGrid.DataContext = null;

                ReloadConditions();
            }
        }

        private void ParamConditionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ParamConditionGrid.DataContext is ParameterCondition)
                {
                    var condition = ParamConditionGrid.DataContext as ParameterCondition;

                    if (SQLConditionQueryEditor.Text != null)
                        condition.Query = SQLConditionQueryEditor.Text;
                }

                ConditionSelect(ParamConditionsListBox.SelectedItem as ParameterCondition);
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

        private void ConditionSelect(ParameterCondition condition = null)
        {
            CurrentCondition = condition;

            if (condition != null)
            {
                ParamConditionGrid.DataContext = condition;
                SQLConditionQueryEditor.Text = condition.Query;
                SQLConditionQueryEditor.IsEnabled = true;
                RepParamNameTextBox.IsEnabled = true;
                RecordCountTextBox.IsEnabled = true;
                SQLConditionQueryEditor.Select(0, 0);
            }
        }

        private void ReloadConditions(ReportCondition rc = null)
        {
            if (Parameter == null)
                return;

            var conditions = Parameter.ParameterConditions.ToList();
            ParamConditionsListBox.ItemsSource = conditions;
            ParamConditionsListBox.DisplayMemberPath = "Name";
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

        private void SaveData(bool isSave)
        {
            if (ParamConditionsListBox.SelectedItem != null)
            {
                var cond = ParamConditionsListBox.SelectedItem as ParameterCondition;
                cond.Query = SQLConditionQueryEditor.Text;
            }

            try
            {
                if (_connection != null)
                    ExtractDataEngine.CheckParameterCondition(_connection, CurrentCondition, _rowId, _userId);

                var ctx = ((App)Application.Current).GetDBContext;
                ctx.SaveChanges();
            }
            catch (Exception ex)
            {
                if (isSave)
                    MessageBox.Show($"Ошибка при проверке и сохранении: {ex.Message}");
            }
        }

        private void SaveParam_Click(object sender, RoutedEventArgs e)
        {
            SaveData(true);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveData(false);
        }
    }
}
