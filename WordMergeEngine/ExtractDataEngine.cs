using System.Data.Common;
using System.Data;
using System.Reflection;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine.Models.Helpers;

namespace WordMergeEngine
{
    /// <summary>
    /// Класс подтягивания данных из БД 
    /// </summary>
    public partial class ExtractDataEngine
    {
        private static string _mcdsoftFlagField = "mcdsoft__flag_reserved";

        /// <summary>
        /// Подгрузка данных по отчету
        /// </summary>
        /// <param name="report"></param>
        /// <param name="isLoadDocument"></param>
        public static void Loader(Report report, bool isLoadDocument = true)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(isLoadDocument)} = {isLoadDocument})");

            LoadNestedDataSources(report.Datasource);

            if (report.Document == null)
                return;

            if (isLoadDocument)
                Loader(report.Document);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(isLoadDocument)} = {isLoadDocument}) отработал.");
        }

        /// <summary>
        /// Загрузка связанных источников данных
        /// </summary>
        /// <param name="dataSource"></param>
        public static void LoadNestedDataSources(DataSource dataSource)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(dataSource.Datasourcename)} = {dataSource?.Datasourcename})");

            if (dataSource != null)
            {
                foreach (var ds in dataSource.InverseParentDataSource)
                    LoadNestedDataSources(ds);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(dataSource.Datasourcename)} = {dataSource?.Datasourcename}) отработал.");
        }

        /// <summary>
        /// Подгрузка данных по шаблону
        /// </summary>
        /// <param name="document"></param>
        public static void Loader(Document document)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name})");

            foreach (var docContent in document.DocumentContents)
            {
                if (docContent.Paragraphs == null)
                    return;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}) отработал.");
        }

        /// <summary>
        /// Получения набора таблиц на основе источников данных и ID
        /// </summary>
        /// <param name="report"></param>
        /// <param name="RowID"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(ConnectionData connectionData, Report report, object RowID)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(RowID)} = {RowID})");

            CheckFields(report);

            var dataSet = new DataSet();
            var connection = GetConnection(connectionData.CreateConnection(report.Servername?.Trim(), report.Defaultdatabase));

            connection.Open();

            using (connection)
                LoadNestedDataTable(connection, report.Datasource, dataSet, LoadRootDataTable(connection, report.Datasource, dataSet, RowID));

            dataSet.ChangeQuotes();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(RowID)} = {RowID}) отработал. Получено {dataSet?.Tables?.Count} таблиц");

            return dataSet;
        }

        /// <summary>
        /// Проверка полей отчета
        /// </summary>
        /// <param name="report"></param>
        private static void CheckFields(Report report)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname})");

            if (string.IsNullOrEmpty(report.Servername))
                throw new ApplicationException($"В отчете \"{report.Reportname}\" не указано имя сервера базы данных");

            if (string.IsNullOrEmpty(report.Defaultdatabase))
                throw new ApplicationException($"В отчете \"{report.Reportname}\" не указано имя базы данных");

            if (!report.IsCustomTemplate && string.IsNullOrEmpty(report.Reportpath))
                throw new ApplicationException($"В отчете \"{report.Reportname}\" не указан путь к файлу с отчетом");

            if (report.IsCustomTemplate && report.Document == null)
                throw new ApplicationException($"В отчете \"{report.Reportname}\" не указан шаблон");

            if (string.IsNullOrEmpty(report.Reportcode))
                throw new ApplicationException($"В отчете \"{report.Reportname}\" не указан код интеграции");

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}) отработал");
        }

        /// <summary>
        /// Проверка условий отчета
        /// </summary>
        /// <param name="report"></param>
        /// <param name="conditions"></param>
        /// <param name="recordId"></param>
        /// <param name="userId"></param>
        /// <param name="checkFilled"></param>
        /// <returns></returns>
        public static List<string> CheckReportConditions(ConnectionData connectionData, Report report, IEnumerable<Condition> conditions, object recordId, Guid userId, bool checkFilled = true)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(conditions)} = {conditions?.Count()}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}, {nameof(checkFilled)} = {checkFilled})");

            if (report == null)
                throw new ArgumentNullException("report");

            if (conditions == null)
                throw new ArgumentNullException("conditions");

            var connection = GetConnection(connectionData.CreateConnection(report.Servername?.Trim(), report.Defaultdatabase));
            connection.Open();

            if (CheckUseCondition(connection, Helper.SubstituteParams(report.SqlQueryUseCondition, report, new Dictionary<string, string> { ["userid"] = userId.ToString() })))
                return new List<string>();

            var errors = new List<string>();

            CheckFields(report);

            if (!conditions.Any())
                return errors;

            using (connection)
            {
                foreach (var condition in conditions)
                {
                    if (condition.DataQuery == null)
                        throw new ApplicationException($"{condition.Conditionname}: Не указан запрос для выполнения проверки");

                    if (condition.Conditionoperator == null)
                        throw new ApplicationException($"{condition.Conditionname}: Не указан оператор сравнения для выполнения проверки");

                    if (condition.Recordcount == null)
                        throw new ApplicationException($"{condition.Conditionname}: Не указано число записей для выполнения проверки");

                    if (condition.Errormessage == null)
                        throw new ApplicationException($"{condition.Conditionname}: Не указан текст ошибки для проверки");

                    var dataQuery = condition.DataQuery.Replace("@reportcode", $"'{report.Reportcode}'");

                    if (userId != Guid.Empty)
                        dataQuery = dataQuery.Replace("@userid", $"'{userId}'");

                    dataQuery = Helper.InsertParametrValueInQuery(dataQuery, recordId.ToString());

                    var command = GetCommand(connection, dataQuery);
                    command.CommandTimeout = 300;

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var dt = new DataTable();
                            dt.Load(reader);

                            switch (condition.Conditionoperator)
                            {
                                case "=":
                                    if (!(dt.Rows.Count == condition.Recordcount))
                                        errors.Add(condition.Errormessage);
                                    break;
                                case "<>":
                                    if (!(dt.Rows.Count != condition.Recordcount))
                                        errors.Add(condition.Errormessage);
                                    break;
                                case "<":
                                    if (!(dt.Rows.Count < condition.Recordcount))
                                        errors.Add(condition.Errormessage);
                                    break;
                                case ">":
                                    if (!(dt.Rows.Count > condition.Recordcount))
                                        errors.Add(condition.Errormessage);
                                    break;
                                case ">=":
                                    if (!(dt.Rows.Count >= condition.Recordcount))
                                        errors.Add(condition.Errormessage);
                                    break;
                                case "<=":
                                    if (!(dt.Rows.Count <= condition.Recordcount))
                                        errors.Add(condition.Errormessage);
                                    break;
                                default:
                                    throw new ApplicationException($"{condition.Conditionname}: Задано некорректное условие сравнения.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Произошла ошибка при обработке корневого запроса. {ex.Message}");
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(conditions)} = {conditions?.Count()}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}, {nameof(checkFilled)} = {checkFilled}) отработал");

            return errors;
        }

        public static bool CheckReportCondition(ConnectionData connectionData, Report report, Condition condition, object recordId, Guid userId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(condition)} = {condition?.Conditionid}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId})");

            var connection = GetConnection(connectionData.CreateConnection(report.Servername?.Trim(), report.Defaultdatabase));

            connection.Open();

            var result = false;

            using (connection)
            {
                result = CheckCondition(connection, $"{condition.Conditionname} ({report.Reportname})", condition.DataQuery, condition.Conditionoperator, condition.Recordcount, recordId, userId);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(condition)} = {condition?.Conditionid}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}) отработал");

            return result;
        }

        /// <summary>
        /// Получить ошибки
        /// </summary>
        /// <param name="report"></param>
        /// <param name="recordId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<string> GetErrors(ConnectionData connectionData, Report report, object recordId, Guid userId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId})");

            var result = CheckReportConditions(connectionData, report, report.ReportConditions.Where(x => x.Condition.Conditiontype == null && x.Condition.ReportPackageEntry == null).Select(x => x.Condition), recordId, userId);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}) отработал");

            return result;
        }

        /// <summary>
        /// Получить ошибки при проверке предусловий
        /// </summary>
        /// <param name="report"></param>
        /// <param name="recordId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<string> GetErrorsPrecondition(ConnectionData connectionData, Report report, object recordId, Guid userId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId})");

            var result = CheckReportConditions(connectionData, report, report.ReportConditions.Where(x => x.Condition.Precondition != null && x.Condition.Precondition.Value == true).Select(x => x.Condition), recordId, userId);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}) отработал");

            return result;
        }

        private static void LoadNestedDataTable(DbConnection myDbConnection, DataSource parentDataSource, DataSet dataSet, DataRow parentRow)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(myDbConnection)} = {myDbConnection?.ConnectionString}, {nameof(parentDataSource)} = {parentDataSource}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц, {nameof(parentRow)} = {parentRow})");

            if (parentRow.Table.Columns.Contains(_mcdsoftFlagField))
            {
                if ((string)parentRow[_mcdsoftFlagField] == "1")
                    return;
                else
                    parentRow[_mcdsoftFlagField] = "1";
            }

            foreach (var ds in parentDataSource.InverseParentDataSource)
            {
                if (string.IsNullOrEmpty(ds.Datasourcename))
                    throw new ApplicationException("Не указано имя корневого источника данных");

                if (string.IsNullOrEmpty(ds.Keyfieldname))
                    throw new ApplicationException($"{ds.Datasourcename} - Не указано имя ключевого столбца в источнике данных");

                if (string.IsNullOrEmpty(ds.Dataquery))
                    throw new ApplicationException($"{ds.Datasourcename} - Не найден запрос в источнике данных");

                var query = ds.Dataquery;

                if (!string.IsNullOrEmpty(ds.Foreignkeyfieldname))
                    query = query.Replace("?", $"'{parentRow[ds.ParentDataSource.Keyfieldname]}'");

                var command = GetCommand(myDbConnection, query);
                command.CommandTimeout = 600;

                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!(ds.Assingleline ?? false))
                            LoadDataTable(myDbConnection, dataSet, ds, reader);
                        else
                        {
                            if (ds.Assingleline ?? false)
                            {
                                if (ds.InverseParentDataSource.Any())
                                    throw new ApplicationException($"{ds.Datasourcename} - У источника данных с выбранной опцией \"Одной строкой\" не может быть дочерних сущностей");
                            }

                            JoinDataTableAsSingleLine(parentRow.Table, parentRow, reader);
                        }
                    }
                }
                catch (ApplicationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"{ds.Datasourcename} - Произошла ошибка при обработке запроса. {ex.Message}");
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(myDbConnection)} = {myDbConnection?.ConnectionString}, {nameof(parentDataSource)} = {parentDataSource}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц, {nameof(parentRow)} = {parentRow}) отработал.");
        }

        private static void LoadDataTable(DbConnection connection, DataSet dataSet, DataSource ds, DbDataReader reader)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(ds)} = {ds?.Datasourcename}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц, {nameof(reader)} = {reader})");

            DataTable nestedTable;

            if (dataSet.Tables.Contains(ds.Datasourcename))
                nestedTable = dataSet.Tables[ds.Datasourcename];
            else
            {
                nestedTable = new DataTable(ds.Datasourcename);
                nestedTable.Columns.Add(new DataColumn { ColumnName = _mcdsoftFlagField, DefaultValue = 0 });
                dataSet.Tables.Add(nestedTable);
            }

            nestedTable.Load(reader);

            if (!string.IsNullOrEmpty(ds.Foreignkeyfieldname))
            {
                var relName = $"{ds.ParentDataSource.Datasourcename}_{ds.Datasourcename}_Relation";

                if (!dataSet.Relations.Contains(relName))
                {
                    var parentTable = dataSet.Tables[ds.ParentDataSource.Datasourcename];

                    var col1 = parentTable.Columns[ds.ParentDataSource.Keyfieldname];
                    var col2 = nestedTable.Columns[ds.Foreignkeyfieldname];

                    if (col2 == null)
                        throw new ApplicationException($"Произошла ошибка при обработке запроса. Внешний ключ ({ds.Foreignkeyfieldname}) в источнике данных \"{nestedTable.TableName}\" не найден.");

                    dataSet.Relations.Add(new DataRelation(relName, col1, col2, false));
                }
            }

            foreach (DataRow row in nestedTable.Rows)
                LoadNestedDataTable(connection, ds, dataSet, row);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(ds)} = {ds?.Datasourcename}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц, {nameof(reader)} = {reader}) отработал.");
        }

        private static void JoinDataTableAsSingleLine(DataTable joinTable, DataRow joinRow, DbDataReader reader)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(joinTable)} = {joinTable?.Rows?.Count} строк, {nameof(joinRow)} = {joinRow}, {nameof(reader)} = {reader})");

            var recCount = 0;

            if (reader.HasRows)
            {
                var currentRecord = joinRow;

                while (reader.Read())
                {
                    recCount++;

                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        DataColumn dc;

                        var columnName = $"{reader.GetName(i)}_{recCount}";

                        if (!joinTable.Columns.Contains(columnName))
                            dc = joinTable.Columns.Add(columnName, reader.GetFieldType(i));
                        else
                            dc = joinTable.Columns[columnName];

                        currentRecord[dc] = reader.GetValue(i);
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(joinTable)} = {joinTable?.Rows?.Count} строк, {nameof(joinRow)} = {joinRow}, {nameof(reader)} = {reader}) отработал.");
        }

        /// <summary>
        /// Загрузка основного источника
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="rootSource"></param>
        /// <param name="ds"></param>
        /// <param name="testid"></param>
        /// <returns></returns>
        public static DataRow LoadRootDataTable(DbConnection connection, DataSource rootSource, DataSet ds, object testid)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(rootSource)} = {rootSource?.Datasourcename}, {nameof(ds)} = {ds?.Tables?.Count} таблиц, {nameof(testid)} = {testid})");

            if (string.IsNullOrEmpty(rootSource.Datasourcename))
                throw new ApplicationException("Не указано имя корневого источника данных");

            if (testid == null)
                throw new ApplicationException($"{rootSource.Datasourcename} - Не указан Тестовый идентификатор");

            if (string.IsNullOrEmpty(rootSource.Keyfieldname))
                throw new ApplicationException($"{rootSource.Datasourcename} - Не указано имя ключевого столбца в источнике данных");

            if (string.IsNullOrEmpty(rootSource.Dataquery))
                throw new ApplicationException($"{rootSource.Datasourcename} - Не найден запрос в источнике данных");

            var root = new DataTable(rootSource.Datasourcename);

            var queryWithParamsValues = Helper.InsertParametrValueInQuery(rootSource.Dataquery, testid.ToString());

            var command = GetCommand(connection, queryWithParamsValues);
            command.CommandTimeout = 300;

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    root.Load(reader);

                    if (root.Rows.Count == 0)
                        throw new InvalidOperationException("Для заданных параметров невозможно сформировать отчет");
                    else
                    {
                        if (root.Rows.Count != 1)
                            throw new ApplicationException($"Корневой запрос должен возвращать только одну запись. Возвращено записей: {root.Rows.Count.ToString()}");
                    }

                    ds.Tables.Add(root);
                }
            }
            catch (InvalidOperationException ex)
            {
                throw new ApplicationException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Произошла ошибка при обработке корневого запроса. {ex.Message}");
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(rootSource)} = {rootSource?.Datasourcename}, {nameof(ds)} = {ds?.Tables?.Count} таблиц, {nameof(testid)} = {testid}) отработал.");

            return root.Rows[0];
        }

        /// <summary>
        /// Проверка условий параметра
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="condition"></param>
        /// <param name="recordId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CheckParameterCondition(DbConnection connection, ParameterCondition condition, object recordId, Guid userId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(condition)} = {condition?.Name}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId})");

            if (condition == null)
                throw new ArgumentNullException("condition");

            var result = CheckCondition(connection, condition.Name, condition.Query, condition.ConditionOperator, condition.RecordCount, recordId, userId);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(condition)} = {condition?.Name}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}) отработал.");

            return result;
        }

        /// <summary>
        /// Проверка условий фильтра
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="filter"></param>
        /// <param name="recordId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CheckFilterCondition(DbConnection connection, Filter filter, object recordId, Guid userId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(filter)} = {filter?.Name}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId})");

            if (filter == null)
                throw new ArgumentNullException("condition");

            var result = CheckCondition(connection, filter.Name, filter.VisibleQuery, filter.VisibleConditionOperator, filter.VisibleRecordCount, recordId, userId);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(filter)} = {filter?.Name}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}) отработал.");

            return result;
        }

        /// <summary>
        /// Проверка условия
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="name"></param>
        /// <param name="query"></param>
        /// <param name="conditionOperator"></param>
        /// <param name="count"></param>
        /// <param name="recordId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool CheckCondition(DbConnection connection, string name, string query, string conditionOperator, decimal? count, object recordId, Guid userId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(name)} = {name}, {nameof(query)} = {query}, {nameof(conditionOperator)} = {conditionOperator}, {nameof(count)} = {count}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId})");

            if (query == null)
                throw new ApplicationException($"{name}: Не указан запрос для выполнения проверки.");

            if (conditionOperator == null)
                throw new ApplicationException($"{name}: Не указан оператор сравнения для выполнения проверки.");

            if (count == null)
                throw new ApplicationException($"{name}: Не указано число записей для выполнения проверки.");

            var dataQuery = query;

            if (userId != Guid.Empty)
                dataQuery = dataQuery.Replace("@userid", $"'{userId}'");

            dataQuery = Helper.InsertParametrValueInQuery(dataQuery, recordId.ToString());

            var command = GetCommand(connection, dataQuery);
            command.CommandTimeout = 300;

            var result = false;

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);

                    switch (conditionOperator)
                    {
                        case "=":
                            result = dt.Rows.Count == count;
                            break;

                        case "<>":
                            result = dt.Rows.Count != count;
                            break;

                        case "<":
                            result = dt.Rows.Count < count;
                            break;

                        case ">":
                            result = dt.Rows.Count > count;
                            break;

                        case ">=":
                            result = dt.Rows.Count >= count;
                            break;

                        case "<=":
                            result = dt.Rows.Count <= count;
                            break;

                        default:
                            throw new ApplicationException($"{name}: Задано некорректное условие сравнения.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Произошла ошибка при обработке корневого запроса. {ex.Message}");
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(name)} = {name}, {nameof(query)} = {query}, {nameof(conditionOperator)} = {conditionOperator}, {nameof(count)} = {count}, {nameof(recordId)} = {recordId}, {nameof(userId)} = {userId}) отработал.");

            return result;
        }

        private static bool CheckUseCondition(DbConnection connection, string query)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query})");

            if (connection == null || string.IsNullOrEmpty(connection.ConnectionString) || string.IsNullOrEmpty(query))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}) отработал. Получено: пусто");

                return false;
            }

            try
            {
                var table = new DataTable(connection.DataSource);
                var command = GetCommand(connection, query);
                command.CommandTimeout = 300;

                var reader = command.ExecuteReader();
                table.Load(reader);

                var result = table.Rows.Count > 0;

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}) отработал. Получено: {result}");

                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Не удалось выполнить запрос. ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}), exeption = {ex.ToString()}");
            }
        }
    }
}
