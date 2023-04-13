using System;

namespace WordMergeService.DataContracts.Requests
{
    public class MergingParametrizedDocumentRequest
    {
        public string ReportCode { get; set; }
        public Guid RowId { get; set; }
        public LabelValueEntry[] ParamValues { get; set; }
        public string ChooseFormat { get; set; } = null;
        public bool IsIgnoreCondition { get; set; }
    }
}