using System;

namespace WordMergeService.DataContracts
{
    internal class ConditionReport
    {
        public Guid ConditionId { get; set; }

        public Guid ReportId { get; set; }

        public string ServerName { get; set; }

        public string DBName { get; set; }

        public bool GlobalCondition { get; set; }

        public string ConditionName { get; set; }

        public string ConditionQuery { get; set; }

        public string ConditionOperator { get; set; }

        public decimal ConditionCount { get; set; }

        public int ConditionOrder { get; set; }

        public string ConditionMessage { get; set; }

        public ConditionReport(WordMergeEngine.Models.Condition condition, WordMergeEngine.Models.Report report)
        {
            ConditionId = condition.conditionid;
            ReportId = report.reportid;
            ServerName = report.servername;
            DBName = report.defaultdatabase;
            GlobalCondition = condition.isglobal == true;
            ConditionName = condition.conditionname;
            ConditionQuery = condition.dataquery;
            ConditionOperator = condition.conditionoperator;
            ConditionCount = condition.recordcount ?? 0;
            ConditionOrder = condition.OrderNo ?? int.MaxValue;
            ConditionMessage = condition.errormessage;
        }
    }
}