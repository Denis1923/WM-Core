using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordMergeEngine.DirectImport
{
    public class SqlImportExport
    {
        private readonly string _connectionString;
        public SqlImportExport(string serverName, string dbName)
        {
            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = dbName,
                IntegratedSecurity = true,
                MultipleActiveResultSets = true
            };

            _connectionString = sqlBuilder.ToString();
        }

        private DataTable RunCommand(string entityName, string commandText)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(commandText, connection))
                {
                    connection.Open();

                    var sda = new SqlDataAdapter(command);
                    var dt = new DataTable(entityName);
                    sda.Fill(dt);

                    return dt;
                }
            }
        }

        private string GetTableString(DataTable dt)
        {
            using (StringWriter sw = new StringWriter())
            {
                dt.WriteXml(sw);
                return sw.ToString();
            }
        }

        private string GetRowString(string entityName, string commandText) => GetTableString(RunCommand(entityName, commandText));

        public string ReadReport(string id) => GetRowString("Report", $"select top 1 * from Report where reportid = '{id}'");

        public string ReadDataSources(string id)
        {
            var result = new List<DataTable>();

            result.Add(RunCommand("DataSource", $"select ds.* from DataSource ds where ds.datasourceid = '{id}'"));

            ReadNestedDataSource(result, id);

            var resultTable = new DataTable("DataSource");

            result.ForEach(x => resultTable.Merge(x));

            return GetTableString(resultTable);
        }

        private void ReadNestedDataSource(List<DataTable> result, string id)
        {
            var table = RunCommand("DataSource", $"select * from DataSource where parentdatasourceid = '{id}'");

            result.Add(table);

            foreach (DataRow row in table.Rows)
            {
                var childId = row["datasourceid"].ToString();

                ReadNestedDataSource(result, childId);
            }
        }

        public string ReadConditions(string reportId) => GetRowString("Condition", $"select c.* from Condition c join ReportCondition rc on c.conditionid = rc.conditionid where rc.reportid = '{reportId}' and c.ReportPackageEntryId is null");
        public string ReadParameters(string reportId) => GetRowString("Parameter", $"select p.* from Parameter p join ReportParameter rp on p.id = rp.parameterid where rp.reportid = '{reportId}'");
        public string ReadDocument(string documentId) => GetRowString("Document", $"select top 1 d.* from Document d where d.Id = '{documentId}'");
        public string ReadDocumentContent(string documentId) => GetRowString("DocumentContent", $"select dc.* from DocumentContent dc where dc.documentid = '{documentId}'");
        public string ReadParagraphs(string documentId) => GetRowString("Paragraph", $"select p.* from Paragraph p join DocumentContent dc on p.DocumentContentId = dc.Id where dc.documentid = '{documentId}'");
        public string ReadParagraphContent(string documentId) => GetRowString("ParagraphContent", $"select pc.* from ParagraphContent pc join Paragraph p on pc.ParagraphId = p.Id join DocumentContent dc on p.DocumentContentId = dc.Id where dc.documentid = '{documentId}'");

    }
}
