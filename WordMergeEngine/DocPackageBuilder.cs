using Aspose.Words;
using DocumentsPackage;
using System.Data;
using System.Data.Common;
using System.Text;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine.Models.Helpers;

using AsposeDocument = Aspose.Words.Document;

namespace WordMergeEngine
{
    /// <summary>
    /// Перечисление форматов сохранения
    /// </summary>
    public enum PackagePrintFormat
    {
        /// <summary>
        /// Формат архива ZIP
        /// </summary>
        zip,
        /// <summary>
        /// Формат документа PDF
        /// </summary>
        pdf,
        /// <summary>
        /// Формат документа Microsoft Word
        /// </summary>
        doc,
        /// <summary>
        /// Формат документа Microsoft Word версии 2007 и выше 
        /// </summary>
        docx,
        /// <summary>
        /// Формат файла данных XPS
        /// </summary>
        xps
    };

    /// <summary>
    /// Перечисление форматов входного файла
    /// </summary>
    public enum FileFormat
    {
        /// <summary>
        /// Формат документа Microsoft Word
        /// </summary>
        doc
    };

    public class DocPackageBuilder : IDocPackageBuildStrategy, IDocPrintStrategy
    {
        protected DataModel dbContext { get; private set; }
        protected ConnectionData connection { get; private set; }

        protected PackagePrintFormat packagePrintFormat;

        public CallingContext Context { get; private set; }

        public List<string> Templates { get; private set; }

        public List<KeyValuePair<string, string>> TemplateWarnings { get; private set; }

        public List<KeyValuePair<string, string>> TemplateErrors { get; private set; }

        public List<KeyValuePair<string, string>> PrintErrors { get; private set; }

        private Dictionary<string, string> ParamValues { get; set; }

        public ReportPackage Package { get; set; }

        public DocPackageBuilder(DataModel dbContext, ConnectionData connection, CallingContext context, PackagePrintFormat printFormat = PackagePrintFormat.zip)
        {
            packagePrintFormat = printFormat;
            this.dbContext = dbContext;
            this.connection = connection;

            if (string.IsNullOrEmpty(context.PackageName))
                throw new ApplicationException("В контексте вызова формирования комплекта документов не задано уникальное имя комплекта");

            if (string.IsNullOrEmpty(context.RecordId.ToString()))
                throw new ApplicationException("В контексте вызова формирования комплекта документов не задан идентификатор конкретной записи");

            Package = (from p in this.dbContext.ReportPackages where p.Name == context.PackageName select p).FirstOrDefault();

            if (Package == null)
                throw new ApplicationException(string.Format("Не найдено описание комплекта документов \"{0}\"!", context.PackageName));

            Context = context;

            Templates = new List<string>();
            TemplateWarnings = new List<KeyValuePair<string, string>>();
            TemplateErrors = new List<KeyValuePair<string, string>>();
            PrintErrors = new List<KeyValuePair<string, string>>();
        }

        public DocPackageBuilder(DataModel dbContext, ConnectionData connection, CallingContext context, Dictionary<String, String> paramValues, PackagePrintFormat printFormat = PackagePrintFormat.zip)
        {
            ParamValues = paramValues;

            packagePrintFormat = printFormat;
            this.dbContext = dbContext;
            this.connection = connection;

            if (string.IsNullOrEmpty(context.PackageName))
                throw new ApplicationException("В контексте вызова формирования комплекта документов не задано уникальное имя комплекта");

            if (string.IsNullOrEmpty(context.RecordId.ToString()))
                throw new ApplicationException("В контексте вызова формирования комплекта документов не задан идентификатор конкретной записи");

            Package = (from p in this.dbContext.ReportPackages where p.Name == context.PackageName select p).FirstOrDefault();

            if (Package == null)
                throw new ApplicationException(string.Format("Не найдено описание комплекта документов \"{0}\"!", context.PackageName));

            Context = context;

            Templates = new List<string>();
            TemplateWarnings = new List<KeyValuePair<string, string>>();
            TemplateErrors = new List<KeyValuePair<string, string>>();
            PrintErrors = new List<KeyValuePair<string, string>>();
        }

        public List<string> GetTemplateList(CallingContext context)
        {
            if (context == null)
                context = Context;

            var result = new List<string>();

            var packageEntries = (from pe in dbContext.ReportPackageEntries where pe.ReportPackage.ReportPackageId == Package.ReportPackageId orderby pe.NumberPosition ascending select pe).ToList();

            foreach (var entry in packageEntries)
            {
                var messages = ExtractDataEngine.CheckReportConditions(connection, entry.ReportReport, entry.Conditions, context.RecordId, Guid.Empty);

                if (!messages.Any())
                    result.Add(entry.ReportReport.Reportcode);
                else
                {
                    if (entry.IsObligatory)
                    {
                        foreach (var message in messages)
                            TemplateErrors.Add(new KeyValuePair<string, string>(entry.ReportReport.Reportname, message));
                    }
                    else
                    {
                        foreach (var message in messages)
                            TemplateWarnings.Add(new KeyValuePair<string, string>(entry.ReportReport.Reportname, message));
                    }
                }
            }

            Templates.Clear();
            Templates.AddRange(result);

            return result;
        }

        public PrintableDocument PrintDocument(string templateId)
        {
            if (ParamValues == null)
                return DocBuilder.PrintDocument(dbContext, connection, templateId, Context.RecordId, packagePrintFormat != PackagePrintFormat.zip);
            else
            {
                var report = (from p in dbContext.Reports where p.Reportcode == templateId orderby p.Reportname select p).FirstOrDefault();

                if (report == null)
                    throw new Exception($"Не удалось найти в базе WordMerger документа с кодом \"{templateId}\".");

                report.Datasource = Helper.SubstituteParams(report.Datasource, report, ParamValues);

                return DocBuilder.PrintDocument(dbContext, connection, report, Context.RecordId, ParamValues).document;
            }
        }

        public List<string> CheckDocuments()
        {
            var result = new List<string>();

            foreach (var template in Templates)
            {
                var report = (from r in dbContext.Reports where r.Reportcode == template select r).FirstOrDefault();

                var messages = ExtractDataEngine.GetErrors(connection, report, Context.RecordId, Guid.Empty);

                foreach (var message in messages)
                    PrintErrors.Add(new KeyValuePair<string, string>(report.Reportname, message));

                if (messages.Any())
                    result.AddRange(messages);
            }

            return result;
        }

        public PrintableDocument PrintPackage()
        {
            var pkgContext = new DocPackageBuildContext(Context, this, this);
            var pkg = pkgContext.BuildDocPackage();

            var packageEntries = (from pe in dbContext.ReportPackageEntries where pe.ReportPackage.ReportPackageId == Package.ReportPackageId orderby pe.NumberPosition ascending select pe);
            var package = (from packages in dbContext.ReportPackages where packages.ReportPackageId == Package.ReportPackageId select packages).FirstOrDefault();
            var templateId = pkg.Documents.Select(x => x.TemplateId).FirstOrDefault();
            var report = (from r in dbContext.Reports where r.Reportcode == templateId orderby r.Reportname select r).FirstOrDefault();
            var connection = ExtractDataEngine.GetConnection(this.connection.CreateConnection(report.Servername?.Trim(), report.Defaultdatabase));
            var displayName = GetFileName(package, connection, $"'{Context.RecordId}'");

            foreach (var entry in packageEntries)
            {
                var numberOfCopies = GetNumberOfCopies(entry, connection, Context.RecordId.ToString());
                if (numberOfCopies > 1)
                {
                    var doc = pkg.Documents.FirstOrDefault(d => string.CompareOrdinal(d.TemplateId, entry.ReportReport.Reportcode) == 0);

                    if (doc != null)
                    {
                        var i = 2;

                        while (i <= numberOfCopies)
                        {
                            var docClone = new PrintableDocument
                            {
                                FileName = $"{Path.GetFileNameWithoutExtension(doc.FileName)}_{i}{Path.GetExtension(doc.FileName)}",
                                Data = doc.Data,
                                TemplateId = doc.TemplateId
                            };

                            pkg.Documents.Add(docClone);

                            ++i;
                        }
                    }
                }
            }

            if (pkg.Documents.Any(i => i.PrintErrors.Any()))
            {
                var errorMessage = "Некоторые документы не удалось сформировать:";
                var documentsWithError = pkg.Documents.Where(i => i.PrintErrors.Any()).ToList();

                documentsWithError.ForEach(i => i.PrintErrors.ForEach(y => errorMessage += "\n" + y));

                throw new ApplicationException(errorMessage);
            }

            var result = new PrintableDocument();
            result.TemplateId = Context.PackageName;
            result.FileName = $"{displayName}.zip";

            if (packagePrintFormat == PackagePrintFormat.zip)
                result.Data = pkg.GetCompressedDocuments();
            else
            {
                var docList = pkg.Documents;
                var mergedDoc = new AsposeDocument();

                if (docList.Any())
                {
                    mergedDoc = new AsposeDocument(new MemoryStream(docList.First().Data));
                    docList.RemoveAt(0);

                    foreach (var d in docList)
                        mergedDoc.AppendDocument(new AsposeDocument(new MemoryStream(d.Data)), ImportFormatMode.KeepSourceFormatting);
                }

                using (var ms = new MemoryStream())
                {
                    if (packagePrintFormat == PackagePrintFormat.pdf)
                    {
                        mergedDoc.Save(ms, SaveFormat.Pdf);
                        result = new PrintableDocument { FileName = $"{displayName}.pdf", Data = ms.ToArray() };
                    }
                    else if (packagePrintFormat == PackagePrintFormat.doc)
                    {
                        mergedDoc.Save(ms, SaveFormat.Doc);
                        result = new PrintableDocument { FileName = $"{displayName}.doc", Data = ms.ToArray() };
                    }
                    else if (packagePrintFormat == PackagePrintFormat.docx)
                    {
                        mergedDoc.Save(ms, SaveFormat.Docx);
                        result = new PrintableDocument { FileName = $"{displayName}.docx", Data = ms.ToArray() };
                    }
                    else
                    {
                        mergedDoc.Save(ms, SaveFormat.Xps);
                        result = new PrintableDocument { FileName = $"{displayName}.xps", Data = ms.ToArray() };
                    }
                }
            }

            result.PrintErrors.Clear();

            foreach (var doc in pkg.Documents)
            {
                if (doc.PrintErrors.Any())
                    result.PrintErrors.AddRange(doc.PrintErrors);
            }

            return result;
        }

        public string MessagesToString(bool criticalErrorsOnly)
        {
            var errors = new StringBuilder();

            if (TemplateErrors.Any())
            {
                errors.AppendLine("Ошибки формирования комплекта: ");

                foreach (var err in TemplateErrors)
                    errors.AppendFormat($"- документ \"{err.Key}\": {err.Value}\n");

                errors.AppendLine(string.Empty);
            }

            if (!criticalErrorsOnly && TemplateWarnings.Any())
            {
                errors.AppendLine("Предупреждения о непопадании документов в комплект: ");

                foreach (var err in TemplateWarnings)
                    errors.AppendFormat($"- документ \"{err.Key}\": {err.Value}\n");

                errors.AppendLine(string.Empty);
            }

            if (!criticalErrorsOnly && PrintErrors.Any())
            {
                errors.AppendLine("Ошибки печати документов: ");

                foreach (var err in PrintErrors)
                    errors.AppendFormat($"- документ \"{err.Key}\": {err.Value}\n");

                errors.AppendLine(string.Empty);
            }

            return errors.ToString();
        }

        public static string GetFileName(ReportPackage reportPackage, DbConnection connection, string param)
        {
            var timeStamp = (bool)reportPackage.IsSetDate ? $"_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}" : string.Empty;
            var query = reportPackage.Sqlqueryfilename;
            if (!string.IsNullOrEmpty(query))
            {
                query = !string.IsNullOrEmpty(param) ? query.Replace("?", param) : query;

                var fileName = ExecSqlQuery(connection, query, "FileName");
                if (!string.IsNullOrEmpty(fileName))
                    return $"{Helper.ReplacingIllegalCharacters(fileName)}{timeStamp}";
            }
            return $"{reportPackage.DisplayName}{timeStamp}";
        }

        public static int? GetNumberOfCopies(ReportPackageEntry reportPackageEntry, DbConnection connection, string param)
        {
            var query = reportPackageEntry.SqlQueryCopyNumber;
            if (!string.IsNullOrEmpty(query))
            {
                query = !string.IsNullOrEmpty(param) ? query.Replace("?", param) : query;

                var numberOfCopiesString = ExecSqlQuery(connection, query, "NumberOfCopies");
                if (int.TryParse(numberOfCopiesString, out int numberOfCopies))
                    return numberOfCopies;
            }
            return reportPackageEntry.NumberOfCopies;
        }

        public static string ExecSqlQuery(DbConnection connection, string query, string fieldName)
        {
            if (connection == null || string.IsNullOrEmpty(connection.ConnectionString) || string.IsNullOrEmpty(query))
                return string.Empty;

            try
            {
                var table = new DataTable(connection.DataSource);

                var command = ExtractDataEngine.GetCommand(connection, query);
                command.CommandTimeout = 300;
                connection.Open();

                var reader = command.ExecuteReader();
                table.Load(reader);

                if (table.Rows.Count != 1)
                    throw new ApplicationException($"Запрос должен возвращать только одну запись. Возвращено записей: {table.Rows.Count.ToString()}");

                if (!table.Rows[0].Table.Columns.Contains(fieldName))
                    throw new ApplicationException($"Запись, которую вернул запрос ({query}), не содержит поле \"{fieldName}\"");

                connection.Close();

                return table.Rows[0][fieldName].ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Не удалось выполнить запрос. SqlConnection.ConnectionString = {connection.ConnectionString}, query = {query}, exeption = {ex.ToString()}");
            }
        }
    }
}
