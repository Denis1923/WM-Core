using Aspose.Cells.Charts;
using Aspose.Cells;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WordMergeEngine.Models;

namespace WordMergeEngine.Helpers
{
    /// <summary>
    /// Общий Хелпер
    /// </summary>
    public class Helper
    {
        /// <summary>
        /// Замена запрещенных символов
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplacingIllegalCharacters(string str)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(str)} = {str})");

            var sb = new StringBuilder(str);

            sb.Replace('~', '_');
            sb.Replace('&', '_');
            sb.Replace('\'', '_');
            sb.Replace(';', '_');
            sb.Replace('#', '_');
            sb.Replace('%', '_');
            sb.Replace('*', '_');
            sb.Replace(':', '_');
            sb.Replace('<', '_');
            sb.Replace('>', '_');
            sb.Replace('|', '_');
            sb.Replace('?', '_');
            sb.Replace('/', '_');
            sb.Replace('\\', '_');
            sb.Replace('{', '_');
            sb.Replace(' ', '_');
            sb.Replace('}', '_');

            var result = sb.ToString();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(str)} = {str}) отработал. Получено {result}");

            return result;
        }

        /// <summary>
        /// Добавление значений параметров в запрос
        /// </summary>
        /// <param name="query"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public static string InsertParametrValueInQuery(string query, string parameterValue)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(query)} = {query}, {nameof(parameterValue)} = {parameterValue})");

            if (string.IsNullOrEmpty(query))
                return query;

            var result = query.Replace("?", $"'{parameterValue}'");

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(query)} = {query}, {nameof(parameterValue)} = {parameterValue}) отработал. Получено {result}");

            return result;
        }

        public static string SubstituteParams(string query, Report report, Dictionary<string, string> paramValues)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(query)} = {query}, {nameof(report)} = {report?.Reportname}, {(paramValues != null && paramValues.Any() ? string.Join(";", paramValues.Select(x => $"{x.Key} : {x.Value}")) : string.Empty)})");

            if (string.IsNullOrEmpty(query))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(query)} = {query}, {nameof(report)} = {report?.Reportname}, {(paramValues != null && paramValues.Any() ? string.Join(";", paramValues.Select(x => $"{x.Key} : {x.Value}")) : string.Empty)}) отработал. Запрос пустой");
                return query;
            }

            query = Regex.Replace(query, $@"@reportcode", $"'{report.Reportcode}'");

            foreach (var key in paramValues.Keys)
            {
                try
                {
                    if (key == "userid")
                    {
                        query = query.Replace($"@{key}", $"'{paramValues[key]}'");
                        continue;
                    }

                    if (report.ReportParameters == null || report.ReportParameters.Where(x => x.Parameter.Name == key) == null)
                        continue;

                    switch (report.ReportParameters.Where(x => x.Parameter.Name == key).Select(x => x.Parameter).ToArray()[0].Datatype)
                    {
                        case "String":
                            {
                                query = query.Replace($"@{key}", $"N'{paramValues[key]}'");
                                break;
                            }
                        case "DateTime":
                            {
                                DateTime temp;

                                if (DateTime.TryParse(paramValues[key], out temp))
                                    query = query.Replace($"@{key}", $"'{temp.ToString("s")}'");
                                else
                                    query = query.Replace($"@{key}", "null");
                                break;
                            }
                        case "Bool":
                            {
                                bool temp;
                                var value = paramValues[key] == "true,false" ? "true" : paramValues[key];
                                if (bool.TryParse(value, out temp))
                                {
                                    if (temp)
                                        query = query.Replace($"@{key}", "1");
                                    else
                                        query = query.Replace($"@{key}", "0");
                                }
                                else
                                    query = query.Replace($"@{key}", "null");
                                break;
                            }
                        case "OptionSet":
                            {
                                query = query.Replace($"@{key}", paramValues[key]);
                                break;
                            }
                        case "LookUp":
                            {
                                Guid temp;
                                if (Guid.TryParse(paramValues[key], out temp))
                                    query = query.Replace($"@{key}", "'" + temp.ToString() + "'");
                                else
                                    query = query.Replace($"@{key}", "null");
                                break;
                            }
                    }
                }
                catch
                {
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(query)} = {query}, {nameof(report)} = {report?.Reportname}, {(paramValues != null && paramValues.Any() ? string.Join(";", paramValues.Select(x => $"{x.Key} : {x.Value}")) : string.Empty)}) отработал. Получено {query}");

            return query;
        }

        /// <summary>
        /// Подстановка параметров в запросы
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="report"></param>
        /// <param name="paramValues"></param>
        /// <returns></returns>
        public static DataSource SubstituteParams(DataSource dataSource, Report report, Dictionary<string, string> paramValues)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(dataSource)} = {dataSource?.Datasourcename}, {nameof(report)} = {report?.Reportname}, {nameof(paramValues)} = {(paramValues != null && paramValues.Any() ? string.Join(";", paramValues.Select(x => $"{x.Key} : {x.Value}")) : string.Empty)})");

            dataSource.Dataquery = SubstituteParams(dataSource.Dataquery, report, paramValues);

            foreach (var reportCondition in report.ReportConditions)
                reportCondition.Condition.ChangedDataQuery = SubstituteParams(reportCondition.Condition.Dataquery, report, paramValues);

            if (dataSource.InverseParentDataSource != null && dataSource.InverseParentDataSource.Any())
                RelacestituteParams(dataSource, report, paramValues);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(dataSource)} = {dataSource?.Datasourcename}, {nameof(report)} = {report?.Reportname}, {(paramValues != null && paramValues.Any() ? string.Join(";", paramValues.Select(x => $"{x.Key} : {x.Value}")) : string.Empty)}) отработал. Получено {dataSource?.Datasourcename}");

            return dataSource;
        }

        /// <summary>
        /// Подстановка параметров в дочерние запросы
        /// </summary>
        /// <param name="datasource"></param>
        /// <param name="report"></param>
        /// <param name="paramValues"></param>
        public static void RelacestituteParams(DataSource datasource, Report report, Dictionary<string, string> paramValues)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(datasource)} = {datasource?.Datasourcename}, {nameof(report)} = {report?.Reportname}, {nameof(paramValues)} = {paramValues?.Count} параметров)");

            foreach (var item in datasource.InverseParentDataSource)
            {
                item.Dataquery = SubstituteParams(item.Dataquery, report, paramValues);

                if (item.InverseParentDataSource.Count != 0)
                    RelacestituteParams(item, report, paramValues);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(datasource)} = {datasource?.Datasourcename}, {nameof(report)} = {report?.Reportname}, {nameof(paramValues)} = {paramValues?.Count} параметров) отработал");
        }

        public static CellArea ParseCellArea(string stringValue, string tableName, int startRow, int maxRow, int maxColumn)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(stringValue)} = {stringValue}, {nameof(tableName)} = {tableName}, {nameof(startRow)} = {startRow}, {nameof(maxRow)} = {maxRow}, {nameof(maxColumn)} = {maxColumn})");

            var result = new CellArea();

            result.StartRow = startRow + 1;
            result.EndRow = maxRow;

            if (startRow == maxRow)
                result.EndRow = startRow + 1;

            stringValue = stringValue.Replace("{" + tableName + "}", "").Replace(" ", "").Replace("{", "").Replace("}", "");

            if (stringValue.StartsWith("-"))
            {
                if (stringValue.Length < 2)
                    throw new ApplicationException($"В разметке документа допущена ошибка:\nТег сдвига диапазона для источника \"{tableName}\" задан неверно!\nДоступные варианты {{*-*}}, {{*-}}, {{-*}} или не указывать.");

                result.StartColumn = 0;
                result.EndColumn = CellsHelper.ColumnNameToIndex(stringValue[1].ToString());

                if (result.EndColumn == -1)
                    throw new ApplicationException("В разметке документа допущена ошибка:\nНеверно указано значение столбца в тэге сдвига.");
            }
            else
            {
                if (stringValue.Length < 2)
                    throw new ApplicationException($"В разметке документа допущена ошибка:\nТег сдвига диапазона для источника \"{tableName}\" задан неверно!\nДоступные варианты {{*-*}}, {{*-}}, {{-*}} или не указывать.");

                result.StartColumn = CellsHelper.ColumnNameToIndex(stringValue[0].ToString());

                if (stringValue.Length == 2)
                    result.EndColumn = maxColumn;
                else
                    result.EndColumn = CellsHelper.ColumnNameToIndex(stringValue[2].ToString());

                if (result.StartColumn == -1 || result.EndColumn == -1 || result.StartColumn > result.EndColumn)
                    throw new ApplicationException("В разметке документа допущена ошибка:\nНеверно указано значение столбца в тэге сдвига.");
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(stringValue)} = {stringValue}, {nameof(tableName)} = {tableName}, {nameof(startRow)} = {startRow}, {nameof(maxRow)} = {maxRow}, {nameof(maxColumn)} = {maxColumn}) отработал");

            return result;
        }

        /// <summary>
        /// Сохранение стиля после переноса
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="area"></param>
        /// <param name="destRow"></param>
        /// <param name="destCol"></param>
        public static void SaveFormatAfterMove(ref Worksheet sheet, CellArea area, int destRow, int destCol)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(sheet)} = {sheet}, {nameof(area)} = {area}, {nameof(destRow)} = {destRow}, {nameof(destCol)} = {destCol})");

            var ii = 0;
            var jj = 0;

            for (int i = area.StartColumn; i <= area.EndColumn; i++)
            {
                for (int j = area.StartRow; j <= area.EndRow; j++)
                {
                    sheet.Cells[j, i].SetStyle(sheet.Cells[destRow + jj, destCol + ii].GetStyle());
                    jj++;
                }

                ii++;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(sheet)} = {sheet}, {nameof(area)} = {area}, {nameof(destRow)} = {destRow}, {nameof(destCol)} = {destCol}) отработал");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetBook"></param>
        /// <param name="Tables"></param>
        /// <returns></returns>
        public static Workbook ShiftAreas(Workbook targetBook, DataTableCollection Tables)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(targetBook)} = {targetBook}, {nameof(Tables)} = {Tables?.Count} таблиц)");

            foreach (DataTable t in Tables)
            {
                foreach (Worksheet sheet in targetBook.Worksheets)
                {
                    var cell = sheet.Cells.Find("{" + t.TableName + "}", null, new FindOptions() { LookInType = LookInType.Values, LookAtType = LookAtType.StartWith });

                    if (cell != null)
                    {
                        if (!cell.StringValue.Contains("movetable"))
                            continue;

                        var startCol = cell.Column;
                        var startRow = cell.Row;

                        if (string.Equals(cell.StringValue, "{" + t.TableName + "}"))
                        {
                            var columnCount = 0;

                            if (t.Rows.Count != 0)
                            {
                                for (var i = 0; i < t.Rows[0].ItemArray.Count(); i++)
                                {
                                    if (!t.Columns[i].ColumnName.StartsWith("ignore_", StringComparison.CurrentCultureIgnoreCase) && t.Columns[i].ColumnName != "mcdsoft__flag_reserved" && !t.Columns[i].ColumnName.StartsWith("skipcolumn_", StringComparison.CurrentCultureIgnoreCase))
                                        columnCount++;
                                }
                            }

                            var areaShift = new CellArea();

                            areaShift.StartColumn = startCol;
                            areaShift.StartRow = startRow + 1;
                            areaShift.EndColumn = startCol + columnCount - 1;
                            areaShift.EndRow = sheet.Cells.MaxRow;

                            if (startRow == sheet.Cells.MaxRow)
                                areaShift.EndRow = startRow + 1;

                            if (areaShift.StartColumn < areaShift.EndColumn && areaShift.StartRow <= areaShift.EndRow)
                            {
                                sheet.Cells.InsertRows(areaShift.StartRow, t.Rows.Count - 1);

                                for (var j = areaShift.StartRow; j < startRow + t.Rows.Count; j++)
                                {
                                    for (var i = areaShift.StartColumn; i <= areaShift.EndColumn; i++)
                                    {
                                        var column = areaShift.StartColumn == 0 ? startCol + i : startCol + i - 1;

                                        sheet.Cells[j, i].SetStyle(sheet.Cells[startRow, column].GetStyle());
                                    }
                                }

                                sheet.AutoFitRow(areaShift.StartRow, areaShift.StartRow + t.Rows.Count - 1, areaShift.StartColumn, areaShift.EndColumn);
                            }
                        }
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(targetBook)} = {targetBook}, {nameof(Tables)} = {Tables?.Count} таблиц) отработал");

            return targetBook;
        }

        /// <summary>
        /// Заполнение диаграмм
        /// </summary>
        /// <param name="targetBook"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static Workbook FillCharts(Workbook targetBook, List<SourceCell> cells)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(targetBook)} = {targetBook}, {nameof(cells)} = {cells?.Count} ячеек)");

            foreach (Worksheet sheet in targetBook.Worksheets)
            {
                foreach (Chart chart in sheet.Charts)
                {
                    foreach (Series series in chart.NSeries)
                    {
                        var clearCellName = series.Name.Replace("=", "").Replace("$", "").Replace("'", "").Replace("!", "");
                        var equal = cells.Where(x => series.Name.Contains(x.SheetName) && x.CellName == clearCellName.Replace(x.SheetName, "")).FirstOrDefault();

                        if (equal != null)
                        {
                            var str = series.Values.Replace("=", "").Replace(equal.SheetName, "").Replace("'", "").Replace("!", "").Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                            series.Values += ":" + str[0] + "$" + (Convert.ToInt32(str[1]) + equal.RowCount - 1).ToString();

                            str = series.XValues.Replace("=", "").Replace(equal.SheetName, "").Replace("'", "").Replace("!", "").Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                            series.XValues += ":$" + str[0] + "$" + (Convert.ToInt32(str[1]) + equal.RowCount - 1).ToString();
                        }
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(targetBook)} = {targetBook}, {nameof(cells)} = {cells?.Count} ячеек) отработал");

            return targetBook;
        }

        /// <summary>
        /// разбор гиперссылки
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <param name="url"></param>
        /// <param name="displayText"></param>
        public static void SplitHyperlinkValue(string fieldValue, out string url, out string displayText)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(fieldValue)} = {fieldValue})");

            var splits = fieldValue.Split(",".ToCharArray(), 2);

            url = splits[0].Trim();

            if (splits.Length > 1)
                displayText = splits[1];
            else
                displayText = url;

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(fieldValue)} = {fieldValue}, {nameof(url)} = {url}, {nameof(displayText)} = {displayText}) отработал");
        }
    }
}
