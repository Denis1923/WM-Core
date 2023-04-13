namespace WordMergerUI.Models
{
    public class WmDataViewModel
    {
        public List<TempContainer> LstView { get; set; }

        public ReportParameter[] ReportParams { get; set; }

        public List<FilterModel> Filters { get; set; } = new List<FilterModel>();

        public string ID { get; set; }

        public string SaveFormat { get; set; }

        public bool EnableIgnoreCondition { get; set; }

        public bool IsIgnoreCondition { get; set; }

        public WmDataViewModel()
            : this(null, null)
        {
        }

        public WmDataViewModel(IEnumerable<TempContainer> items)
            : this(items, null)
        {
        }

        public WmDataViewModel(IEnumerable<TempContainer> items, IEnumerable<ReportParameter> repParams)
        {
            LstView = items == null ? new List<TempContainer>() : items.ToList();
            ReportParams = repParams == null ? new ReportParameter[0] : repParams.ToArray();
        }
    }
}
