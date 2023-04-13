using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using WordMergeEngine.Models;
using WordMergeEngine;

using RReport = WordMergeEngine.Models.Report;

namespace WordMergeUtil_Core.Assets
{
    public static class ReportExtension
    {
        public static void DeleteReportCascade(this RReport report, DataModel context)
        {
            try
            {
                ExtractDataEngine.Loader(report);

                if (report.Datasource != null && report.Datasource.Isglobal != true)
                    DeleteAllDataSource(report.Datasource, context);

                foreach (var reportCondition in report.ReportConditions.ToList())
                {
                    if (reportCondition.Condition.Isglobal != true)
                        context.Conditions.Remove(reportCondition.Condition);

                    context.ReportConditions.Remove(reportCondition);
                }

                foreach (var reportParameter in report.ReportParameters.ToList())
                {
                    if (reportParameter.Parameter.Isglobal != true)
                    {
                        foreach (var pc in reportParameter.Parameter.ParameterConditions.ToList())
                            context.ParameterConditions.Remove(pc);

                        context.Parameters.Remove(reportParameter.Parameter);
                    }

                    context.ReportParameters.Remove(reportParameter);
                }

                foreach (var reportFilter in report.ReportFilters.ToList())
                {
                    if (reportFilter.Filter.ReportFilters.Count == 1)
                        context.Filters.Remove(reportFilter.Filter);

                    context.ReportFilters.Remove(reportFilter);
                }

                foreach (var integration in report.Integrations.ToList())
                    context.Integrations.Remove(integration);

                context.Reports.Remove(report);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при удалении отчёта:\n{ex.Message}", ex);
            }
        }

        public static void DeleteAllDataSource(DataSource dataSource, DataModel context)
        {
            if (dataSource.InverseParentDataSource.Any())
            {
                foreach (var nestedDataSource in dataSource.InverseParentDataSource.ToList())
                    DeleteAllDataSource(nestedDataSource, context);
            }

            context.DataSources.Remove(dataSource);
            context.SaveChanges();
        }

        public static DataSource CreateNewSource(DataSource oldSource, Dictionary<Guid, DataSource> oldToNewMap, RReport newReport = null)
        {
            var dataSource = new DataSource
            {
                Assingleline = oldSource.Assingleline,
                Dataquery = oldSource.Dataquery,
                Datasourceid = Guid.NewGuid(),
                Datasourcename = oldSource.Datasourcename,
                Foreignkeyfieldname = oldSource.Foreignkeyfieldname,
                Keyfieldname = oldSource.Keyfieldname,
                Position = oldSource.Position,
                Isglobal = oldSource.Isglobal,
                Createdon = DateTime.Now,
                Createdby = WindowsIdentity.GetCurrent().Name,
                Modifiedon = DateTime.Now,
                Modifiedby = WindowsIdentity.GetCurrent().Name
            };

            if (oldSource.ParentDataSource != null)
            {
                dataSource.ParentDataSource = oldToNewMap[oldSource.ParentDataSource.Datasourceid];
                oldToNewMap[oldSource.ParentDataSource.Datasourceid].InverseParentDataSource.Add(dataSource);
            }

            if (newReport != null)
                newReport.Datasource = dataSource;

            return dataSource;
        }

        public static List<Guid> DeleteDocumentCascade(this Document document, DataModel context)
        {
            try
            {
                ExtractDataEngine.Loader(document);

                var result = document.Reports.Select(x => x.Reportid).ToList();

                foreach (var docContent in document.DocumentContents.ToList())
                {
                    foreach (var paragraph in docContent.Paragraphs.ToList())
                    {
                        foreach (var content in paragraph.ParagraphContents.ToList())
                            context.ParagraphContents.Remove(content);

                        context.Paragraphs.Remove(paragraph);
                    }

                    context.DocumentContents.Remove(docContent);
                }

                context.Documents.Remove(document);
                context.SaveChanges();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при удалении шаблона согласования:\n{ex.Message}", ex);
            }
        }
    }
}
