using System;
using System.Collections.ObjectModel;
using System.Windows;
using Telerik.Windows.Controls;
using WordMergeEngine.Assets.Enums;
using WordMergeEngine.Assets;
using WordMergeEngine.Helpers;
using WordMergeUtil_Core.AgreementTool.DataContracts;

namespace WordMergeUtil_Core.AgreementTool
{
    public partial class ConditionWindow : Window
    {
        public ConditionWindow(ObservableCollection<ConditionGroup> conditions, DateTime? activeFrom, DateTime? activeTill)
        {
            var dataModel = new DocumentDataModel(conditions);
            this.DataContext = dataModel;
            InitializeComponent();
            this.dateFrom.SelectedDate = activeFrom;
            this.dateTo.SelectedDate = activeTill;

            LogHelper.Log(GetType().Name);
        }

        public DateTime? ActiveFrom
        {
            get
            {
                return this.dateFrom.SelectedDate;
            }
        }

        public DateTime? ActiveTill
        {
            get
            {
                return this.dateTo.SelectedDate;
            }
        }

        private void RadGridView_CopyingCellClipboardContent(object sender, GridViewCellClipboardEventArgs e)
        {
            if (e.Cell.Column.UniqueName != "GroupType")
                return;

            foreach (var item in (e.Cell.Item as ConditionGroup).Conditions)
            {
                e.Value = $"{e.Value};{$"{item.Variable},{(int)item.Operator},{item.Value}"}";
            }
        }

        private void RadGridView_PastingCellClipboardContent(object sender, GridViewCellClipboardEventArgs e)
        {
            if (!e.Value.ToString().Contains(";"))
                return;

            var values = e.Value.ToString().Split(';');

            var group = e.Cell.Item as ConditionGroup;

            group.Conditions.Clear();

            for (var i = 1; i < values.Length; i++)
            {
                var cells = values[i].Split(',');

                var condition = new WordMergeEngine.Assets.Condition
                {
                    Variable = cells[0],
                    Operator = (OperatorEnum)int.Parse(cells[1]),
                    Value = cells[2]
                };

                group.Conditions.Add(condition);
            }

            e.Value = values[0];
        }
    }
}
