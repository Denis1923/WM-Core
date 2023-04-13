using System.Collections.ObjectModel;
using System.ComponentModel;
using Telerik.Windows.Data;
using WordMergeEngine.Assets.Enums;

namespace WordMergeEngine.Assets
{
    public class ConditionGroup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private GroupTypeEnum groupType;

        public GroupTypeEnum GroupType
        {
            get
            {
                return groupType;
            }
            set
            {
                if (groupType != value)
                {
                    groupType = value;
                    OnPropertyChanged("GroupType");
                }
            }
        }

        private IEnumerable<EnumMemberViewModel> operators;

        public IEnumerable<EnumMemberViewModel> Operators
        {
            get
            {
                if (operators == null)
                {
                    operators = EnumDataSource.FromType<OperatorEnum>();
                }

                return operators;
            }
        }

        public ObservableCollection<Condition> Conditions { get; set; } = new ObservableCollection<Condition>();
    }
}
