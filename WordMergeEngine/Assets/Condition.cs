using System.ComponentModel;
using WordMergeEngine.Assets.Enums;

namespace WordMergeEngine.Assets
{
    public class Condition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string variable;

        public string Variable
        {
            get
            {
                return variable;
            }
            set
            {
                if (value != variable)
                {
                    variable = value;
                    OnPropertyChanged("Variable");
                }
            }
        }

        private OperatorEnum operatorEnum;

        public OperatorEnum Operator
        {
            get
            {
                return operatorEnum;
            }
            set
            {
                if (operatorEnum != value)
                {
                    operatorEnum = value;
                    OnPropertyChanged("Operator");
                }
            }
        }

        public string ConditionOperator
        {
            get
            {
                switch (operatorEnum)
                {
                    case OperatorEnum.Equal:
                        return "=";

                    case OperatorEnum.NotEqual:
                        return "!=";

                    default:
                        return string.Empty;
                }
            }
            set
            {
                switch (value)
                {
                    case "=":
                        operatorEnum = OperatorEnum.Equal;
                        break;

                    case "!=":
                        operatorEnum = OperatorEnum.NotEqual;
                        break;

                    default:
                        break;
                }
                OnPropertyChanged("Operator");
            }
        }

        private string value;

        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }
    }
}
