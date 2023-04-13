using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Report;
using RReport = WordMergeEngine.Models.Report;

namespace WordMergeUtil_Core.Assets
{
    public class ImportExport
    {
        public static void Export(RReport reportToExport)
        {
            var reports = new List<RReport>();
            reports.Add(reportToExport);
            Export(reports);
        }

        public static void Export(List<RReport> reports, DataModel context = null)
        {
            try
            {
                LogHelper.Log("ImportExport", $"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reports)} = {reports?.Count})");

                if (context == null)
                    context = ((App)Application.Current).GetDBContext;

                context.Configuration.ProxyCreationEnabled = false; // System.Data.Entity

                var selectedReports = new List<RReport>();
                var customReports = reports.Where(r => r.IsCustomTemplate && r.Document != null).ToList();
                selectedReports.AddRange(reports.Except(customReports));
                if (customReports.Any())
                {
                    var documents = customReports.Select(r => r.Document).Distinct().ToList();

                    if (documents.Any(x => x.Reports.Except(customReports).Any()))
                    {
                        var dialog = new SelectTemplateWindow(customReports, documents);

                        dialog.ShowDialog();

                        if (dialog.DialogResult == false)
                        {
                            context.Configuration.ProxyCreationEnabled = true;

                            return;
                        }

                        var ids = dialog.SelectedReports.Where(x => x.IsChecked).Select(x => x.Id);

                        selectedReports.AddRange(context.Reports.Where(x => ids.Contains(x.Reportid)));
                    }
                    else
                    {
                        selectedReports.AddRange(customReports);
                    }
                }

                var dlg = new SaveFileDialog(); //Microsoft.Win32

                var filename = string.Empty;
                var allReportsCount = context.Reports.Count();

                if (selectedReports.Count == 1)
                    filename = $"{selectedReports.First().Reportname}_{DateTime.Now.ToShortDateString()}";
                else if (selectedReports.Count == allReportsCount)
                    filename = $"ALL_REPORTS_{DateTime.Now.ToShortDateString()}";
                else
                    filename = $"REPORTS_{DateTime.Now.ToShortDateString()}";

                filename = Helper.ReplacingIllegalCharacters(filename);

                dlg.FileName = filename;
                dlg.DefaultExt = ".xml";
                dlg.Filter = "XML files (.xml)|*.xml";

                var result = dlg.ShowDialog();

                if (result == false)
                {
                    context.Configuration.ProxyCreationEnabled = true;

                    return;
                }

                var xmlReportCollection = new List<XmlReport>();
                var xmlDocuments = new List<Document>();
                var xmlDocumentCollection = new List<XmlDocument>();

                foreach (var proxyReport in selectedReports)
                {
                    LogHelper.Log("ImportExport", $"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(proxyReport)} = {proxyReport?.Reportname})");

                    var report = context.Reports
                                        .Include("DataSource")
                                        .Include("DataSource.NestedDataSources")
                                        .Include("ReportCondition")
                                        .Include("ReportCondition.Condition")
                                        .Include("ReportParameter")
                                        .Include("ReportParameter.Parameter")
                                        .AsNoTracking()
                                        .FirstOrDefault(r => r.Reportid == proxyReport.Reportid);

                    if (report.Document != null && !xmlDocuments.Any(d => d.Id == report.Document.Id))
                    {
                        xmlDocuments.Add(report.Document);
                    }

                    var sources = new List<DataSource> { report.Datasource };

                    if (report.Datasource != null)
                        GetDataSources(context, sources, report.Datasource.InverseParentDataSource);

                    xmlReportCollection.Add(new XmlReport
                    {
                        Report = report,
                        DataSources = sources,
                        Conditions = report.ReportConditions.Select(x => x.Condition).Where(c => c.ReportPackageEntry == null).ToList(),
                        Parameters = report.ReportParameters.Select(x => x.Parameter).ToList()
                    });
                }

                foreach (var proxyDocument in xmlDocuments)
                {
                    var document = context.Documents
                                          .Include("DocumentContent")
                                          .Include("Paragraph")
                                          .Include("Paragraph.ParagraphContent")
                                          .AsNoTracking()
                                          .FirstOrDefault(d => d.Id == proxyDocument.Id);

                    xmlDocumentCollection.Add(new XmlDocument
                    {
                        Document = document,
                        DocumentContents = document.DocumentContents.ToList(),
                        Paragraphs = document.Paragraph.ToList(),
                        ParagraphContents = document.Paragraph.SelectMany(p => p.ParagraphContents).ToList()
                    });
                }

                var xmlReport = new ReportXmlWrapper
                {
                    XmlReports = xmlReportCollection,
                    XmlDocuments = xmlDocumentCollection,
                    ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                };

                var writer = new XmlSerializer(typeof(ReportXmlWrapper));

                var file = new StreamWriter(dlg.FileName);

                writer.Serialize(file, xmlReport);
                file.Close();

                context.Configuration.ProxyCreationEnabled = true;

                LogHelper.Log("ImportExport", $"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reports)} = {reports?.Count}) отработал.");

                MessageBox.Show("Экспорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                context.Configuration.ProxyCreationEnabled = true;
                LogHelper.Log("ImportExport", $"Ошибка: {ex?.Message}");
                throw;
            }
        }

        public static void Export(DataSource source)
        {
            Export(new List<DataSource> { source });
        }

        public static void Export(List<DataSource> datasources)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var filename = datasources.Count == 1 ? $"{datasources.First().Datasourcename}_{DateTime.Now.ToShortDateString()}" : $"ALL_DATASOURCES_{DateTime.Now.ToShortDateString()}";

            filename = Helper.ReplacingIllegalCharacters(filename);

            dialog.FileName = filename;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                ExportSource(datasources, dialog.FileName);
                MessageBox.Show("Экспорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static void ExportSource(List<DataSource> dataSources, string path)
        {
            var xmlSourcesCollection = new List<XmlDataSource>();

            var context = ((App)Application.Current).GetDBContext;
            context.Configuration.ProxyCreationEnabled = false;

            foreach (var proxyDataSource in dataSources)
            {
                var dataSource = context.DataSources.AsNoTracking().Include("NestedDataSources").FirstOrDefault(ds => ds.Datasourceid == proxyDataSource.Datasourceid);

                var sources = new List<DataSource> { dataSource };
                GetDataSources(context, sources, dataSource.InverseParentDataSource);
                xmlSourcesCollection.Add(new XmlDataSource { DataSources = sources });
            }

            var xmlDataSources = new DataSourceXmlWrapper
            {
                XmlDataSources = xmlSourcesCollection,
                ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };

            var writer = new XmlSerializer(typeof(DataSourceXmlWrapper));
            var file = new StreamWriter(path);

            writer.Serialize(file, xmlDataSources);

            file.Close();

            context.Configuration.ProxyCreationEnabled = true;
        }

        public static void Export(WordMergeEngine.Models.Condition condition)
        {
            Export(new List<WordMergeEngine.Models.Condition> { condition });
        }

        public static void Export(List<WordMergeEngine.Models.Condition> conditions)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var filename = conditions.Count == 1 ? $"{conditions.First().Conditionname}_{DateTime.Now.ToShortDateString()}" : $"ALL_CONDITIONS_{DateTime.Now.ToShortDateString()}";
            filename = Helper.ReplacingIllegalCharacters(filename);

            dialog.FileName = filename;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                ExportCondition(conditions, dialog.FileName);
                MessageBox.Show("Экспорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static void ExportCondition(List<WordMergeEngine.Models.Condition> conditions, string path)
        {
            var xmlCondCollection = new List<XmlCondition>();

            var context = ((App)Application.Current).GetDBContext;
            context.Configuration.ProxyCreationEnabled = false;

            foreach (var proxyCondition in conditions)
            {
                var condition = context.Conditions.AsNoTracking().FirstOrDefault(c => c.Conditionid == proxyCondition.Conditionid);
                xmlCondCollection.Add(new XmlCondition { Condition = condition });
            }

            var xmlConditions = new ConditionXmlWrapper
            {
                XmlConditions = xmlCondCollection,
                ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
            };

            var writer = new XmlSerializer(typeof(ConditionXmlWrapper));
            var file = new StreamWriter(path);

            writer.Serialize(file, xmlConditions);

            file.Close();

            context.Configuration.ProxyCreationEnabled = true;
        }

        public static void Export(Parameter parameter)
        {
            var parameters = new List<Parameter>();
            parameters.Add(parameter);
            Export(parameters);
        }

        public static void Export(List<Parameter> parameters)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var filename = parameters.Count == 1 ? $"{parameters.First().Name}_{DateTime.Now.ToShortDateString()}" : $"ALL_PARAMETERS_{DateTime.Now.ToShortDateString()}";
            filename = Helper.ReplacingIllegalCharacters(filename);

            dialog.FileName = filename;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var context = ((App)Application.Current).GetDBContext;
                context.Configuration.ProxyCreationEnabled = false;

                var xmlParamCollection = new List<XmlParameter>();

                foreach (var proxyParameter in parameters)
                {
                    var parameter = context.Parameters.AsNoTracking().FirstOrDefault(p => p.Id == proxyParameter.Id);

                    xmlParamCollection.Add(new XmlParameter { Parameter = parameter });
                }

                var xmlParameters = new ParameterXmlWrapper
                {
                    XmlParameters = xmlParamCollection,
                    ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                };

                var writer = new XmlSerializer(typeof(ParameterXmlWrapper));
                var file = new StreamWriter(dialog.FileName);

                writer.Serialize(file, xmlParameters);

                file.Close();

                context.Configuration.ProxyCreationEnabled = true;

                MessageBox.Show("Экспорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static void Export(Document document)
        {
            var documents = new List<Document>();
            documents.Add(document);
            Export(documents);
        }

        public static void Export(List<Document> documents)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var filename = documents.Count == 1 ? $"{documents.First().Name}_{DateTime.Now.ToShortDateString()}" : $"ALL_AGREEMENT_TEMPLATES_{DateTime.Now.ToShortDateString()}";
            filename = Helper.ReplacingIllegalCharacters(filename);

            dialog.FileName = filename;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var context = ((App)Application.Current).GetDBContext;
                context.Configuration.ProxyCreationEnabled = false;

                var xmlDocumentCollection = new List<XmlDocument>();

                foreach (var proxyDocument in documents)
                {
                    var document = context.Documents.AsNoTracking().Include("DocumentContent")
                                                                  .Include("Paragraph")
                                                                  .Include("Paragraph.ParagraphContent")
                                                                  .FirstOrDefault(d => d.Id == proxyDocument.Id);

                    xmlDocumentCollection.Add(new XmlDocument
                    {
                        Document = document,
                        DocumentContents = document.DocumentContents.ToList(),
                        Paragraphs = document.Paragraph.ToList(),
                        ParagraphContents = document.Paragraph.SelectMany(x => x.ParagraphContents).ToList()
                    });
                }

                var xmlDocuments = new DocumentXmlWrapper
                {
                    XmlDocuments = xmlDocumentCollection,
                    ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                };

                var writer = new XmlSerializer(typeof(DocumentXmlWrapper));
                var file = new StreamWriter(dialog.FileName);

                writer.Serialize(file, xmlDocuments);

                file.Close();

                context.Configuration.ProxyCreationEnabled = true;

                MessageBox.Show("Экспорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static void Export(Filter filter)
        {
            var filters = new List<Filter>();
            filters.Add(filter);
            Export(filters);
        }

        public static void Export(List<Filter> filters)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var filename = filters.Count == 1 ? $"{filters.First().Name}_{DateTime.Now.ToShortDateString()}" : $"ALL_FILTERS_{DateTime.Now.ToShortDateString()}";
            filename = Helper.ReplacingIllegalCharacters(filename);

            dialog.FileName = filename;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var context = ((App)Application.Current).GetDBContext;
                context.Configuration.ProxyCreationEnabled = false;

                var xmlFilterCollection = new List<XmlFilter>();

                foreach (var proxyFilter in filters)
                {
                    var filter = context.Filters.AsNoTracking().FirstOrDefault(f => f.Id == proxyFilter.Id);
                    xmlFilterCollection.Add(new XmlFilter { Filter = filter });
                }

                var xmlFilters = new FilterXmlWrapper
                {
                    XmlFilters = xmlFilterCollection,
                    ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                };

                var writer = new XmlSerializer(typeof(ParameterXmlWrapper));
                var file = new StreamWriter(dialog.FileName);

                writer.Serialize(file, xmlFilters);

                file.Close();

                context.Configuration.ProxyCreationEnabled = true;

                MessageBox.Show("Экспорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static Guid? Import()
        {
            Guid? importReportId = null;

            var dlg = new OpenFileDialog //Microsoft.Win32
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var result = dlg.ShowDialog();

            if (result == true)
            {
                var context = ((App)Application.Current).GetDBContext;

                var reader = new XmlSerializer(typeof(ReportXmlWrapper));

                var file = new StreamReader(dlg.FileName);

                var rep = (ReportXmlWrapper)reader.Deserialize(file);

                var reportPkgEntries = context.ReportPackageEntries.Select(x => x.ReportReport.Reportid).ToList();

                var setting = context.GlobalSettings.FirstOrDefault();

                foreach (var d in rep.XmlDocuments)
                {
                    var exDocument = context.Documents.Include("Report").FirstOrDefault(x => x.Id == d.Document.Id);

                    var reportIds = new List<Guid>();

                    if (exDocument != null)
                    {
                        var replaceDocumentDlgResult = MessageBoxResult.Yes;
                        var notIncludeReports = exDocument.Reports.Where(r => rep.XmlReports.All(x => x.Report.Reportid != r.Reportid));

                        if (notIncludeReports.Any())
                        {
                            replaceDocumentDlgResult = MessageBox.Show($"Настаиваемый шаблон {d.Document.Name} используется в следующих документах, которые не импортируются: {string.Join(",", notIncludeReports.Select(x => x.reportname))}. Заменить?", "У настраиваемого шаблона есть связанные документы", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        }

                        if (replaceDocumentDlgResult == MessageBoxResult.Yes)
                            reportIds = exDocument.DeleteDocumentCascade(context);
                        else
                            return null;
                    }

                    d.Document.IsLocked = false;
                    d.Document.LockedBy = null;

                    context.Documents.Add(d.Document);

                    foreach (var reportId in reportIds)
                    {
                        d.Document.Reports.Add(context.Reports.FirstOrDefault(x => x.Reportid == reportId));
                    }

                    context.SaveChanges();

                    foreach (var content in d.DocumentContents)
                    {
                        var existingContent = context.DocumentContents.FirstOrDefault(c => c.Id == content.Id);

                        if (existingContent != null)
                        {
                            context.DocumentContents.Remove(existingContent);
                            context.SaveChanges();
                        }

                        context.DocumentContents.Add(content);
                        context.SaveChanges();
                    }

                    foreach (var paragraph in d.Paragraphs)
                    {
                        var existingParagraph = context.Paragraphs.FirstOrDefault(c => c.Id == paragraph.Id);

                        if (existingParagraph != null)
                        {
                            context.Paragraphs.Remove(existingParagraph);
                            context.SaveChanges();
                        }

                        context.Paragraphs.Add(paragraph);
                        context.SaveChanges();
                    }

                    foreach (var paragraphContent in d.ParagraphContents)
                    {
                        var existingParagraphContent = context.ParagraphContents.FirstOrDefault(c => c.Id == paragraphContent.Id);

                        if (existingParagraphContent != null)
                        {
                            context.ParagraphContents.Remove(existingParagraphContent);
                            context.SaveChanges();
                        }

                        context.ParagraphContents.Add(paragraphContent);
                        context.SaveChanges();
                    }
                }

                foreach (var r in rep.XmlReports)
                {
                    var exReport = context.Reports.FirstOrDefault(c => c.reportid == r.Report.Reportid);
                    var resDlgReplace = MessageBoxResult.None;
                    var changedGlobalSourceIds = new Dictionary<Guid, Guid>();

                    if (exReport != null)
                    {
                        if (reportPkgEntries.Any(ee => ee.Equals(exReport.reportid)))
                        {
                            MessageBox.Show($"Документ слияния {exReport.reportname} нельзя загрузить, так как он используется в КД", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }

                        resDlgReplace = MessageBox.Show($"Отчет \"{r.Report.Reportname}\" уже существует! Заменить?", "Отчет с таким идентификатором уже существует! Заменить?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (resDlgReplace == MessageBoxResult.Yes)
                            exReport.DeleteReportCascade(context);
                        else
                            continue;
                    }

                    r.Report.LockedBy = null;
                    r.Report.IsLocked = false;
                    r.Report.Servername = setting.ServerName;
                    r.Report.Defaultdatabase = setting.DbName;

                    foreach (var dataSource in r.DataSources.ToList())
                    {
                        var existingDataSource = context.DataSources.FirstOrDefault(c => c.Datasourceid == dataSource.Datasourceid);
                        var parentId = dataSource.ParentDataSource?.Datasourceid;
                        var checkRelatedDS = parentId != null && changedGlobalSourceIds.ContainsKey(parentId.Value);

                        if (existingDataSource != null && existingDataSource.Isglobal != true)
                        {
                            if (checkRelatedDS)
                            {
                                dataSource.Datasourceid = Guid.NewGuid();
                            }
                            else
                            {
                                context.DataSources.Remove(existingDataSource);
                                context.SaveChanges();
                            }
                        }

                        if (existingDataSource != null && existingDataSource.Isglobal == true)
                        {
                            var msgReplace = MessageBox.Show($"Для отчета \"{r.Report.Reportname}\" глобальный источник \"{existingDataSource.datasourcename}\" уже существует. Заменить?", "Заменить глобальный источник?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (msgReplace == MessageBoxResult.Yes)
                            {
                                ExportSource(new List<DataSource> { existingDataSource }, $@"{setting.GlobalDataSourcePath}\{Helper.ReplacingIllegalCharacters($"{existingDataSource.datasourcename}_{DateTime.Now.ToShortDateString()}.xml")}");

                                context.DataSources.Remove(existingDataSource);
                                context.DataSources.Add(dataSource);
                                context.SaveChanges();
                                continue;
                            }

                            var newId = Guid.NewGuid();
                            changedGlobalSourceIds.Add(dataSource.Datasourceid, newId);
                            dataSource.Datasourceid = newId;
                        }

                        context.DataSources.Add(dataSource);

                        if (checkRelatedDS)
                        {
                            var changedId = changedGlobalSourceIds[parentId.Value];
                            var ds = context.DataSources.FirstOrDefault(x => x.Datasourceid == changedId);
                            dataSource.ParentDataSource = ds;
                        }

                        context.SaveChanges();
                    }

                    context.Reports.Add(r.Report);
                    context.SaveChanges();

                    importReportId = r.Report.Reportid;

                    foreach (var condition in r.Conditions.ToList())
                    {
                        var existingCondition = context.Conditions.FirstOrDefault(c => c.Conditionid == condition.Conditionid);

                        var isNew = true;

                        if (existingCondition != null && existingCondition.Isglobal != true)
                        {
                            context.Conditions.Remove(existingCondition);
                            context.SaveChanges();
                        }

                        if (existingCondition != null && existingCondition.Isglobal == true)
                        {
                            var msgReplace = MessageBox.Show($"Для отчета \"{r.Report.Reportname}\" глобальная проверка \"{existingCondition.conditionname}\" уже существует. Заменить?", "Заменить глобальную проверку?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (msgReplace == MessageBoxResult.Yes)
                            {
                                ExportCondition(new List<WordMergeEngine.Models.Condition> { existingCondition }, $@"{setting.GlobalConditionPath}\{Helper.ReplacingIllegalCharacters($"{existingCondition.conditionname}_{DateTime.Now.ToShortDateString()}.xml")}");

                                context.Conditions.Remove(existingCondition);
                                context.Conditions.Add(condition);
                                context.SaveChanges();
                                isNew = false;
                            }
                            else
                            {
                                condition.Conditionid = Guid.NewGuid();
                            }
                        }

                        if (isNew)
                        {
                            context.Conditions.Add(condition);
                        }

                        var reportCondition = new ReportCondition();
                        reportCondition.Id = Guid.NewGuid();
                        reportCondition.Condition = condition;
                        reportCondition.Report = r.Report;

                        r.Report.ReportConditions.Add(reportCondition);
                        context.ReportConditions.Add(reportCondition);

                        context.SaveChanges();
                    }

                    foreach (var parameter in r.Parameters)
                    {
                        var existedParameter = context.Parameters.FirstOrDefault(c => c.Id == parameter.Id);

                        if (existedParameter != null && existedParameter.Isglobal != true)
                        {
                            context.Parameters.Remove(existedParameter);
                            context.SaveChanges();
                        }

                        if (existedParameter != null && existedParameter.Isglobal == true)
                        {
                            parameter.Id = Guid.NewGuid();
                        }

                        context.Parameters.Add(parameter);

                        var reportParameter = new ReportParameter();
                        reportParameter.Id = Guid.NewGuid();
                        reportParameter.Parameter = parameter;
                        reportParameter.Report = r.Report;

                        r.Report.ReportParameters.Add(reportParameter);
                        context.ReportParameters.Add(reportParameter);

                        context.SaveChanges();
                    }
                }
                MessageBox.Show("Импорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return importReportId;
        }

        public static Guid? ImportDataSource()
        {
            Guid? resultId = null;

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var result = dialog.ShowDialog();

            if (result != true)
                return resultId;

            var context = ((App)Application.Current).GetDBContext;

            var reader = new XmlSerializer(typeof(DataSourceXmlWrapper));

            var file = new StreamReader(dialog.FileName);

            DataSourceXmlWrapper datasourcesXml;

            try
            {
                datasourcesXml = (DataSourceXmlWrapper)reader.Deserialize(file);
            }
            catch (Exception)
            {
                MessageBox.Show("Внимание!", "Данный файл не является набором источников данных", MessageBoxButton.OK, MessageBoxImage.Information);
                return resultId;
            }

            foreach (var item in datasourcesXml.XmlDataSources)
            {
                foreach (var ds in item.DataSources)
                {
                    var existingDataSource = context.DataSources.FirstOrDefault(c => c.Datasourceid == ds.Datasourceid);

                    if (existingDataSource != null)
                    {
                        context.DataSources.Remove(existingDataSource);
                        context.DataSources.Add(ds);
                    }
                    else
                    {
                        context.DataSources.Add(ds);
                    }

                    context.SaveChanges();

                    resultId = ds.Datasourceid;
                }
            }

            MessageBox.Show("Импорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);

            return resultId;
        }

        public static Guid? ImportConditions()
        {
            Guid? resultId = null;

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var result = dialog.ShowDialog();

            if (result != true)
                return resultId;

            var context = ((App)Application.Current).GetDBContext;
            var reader = new XmlSerializer(typeof(ConditionXmlWrapper));
            var file = new StreamReader(dialog.FileName);

            ConditionXmlWrapper conditionsXml;

            try
            {
                conditionsXml = (ConditionXmlWrapper)reader.Deserialize(file);
            }
            catch (Exception)
            {
                MessageBox.Show("Внимание!", "Данный файл не является набором условий", MessageBoxButton.OK, MessageBoxImage.Information);
                return resultId;
            }

            foreach (var item in conditionsXml.XmlConditions)
            {
                var existingCondition = context.Conditions.FirstOrDefault(c => c.Conditionid == item.Condition.Conditionid);

                if (existingCondition != null)
                {
                    context.Conditions.Remove(existingCondition);
                    context.SaveChanges();
                }

                resultId = item.Condition.Conditionid;

                context.Conditions.Add(item.Condition);
                context.SaveChanges();
            }

            MessageBox.Show("Импорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);

            return resultId;
        }

        public static Guid? ImportParameters()
        {
            Guid? resultId = null;

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var result = dialog.ShowDialog();

            if (result != true)
                return resultId;

            var context = ((App)Application.Current).GetDBContext;
            var reader = new XmlSerializer(typeof(ParameterXmlWrapper));
            var file = new StreamReader(dialog.FileName);

            ParameterXmlWrapper parametersXml;

            try
            {
                parametersXml = (ParameterXmlWrapper)reader.Deserialize(file);
            }
            catch (Exception)
            {
                MessageBox.Show("Внимание!", "Данный файл не является набором параметров", MessageBoxButton.OK, MessageBoxImage.Information);
                return resultId;
            }

            foreach (var item in parametersXml.XmlParameters)
            {
                var existingParameter = context.Parameters.FirstOrDefault(c => c.Id == item.Parameter.Id);

                if (existingParameter != null)
                {
                    context.Parameters.Remove(existingParameter);
                    context.SaveChanges();
                }

                resultId = item.Parameter.Id;

                context.Parameters.Add(item.Parameter);
                context.SaveChanges();
            }

            MessageBox.Show("Импорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            return resultId;
        }

        public static List<Guid> ImportDocuments()
        {
            var resultIds = new List<Guid>();

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var result = dialog.ShowDialog();

            if (result != true)
                return resultIds;

            var context = ((App)Application.Current).GetDBContext;
            var reader = new XmlSerializer(typeof(DocumentXmlWrapper));
            var file = new StreamReader(dialog.FileName);

            DocumentXmlWrapper documentsXml;

            try
            {
                documentsXml = (DocumentXmlWrapper)reader.Deserialize(file);
            }
            catch (Exception)
            {
                MessageBox.Show("Внимание!", "Данный файл не является набором шаблонов согласования", MessageBoxButton.OK, MessageBoxImage.Information);
                return resultIds;
            }

            foreach (var item in documentsXml.XmlDocuments)
            {
                var resDlgReplace = MessageBoxResult.None;
                var existingDocument = context.Documents.Include("Report").FirstOrDefault(c => c.Id == item.Document.Id);
                var reportIds = new List<Guid>();

                if (existingDocument != null)
                {
                    var reportNames = string.Join(", ", existingDocument.Reports.Select(x => x.Reportname));
                    resDlgReplace = MessageBox.Show($"Шаблон \"{item.Document.Name}\" уже существует{(!string.IsNullOrEmpty(reportNames) ? $" и используется в документах {reportNames}" : "")}! Заменить?", "Шаблон с таким идентификатором уже существует! Заменить?", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (resDlgReplace == MessageBoxResult.Yes)
                        reportIds = existingDocument.DeleteDocumentCascade(context);
                    else
                        return null;
                }

                resultIds.Add(item.Document.Id);

                item.Document.IsLocked = false;
                item.Document.LockedBy = null;

                context.Documents.Add(item.Document);

                foreach (var reportId in reportIds)
                {
                    item.Document.Reports.Add(context.Reports.FirstOrDefault(x => x.Reportid == reportId));
                }

                context.SaveChanges();

                foreach (var content in item.DocumentContents)
                {
                    var existingContent = context.DocumentContents.FirstOrDefault(c => c.Id == content.Id);

                    if (existingContent != null)
                    {
                        context.DocumentContents.Remove(existingContent);
                        context.SaveChanges();
                    }

                    context.DocumentContents.Add(content);
                    context.SaveChanges();
                }

                foreach (var paragraph in item.Paragraphs)
                {
                    var existingParagraph = context.Paragraphs.FirstOrDefault(c => c.Id == paragraph.Id);

                    if (existingParagraph != null)
                    {
                        context.Paragraphs.Remove(existingParagraph);
                        context.SaveChanges();
                    }

                    context.Paragraphs.Add(paragraph);
                    context.SaveChanges();
                }

                foreach (var paragraphContent in item.ParagraphContents)
                {
                    var existingParagraphContent = context.ParagraphContents.FirstOrDefault(c => c.Id == paragraphContent.Id);

                    if (existingParagraphContent != null)
                    {
                        context.ParagraphContents.Remove(existingParagraphContent);
                        context.SaveChanges();
                    }

                    context.ParagraphContents.Add(paragraphContent);
                    context.SaveChanges();
                }
            }

            MessageBox.Show("Импорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            return resultIds;
        }

        public static Guid? ImportFilters()
        {
            Guid? resultId = null;

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var result = dialog.ShowDialog();

            if (result != true)
                return resultId;

            var context = ((App)Application.Current).GetDBContext;
            var reader = new XmlSerializer(typeof(FilterXmlWrapper));
            var file = new StreamReader(dialog.FileName);

            FilterXmlWrapper filtersXml;

            try
            {
                filtersXml = (FilterXmlWrapper)reader.Deserialize(file);
            }
            catch (Exception)
            {
                MessageBox.Show("Внимание!", "Данный файл не является набором параметров", MessageBoxButton.OK, MessageBoxImage.Information);
                return resultId;
            }

            foreach (var item in filtersXml.XmlFilters)
            {
                var existingFilter = context.Filters.FirstOrDefault(c => c.Id == item.Filter.Id);

                if (existingFilter != null)
                {
                    context.Filters.Remove(existingFilter);
                    context.SaveChanges();
                }

                resultId = item.Filter.Id;

                context.Filters.Add(item.Filter);
                context.SaveChanges();
            }

            MessageBox.Show("Импорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            return resultId;
        }

        private static void GetDataSources(DataModel context, List<DataSource> sources, ICollection<DataSource> dataSources)
        {
            foreach (var dataSource in dataSources.ToList())
            {
                sources.Add(dataSource);

                var dsWithNested = context.DataSources.AsNoTracking().Include("NestedDataSources").FirstOrDefault(ds => ds.Datasourceid == dataSource.Datasourceid);

                if (dsWithNested.NestedDataSources.Any())
                    GetDataSources(context, sources, dsWithNested.NestedDataSources);
            }
        }

        public static void ExportPackages(List<ReportPackage> packages)
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var filename = string.Empty;

            if (packages.Count == 1)
                filename = $"{packages.First().Name}_{DateTime.Now.ToShortDateString()}";
            else
                filename = $"ALL_PACKAGES_{DateTime.Now.ToShortDateString()}";

            dialog.FileName = filename;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var context = ((App)Application.Current).GetDBContext;
                context.Configuration.ProxyCreationEnabled = false;

                var exportPkg = new List<XmlPackage>();

                foreach (var proxyPackage in packages)
                {
                    var package = context.ReportPackages.AsNoTracking().Include("ReportPackageEntry").FirstOrDefault(rp => rp.ReportPackageId == proxyPackage.ReportPackageId);
                    var pkg = new XmlPackage { Package = package };

                    foreach (var proxyPe in package.ReportPackageEntries)
                    {
                        var pe = context.ReportPackageEntries.AsNoTracking().Include("Condition").FirstOrDefault(rpe => rpe.ReportPackageEntryId == proxyPe.ReportPackageEntryId);
                        var xmlentries = new XmlPackageEntry { Entry = pe };

                        foreach (var proxyCondition in pe.Conditions)
                        {
                            var condition = context.Conditions.AsNoTracking().FirstOrDefault(c => c.Conditionid == proxyCondition.Conditionid);
                            xmlentries.Conditions.Add(condition);
                        }

                        pkg.Entries.Add(xmlentries);
                    }

                    exportPkg.Add(pkg);
                }

                var xmlPackage = new ReportPackagesXml
                {
                    Packages = exportPkg,
                    ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                };

                var writer = new XmlSerializer(typeof(ReportPackagesXml));
                var file = new StreamWriter(dialog.FileName);

                writer.Serialize(file, xmlPackage);

                file.Close();

                context.Configuration.ProxyCreationEnabled = true;

                MessageBox.Show("Экспорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public static void ImportPackages()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "XML files (.xml)|*.xml"
            };

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var context = ((App)Application.Current).GetDBContext;
                var reader = new XmlSerializer(typeof(ReportPackagesXml));
                var file = new StreamReader(dialog.FileName);
                var reportPackagesXml = (ReportPackagesXml)reader.Deserialize(file);

                foreach (var package in reportPackagesXml.Packages)
                {
                    var existingPack = context.ReportPackages.FirstOrDefault(c => c.ReportPackageId == package.Package.ReportPackageId);

                    if (existingPack != null)
                    {
                        if (MessageBox.Show($"Пакет документов \"{package.Package.DisplayName}\" уже существует! Заменить?", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                            return;
                    }

                    foreach (var entry in package.Entries)
                    {
                        package.Package.ReportPackageEntries.Add(entry.Entry);
                    }

                    if (existingPack != null)
                    {
                        context.ReportPackages.Remove(existingPack);
                        context.ReportPackages.Add(package.Package);
                    }
                    else
                    {
                        context.ReportPackages.Add(package.Package);
                    }

                    context.SaveChanges();
                }

                MessageBox.Show("Импорт завершен", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

}
