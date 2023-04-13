using System.Collections.Generic;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using WordMergeEngine.Assets.Enums;
using WordMergeEngine.Assets;
using Telerik.Windows.Controls;

namespace WordMergeUtil_Core.AgreementTool.DataContracts
{
    public class DocumentDataModel : ViewModelBase
    {
        private IEnumerable<EnumMemberViewModel> groupTypes;

        public IEnumerable<EnumMemberViewModel> GroupTypes
        {
            get
            {
                if (groupTypes == null)
                {
                    groupTypes = EnumDataSource.FromType<GroupTypeEnum>();
                }

                return groupTypes;
            }
        }

        public ObservableCollection<ConditionGroup> ConditionGroups { get; }

        public DocumentDataModel(ObservableCollection<ConditionGroup> conditionGroups)
        {
            this.ConditionGroups = conditionGroups;
        }
    }
}
