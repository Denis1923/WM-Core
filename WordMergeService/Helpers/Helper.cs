using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WordMergeService.Helpers
{

    public static class Helper
    {
        public static Dictionary<string, string> ExtensionParameters(DataModel dbContext, string reportCode, Guid? auditId = null)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(auditId)} = {auditId})");

            var result = new Dictionary<string, string>();

            var report = (from p in dbContext.Report where p.reportcode == reportCode orderby p.reportname select p).FirstOrDefault();

            if (report == null)
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(auditId)} = {auditId}) отработал успешно.");

                return result;
            }

            result.Add("subjectemail", report.subjectemail);

            var audit = (from a in dbContext.Audit where a.Id == auditId select a).FirstOrDefault();

            if (audit != null)
            {
                result.Add("printFormId", audit.IntegrationDocumentId);
                result.Add("extSystemId", audit.IntegrationExternalId?.ToString());
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(auditId)} = {auditId}) отработал успешно.");

            return result;
        }

        public static Action<Guid> GetBeforeIntegration(DataModel dbContext, ConnectionData connectionData, Report report, object rowId, Dictionary<string, string> inputParameters)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.reportname}, {nameof(rowId)} = {rowId}, {nameof(inputParameters)} = {inputParameters?.Count})");

            var auditActions = report.Integration.Where(x => x.BeforeAction).OrderBy(x => x.Order).Select(integration => GetIntegrationData(dbContext, connectionData, report, integration, rowId, inputParameters)).ToList();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.reportname}, {nameof(rowId)} = {rowId}, {nameof(inputParameters)} = {inputParameters?.Count}) отработал успешно. Получено действий аудита: {auditActions?.Count}");

            return (auditId) => auditActions?.ForEach(x => x?.Invoke(auditId));
        }

        public static Action<Guid> GetAfterIntegration(DataModel dbContext, ConnectionData connectionData, Report report, object rowId, Dictionary<string, string> inputParameters)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.reportname}, {nameof(rowId)} = {rowId}, {nameof(inputParameters)} = {inputParameters?.Count})");

            var auditActions = report.Integration.Where(x => !x.BeforeAction).OrderBy(x => x.Order).Select(integration => GetIntegrationData(dbContext, connectionData, report, integration, rowId, inputParameters)).ToList();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.reportname}, {nameof(rowId)} = {rowId}, {nameof(inputParameters)} = {inputParameters?.Count}) отработал успешно. Получено действий аудита: {auditActions?.Count}");

            return (auditId) => auditActions?.ForEach(x => x?.Invoke(auditId));
        }

        private static Action<Guid> GetIntegrationData(DataModel dbContext, ConnectionData connectionData, Report report, Integration integration, object rowId, Dictionary<string, string> inputParameters)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.reportname}, {nameof(integration)} = {integration?.Id}, {nameof(rowId)} = {rowId}, {nameof(inputParameters)} = {inputParameters?.Count})");

            var configuration = MapConfiguration.GetIntegration(integration.SystemCode);

            if (configuration == null)
                return default;

            var audit = AuditHelper.GetAuditByCodeAndRowId(dbContext, report.reportcode, Guid.Parse(rowId.ToString()));
            var parameters = DocBuilder.GetIntegrationDataSet(dbContext, connectionData, report, integration, rowId, inputParameters);
            var action = configuration.ActionName.IsFromDS ? parameters[configuration.ActionName.Name] : configuration.ActionName.Name;
            var externalId = default(Guid?);

            foreach (var parameter in configuration.Parameters)
            {
                if (parameter.IsRowId)
                    parameters.Add(parameter.Name, rowId.ToString());

                if (audit != null && !string.IsNullOrEmpty(parameter.FromAudit))
                    parameters.Add(parameter.Name, GetPropertyValue(audit, parameter.FromAudit));

                if (report != null && !string.IsNullOrEmpty(parameter.FromReport))
                    parameters.Add(parameter.Name, GetPropertyValue(report, parameter.FromReport));

                if (parameter.Name == "externalId")
                {
                    externalId = parameters.ContainsKey("GUID") && Guid.TryParse(parameters["GUID"], out Guid guid) ? guid : (audit?.IntegrationExternalId ?? Guid.NewGuid());
                    parameters.Add(parameter.Name, externalId.ToString());
                }
            }

            var result = IntegrationHelper.Integration(configuration.Url, action, parameters);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.reportname}, {nameof(integration)} = {integration?.Id}, {nameof(rowId)} = {rowId}, {nameof(inputParameters)} = {inputParameters?.Count}) отработал успешно. {nameof(externalId)} = {externalId}");

            if (externalId != null)
                return (Guid auditId) => AuditHelper.AddIntegrationResultInAudit(dbContext, auditId, result["barCode"].ToString(), result["documentId"].ToString(), result["documentUrl"].ToString(), externalId.Value);

            return default;
        }

        private static string GetPropertyValue<T>(T item, string fieldName) => typeof(T).GetProperty(fieldName)?.GetValue(item)?.ToString();

        public static DataModel GetContext()
        {
            ProtocolWriter.Log.Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var result = ExtractDataEngine.GetContext(GetConnection());

            ProtocolWriter.Log.Debug($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно ()");

            return result;
        }

        public static ConnectionData GetConnection()
        {
            ProtocolWriter.Log.Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var serverName = ConfigurationManager.AppSettings["ServerName"];
            var isPosgreSql = bool.Parse(ConfigurationManager.AppSettings["IsPosgreSql"]);
            var dbName = ConfigurationManager.AppSettings["DatabaseName"];
            var username = ConfigurationManager.AppSettings["UserName"];
            var password = ConfigurationManager.AppSettings["Password"];

            var result = new ConnectionData(serverName, isPosgreSql ? ConnectionType.PostgresDbConnection : ConnectionType.MsSqlConnection, dbName, username, password);

            ProtocolWriter.Log.Debug($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно ()");

            return result;
        }
    }

}