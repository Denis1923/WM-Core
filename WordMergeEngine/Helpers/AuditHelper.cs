using System.Data;
using System.Reflection;
using WordMergeEngine.Models;

namespace WordMergeEngine.Helpers
{
    /// <summary>
    /// Класс методов по аудиту
    /// </summary>
    public static class AuditHelper
    {
        /// <summary>
        /// Добавление записи аудита
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reportCode"></param>
        /// <param name="rowId"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public static Guid AddAuditItem(DataModel context, string reportCode, Guid rowId, Dictionary<string, string> paramValues)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(rowId)} = {rowId}, {nameof(paramValues)} = {paramValues?.Count} параметров)");

            var userId = paramValues?.FirstOrDefault(x => x.Key == "userid").Value;

            var audit = new Audit
            {
                Id = Guid.NewGuid(),
                CreatedOn = DateTime.Now,
                CreatedBy = !string.IsNullOrEmpty(userId) ? Guid.Parse(userId) : default(Guid?),
                ReportCode = reportCode,
                RowId = rowId,
                ParametersToDictionary = paramValues
            };

            context.Audits.Add(audit);
            context.SaveChanges();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(rowId)} = {rowId}, {nameof(paramValues)} = {paramValues?.Count} параметров) отработал");

            return audit.Id;
        }

        /// <summary>
        /// Добавление шаблона в запись аудита
        /// </summary>
        /// <param name="context"></param>
        /// <param name="auditId"></param>
        /// <param name="template"></param>
        public static void AddTemplateInAudit(DataModel context, Guid auditId, string template)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(template)} = {template})");

            var audit = context.Audits.FirstOrDefault(x => x.Id == auditId);

            if (audit == null)
                throw new ApplicationException($"Не найдена запись аудита с ID {auditId}");

            audit.Template = template;

            context.SaveChanges();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(template)} = {template}) отработал");
        }

        /// <summary>
        /// Добавление ссылки в запись аудита
        /// </summary>
        /// <param name="context"></param>
        /// <param name="auditId"></param>
        /// <param name="url"></param>
        public static void AddUrlInAudit(DataModel context, Guid auditId, string url)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(url)} = {url})");

            var audit = context.Audits.FirstOrDefault(x => x.Id == auditId);

            if (audit == null)
                throw new ApplicationException($"Не найдена запись аудита с ID {auditId}");

            audit.DocumentLink = url;

            context.SaveChanges();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(url)} = {url}) отработал");
        }

        /// <summary>
        /// Получение записи аудита по коду отчета и идентификатору
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public static Audit GetAuditByCodeAndRowId(DataModel context, string code, Guid rowId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(code)} = {code}, {nameof(rowId)} = {rowId})");

            var audit = context.Audits.Where(x => x.ReportCode == code && x.RowId == rowId).OrderByDescending(x => x.CreatedOn).FirstOrDefault();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(code)} = {code}, {nameof(rowId)} = {rowId}) отработал. Получена запись {audit?.Id}");

            return audit;
        }

        /// <summary>
        /// Добавление данных из интеграции в запись аудита
        /// </summary>
        /// <param name="context"></param>
        /// <param name="auditId"></param>
        /// <param name="barCode"></param>
        /// <param name="documentId"></param>
        /// <param name="documentUrl"></param>
        /// <param name="externalId"></param>
        public static void AddIntegrationResultInAudit(DataModel context, Guid auditId, string barCode, string documentId, string documentUrl, Guid externalId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(barCode)} = {barCode}, {nameof(documentId)} = {documentId}, {nameof(documentUrl)} = {documentUrl}, {nameof(externalId)} = {externalId})");
            
            var audit = context.Audits.FirstOrDefault(x => x.Id == auditId);

            if (audit == null)
                throw new ApplicationException($"Не найдена запись аудита с ID {auditId}");

            audit.BarCode = barCode;
            audit.IntegrationDocumentId = documentId;
            audit.IntegrationDocumentLocation = documentUrl;
            audit.IntegrationExternalId = externalId;

            context.SaveChanges();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(barCode)} = {barCode}, {nameof(documentId)} = {documentId}, {nameof(documentUrl)} = {documentUrl}, {nameof(externalId)} = {externalId}) отработал");
        }

        /// <summary>
        /// Добавление источника данных в запись аудита
        /// </summary>
        /// <param name="context"></param>
        /// <param name="auditId"></param>
        /// <param name="ds"></param>
        public static void AddDataSetInAudit(DataModel context, Guid auditId, DataSet ds)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(ds)} = {ds?.Tables?.Count} таблиц)");

            var audit = context.Audits.FirstOrDefault(x => x.Id == auditId);

            if (audit == null)
                throw new ApplicationException($"Не найдена запись аудита с ID {auditId}");

            audit.DataSetFromDB = ds;

            context.SaveChanges();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(auditId)} = {auditId}, {nameof(ds)} = {ds?.Tables?.Count} таблиц) отработал");
        }
    }
}
