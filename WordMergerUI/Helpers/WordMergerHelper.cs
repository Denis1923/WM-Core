namespace WordMergerUI.Helpers
{
    public class WordMergerHelper
    {
        private readonly string _quiredtype;

        public WordMergerHelper(string quiredtype)
        {
            _quiredtype = quiredtype;
        }

        private WordMergeSrvClient GetClient()
        {
            return new WordMergeSrvClient();
        }

        public AvailableReport GetReports(string entityName, Guid[] ids, Guid userId)
        {
            try
            {
                using (var client = GetClient())
                {
                    var availableReport = client.GetAvailableReportWithCondWithUserId(entityName, ids, userId);

                    if (!string.IsNullOrEmpty(_quiredtype))
                    {
                        if (_quiredtype == "doc")
                            availableReport.reports = availableReport.reports.Where(x => string.IsNullOrEmpty(x.ReportType) || x.ReportType == "doc").ToArray();
                        else if (_quiredtype == "docsreps")
                            availableReport.reports = availableReport.reports.Where(x => string.IsNullOrEmpty(x.ReportType) || x.ReportType == "doc" || x.ReportType == "rep").ToArray();
                        else
                            availableReport.reports = availableReport.reports.Where(x => !string.IsNullOrEmpty(x.ReportType) && x.ReportType == _quiredtype).ToArray();
                    }

                    return availableReport;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении документов {ex.Message}");
            }
        }

        public List<ReportPackage> GetPackages(string entityName)
        {
            try
            {
                using (var client = GetClient())
                    return client.GetAvailableReportPackage(entityName).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении пакета документов {ex.Message}");
            }
        }

        public ErrorsResult GetSuiteErrors(bool isPackage, string name, Guid[] ids, Guid userId)
        {
            try
            {
                using (var client = GetClient())
                    return client.GetSuiteErrors(isPackage, name, ids, userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при проверке документов {ex.Message}");
            }
        }

        public ErrorsResult GetErrors(bool isPackage, string reportCode, Guid id, Guid userId)
        {
            try
            {
                using (var client = GetClient())
                    return isPackage ? client.GetPackageErrors(reportCode, id) : client.GetErrors(reportCode, id, userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при проверке документа {ex.Message}");
            }
        }

        public MergedDocument OpenDocumentPackage(string packagename, List<Guid> ids, List<LabelValueEntry> paramValues = null)
        {
            try
            {
                using (var client = GetClient())
                {
                    if (ids.Count > 1)
                        return client.MergeDocumentSuite(true, packagename, ids.ToArray());
                    else
                    {
                        if (paramValues == null)
                            return client.MergeDocumentPackage(packagename, ids.First());
                        else
                            return client.MergeParametrizedDocumentPackage(packagename, ids.First(), WordMergeEngine.PackagePrintFormat.zip, paramValues.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка формирования документа {ex.Message}");
            }
        }

        public ReportParameter[] GetReportParameters(string reportCode, Guid id, Guid userId)
        {
            try
            {
                using (var client = GetClient())
                    return client.GetReportParametersWithUserId(reportCode, id, userId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка получения параметризированного документа. {ex.Message}");
            }
        }

        public MergedDocument MergeParametrizedDocument(string reportCode, Guid ids, List<LabelValueEntry> paramValues, string chooseFormat, bool isIgnoreCondition)
        {
            try
            {
                var request = new MergingParametrizedDocumentRequest
                {
                    ReportCode = reportCode,
                    RowId = ids,
                    ParamValues = paramValues.ToArray(),
                    ChooseFormat = chooseFormat,
                    IsIgnoreCondition = isIgnoreCondition
                };

                using (var client = GetClient())
                    return client.MergingParametrizedDocument(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка формирования параметризированного документа {ex.Message}");
            }
        }

        public MergedDocument MergeParametrizedDocumentWithUpload(string reportCode, Guid ids, List<LabelValueEntry> paramValues, string userId, string chooseFormat, bool isIgnoreCondition)
        {
            try
            {
                var request = new MergingParametrizedDocumentWithUserRequest
                {
                    ReportCode = reportCode,
                    RowId = ids,
                    ParamValues = paramValues.ToArray(),
                    ChooseFormat = chooseFormat,
                    IsIgnoreCondition = isIgnoreCondition,
                    UserId = userId
                };

                using (var client = GetClient())
                    return client.MergingParametrizedDocumentWithUploadWithUserContext(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки параметризированного документа {ex.Message}");
            }
        }

        public PreviewDocument GetDocumentForPreview(string docname, List<Guid> ids, List<LabelValueEntry> parameters)
        {

            if (ids.Count > 1)
                throw new Exception("Для пакета документов предварительный просмотр недоступен");
            try
            {
                using (var client = GetClient())
                    return client.GetDocumentAsImages(docname, ids.First().ToString(), parameters.ToArray());
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка предварительного просмотра документа {ex.Message}");
            }
        }

        public UploadResult UploadDocument(string reportCode, byte[] fileContent, string fileName)
        {
            try
            {
                using (var client = GetClient())
                    return client.UploadDocument(reportCode, fileContent, fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления документа {ex.Message}");
            }
        }
    }
}
