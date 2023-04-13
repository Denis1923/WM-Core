using System.Reflection;
using WordMergeEngine.Helpers;
using WordMergeEngine.Assets;
using WordMergeEngine.Models.Helpers;
using Telerik.Windows.Documents;
using Telerik.Windows.Documents.FormatProviders;
using Telerik.Windows.Documents.FormatProviders.Html;
using Telerik.Windows.Documents.FormatProviders.OpenXml.Docx;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using Telerik.Windows.Documents.FormatProviders.Txt;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.Lists;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.Model.Revisions;
using Telerik.Windows.Documents.Model.Styles;
using TelerikParagraph = Telerik.Windows.Documents.Model.Paragraph;
using System.Data;
using WordMergeEngine.Assets.Enums;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace WordMergeEngine
{
    public static class TelerikBuilder
    {
        public static (StyleDefinition Style, string Name) GetDocumentStyle(RadDocument document)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document})");

            var style = default(StyleDefinition);
            var styleName = string.Empty;

            var elements = document.Sections.SelectMany(x => x.Children);

            if (elements.Select(x => x.StyleName).Distinct().Count() == 1)
            {
                style = elements.First().Style;
                styleName = elements.First().StyleName;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document}) отработал. Получено: {nameof(style)} = {style?.DisplayName}, {nameof(styleName)} = {styleName}");

            return (style, styleName);
        }

        public static (byte[] docBA, DataTable dt) PrintWordDocument(Models.DataModel dbContext, Models.Report report, object rowId, DataTable dataTable, Guid auditId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(dataTable)} = {dataTable?.Rows?.Count} строк, {nameof(auditId)} = {auditId})");

            var (doc, dt) = PrintDocument(report.Document, Transpose(dataTable));

            AuditHelper.AddTemplateInAudit(dbContext, auditId, ExportDocument(doc));

            var result = (docBA: new DocxFormatProvider().Export(doc), dt);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(dataTable)} = {dataTable?.Rows?.Count} строк, {nameof(auditId)} = {auditId}) отработал. Получено: {nameof(result.docBA)} = {result.docBA?.Length} байт, {nameof(result.dt)} = {result.dt?.Rows?.Count} строк");

            return result;
        }

        private static string ExportDocument(RadDocument document) => new XamlFormatProvider().Export(document);

        public static (RadDocument doc, DataTable dt) PrintDocument(Models.Document document, DataTable reportVariables)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(reportVariables)} = {reportVariables?.Rows?.Count} строк)");

            var targetDocument = new RadDocument();
            var mainStyle = default(StyleDefinition);
            var mainStyleName = string.Empty;

            var section = new Section();

            targetDocument.Sections.Add(section);

            var variables = CreateDocumentVariablesDataTable();

            if (!string.IsNullOrEmpty(document.Data))
            {
                var sr = new StringReader(document.Data);
                variables.ReadXml(sr);
            }

            if (reportVariables != null && reportVariables.Rows.Count > 0)
                foreach (DataRow row in reportVariables.Rows)
                    variables.Rows.Add(row.ItemArray);

            if (!string.IsNullOrEmpty(document.Template))
            {
                var provider = new XamlFormatProvider();
                targetDocument = provider.Import(document.Template);

                (mainStyle, mainStyleName) = GetDocumentStyle(targetDocument);

                section = targetDocument.Sections.First();
            }

            RecalculateParagraphsFilters(document, variables);
            var Paragraphs = document.Paragraph.Where(y => y.Deleted == false).OrderBy(x => x.OrderNo).ToList();

            foreach (var dbParagraph in Paragraphs.Where(p => p.PassConditions == true && p.Deleted == false).OrderBy(pp => pp.OrderNo))
            {

                var contents = dbParagraph.ParentParagraph != null
                    ? dbParagraph.ParentParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList()
                    : dbParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList();

                var paragraphContentDefault = default(Models.ParagraphContent);
                foreach (var pc in contents)
                {
                    if (pc.DefaultVersion == true)
                    {
                        paragraphContentDefault = pc;
                    }

                    if (string.IsNullOrEmpty(pc.Condition) || (pc.DefaultVersion == true))
                    {
                        pc.PassConditions = true;
                        continue;
                    }

                    if (pc.ActiveFrom.HasValue & pc.ActiveFrom > DateTime.Today)
                    {
                        pc.PassConditions = false;
                        continue;
                    }

                    if (pc.ActiveTill.HasValue & pc.ActiveTill < DateTime.Today)
                    {
                        pc.PassConditions = false;
                        continue;
                    }

                    pc.PassConditions = CheckConditionGroup(variables, pc.Condition);
                }

                var dbParagraphContent = paragraphContentDefault;

                var filteredContents = contents.Where(p => p.PassConditions == true && (p.DefaultVersion == false || p.DefaultVersion is null));

                if (filteredContents.Count() == 1)
                {
                    dbParagraphContent = filteredContents.First();
                }
                else if (filteredContents.Count() > 1)
                {
                    throw new ApplicationException($"{dbParagraph.RelativeNo}{dbParagraph.Name} - Found too many versions");
                }

                if (dbParagraphContent is null)
                {
                    if (contents.Count() == 1)
                    {
                        dbParagraphContent = contents.First();
                    }
                    else
                    {
                        throw new ApplicationException($"{dbParagraph.RelativeNo}{dbParagraph.Name} - Version not found");
                    }
                }

                var textImporter = new XamlFormatProvider();

                if (dbParagraphContent.Content is null) continue;

                var sourceDocument = textImporter.Import(dbParagraphContent.Content.ToString());

                foreach (var sourceListStyle in sourceDocument.ListManager.GetAllListStyles().ToArray())
                {

                    var targetListStyle = new ListStyle(sourceListStyle)
                    {
                        ID = dbParagraph.OrderNo.Value * 10000 + sourceListStyle.ID
                    };

                    if (!(targetListStyle.NumStyleLink is null))
                    {
                        var x = sourceDocument.ListManager.GetAllListStyles().ToArray().Where(ll => ll.StyleLink == targetListStyle.NumStyleLink).First();
                        targetListStyle.NumStyleLink = string.Empty;

                        for (int levelIndex = 0; levelIndex < 9; levelIndex++)
                        {
                            var listLevel = x.Levels[levelIndex];

                            targetListStyle.Levels.Add(listLevel);
                        }
                    }

                    targetDocument.ListManager.RegisterListStyleIfNecessary(targetListStyle);
                }

                var documentListDict = new Dictionary<string, DocumentList>();

                foreach (var localSection in sourceDocument.Sections)
                {
                    if (dbParagraph.Numerable == true)
                    {
                        var documentElement = localSection.Children.First();
                        if (documentElement is TelerikParagraph firstParagraph)
                        {
                            if (firstParagraph.Inlines.Count > 0)
                            {
                                var firstInline = firstParagraph.Inlines.First();
                                if (firstInline is Span localSpan)
                                {
                                    localSpan.Text = $"{dbParagraph.RelativeNo}.\u00A0{localSpan.Text}";
                                }
                            }
                        }
                    }

                    foreach (DocumentElement localParagraph in localSection.Blocks.ToArray())
                    {
                        var localParagraphCopy = CopyElement(localParagraph, targetDocument, mainStyle, mainStyleName);
                        section.Children.Add(localParagraphCopy);

                        if (localParagraph is TelerikParagraph paragraph1 && paragraph1.ListId != -1)
                        {
                            var docList = sourceDocument.ListManager.GetDocumentListById(paragraph1.ListId);
                            var listStyle = sourceDocument.ListManager.GetListStyleById(docList.StyleId);
                            if (!documentListDict.ContainsKey(docList.ID.ToString()))
                            {
                                var documentListStyle = targetDocument.ListManager.GetListStyleById(dbParagraph.OrderNo.Value * 10000 + listStyle.ID);
                                var documentList = new DocumentList(documentListStyle, targetDocument);
                                documentListDict.Add(docList.ID.ToString(), documentList);
                            }
                            documentListDict[docList.ID.ToString()].AddParagraph((TelerikParagraph)localParagraphCopy, paragraph1.ListLevel);
                        }
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(reportVariables)} = {reportVariables?.Rows?.Count} строк) отработал. Получено: {nameof(targetDocument)} = {targetDocument}");

            return (targetDocument, GetDataForDocument(Paragraphs, variables));
        }

        private static DocumentElement CopyElement(DocumentElement source, RadDocument targetDocument, StyleDefinition mainStyle, string mainStyleName)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(source)} = {source?.Tag}, {nameof(targetDocument)} = {targetDocument}, {nameof(mainStyle)} = {mainStyle}, {nameof(mainStyleName)} = {mainStyleName})");

            var copy = source.CreateDeepCopy();

            if (source is TelerikParagraph)
            {
                if (source.HasStyle && !source.Style.IsDefault)
                {
                    var newStyle = new StyleDefinition(source.Style);
                    newStyle.Name = GetNewStyleName(targetDocument.StyleRepository.Select(x => x.Name).ToList(), newStyle.Name);

                    copy.Style = newStyle;
                    targetDocument.StyleRepository.Add(newStyle);
                }
                else
                {
                    copy.Style = mainStyle;
                    copy.StyleName = mainStyleName;
                }
            }
            else if (copy is Table table)
            {
                SetInheritCellBorder(table);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(source)} = {source?.Tag}, {nameof(targetDocument)} = {targetDocument}, {nameof(mainStyle)} = {mainStyle}, {nameof(mainStyleName)} = {mainStyleName}) отработал. Получено: {nameof(copy)} = {copy?.Tag}");

            return copy;
        }

        private static void SetInheritCellBorder(Table table)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table.Rows?.Count} строк)");

            var rowCount = table.Rows.Count;
            var columnCount = table.Rows.FirstOrDefault()?.Cells.Count ?? 0;
            var mergedCells = new Dictionary<(int, int), (int, int)>();

            for (var i = 0; i < rowCount; i++)
            {
                var row = table.Rows.ElementAt(i);
                var skipcell = 0;
                for (var j = 0; j < columnCount; j++)
                {
                    if (mergedCells.ContainsKey((i, j)))
                    {
                        skipcell++;
                        continue;
                    }

                    var currentJ = j - skipcell;
                    var cell = row.Cells.ElementAt(currentJ);

                    mergedCells.Add((i, j), (i, currentJ));

                    if (cell.HasRowSpan)
                        for (var k = 1; k < cell.RowSpan; k++)
                            mergedCells.Add((i + k, j), (i, currentJ));

                    if (cell.HasColumnSpan)
                        for (var m = 1; m < cell.ColumnSpan; m++)
                            mergedCells.Add((i, j + m), (i, currentJ));

                    if (cell.HasColumnSpan && cell.HasRowSpan)
                        for (var k = 1; k < cell.RowSpan; k++)
                            for (var m = 1; m < cell.ColumnSpan; m++)
                                mergedCells.Add((i + k, j + m), (i, currentJ));

                }
            }

            LogHelper.GetLogger().Debug($"В методе {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table.Rows?.Count} строк) получено {Environment.NewLine}{string.Join(Environment.NewLine, mergedCells.Select(x => $"({x.Key.Item1}, {x.Key.Item2}) => ({x.Value.Item1}, {x.Value.Item2})"))}");

            for (var i = 0; i < rowCount; i++)
            {
                var row = table.Rows.ElementAt(i);
                for (var j = 0; j < columnCount; j++)
                {
                    var cell = GetCell(table.Rows, mergedCells, i, j);

                    if (cell.Borders.Top.Style == BorderStyle.Inherit && i > 0)
                    {
                        var topCell = GetCell(table.Rows, mergedCells, i - 1, j);

                        if (topCell.ColumnSpan >= cell.ColumnSpan)
                        {
                            cell.Borders = cell.Borders.SetTop(new Border(topCell.Borders.Bottom));

                            LogHelper.GetLogger().Debug($"В методе {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table.Rows?.Count} строк) обрабатывается ({i}, {j}) установили TOP из ({i - 1}, {j}) = {topCell.Borders.Bottom.Style}");
                        }
                    }

                    if (cell.Borders.Bottom.Style == BorderStyle.Inherit && i < rowCount - 1)
                    {
                        var bottomCell = GetCell(table.Rows, mergedCells, i + 1, j);

                        if (bottomCell.ColumnSpan >= cell.ColumnSpan)
                        {
                            cell.Borders = cell.Borders.SetBottom(new Border(bottomCell.Borders.Top));

                            LogHelper.GetLogger().Debug($"В методе {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table.Rows?.Count} строк) обрабатывается ({i}, {j}) установили BOTTOM из ({i + 1}, {j}) = {bottomCell.Borders.Top.Style}");
                        }
                    }

                    if (cell.Borders.Left.Style == BorderStyle.Inherit && j > 0)
                    {
                        var leftCell = GetCell(table.Rows, mergedCells, i, j - 1);

                        if (leftCell.RowSpan >= cell.RowSpan)
                        {
                            cell.Borders = cell.Borders.SetLeft(new Border(leftCell.Borders.Right));

                            LogHelper.GetLogger().Debug($"В методе {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table.Rows?.Count} строк) обрабатывается ({i}, {j}) установили LEFT из ({i}, {j - 1}) = {leftCell.Borders.Right.Style}");
                        }
                    }

                    if (cell.Borders.Right.Style == BorderStyle.Inherit && j < columnCount - 1)
                    {
                        var rightCell = GetCell(table.Rows, mergedCells, i, j + 1);

                        if (rightCell.RowSpan >= cell.RowSpan)
                        {
                            cell.Borders = cell.Borders.SetRight(new Border(rightCell.Borders.Left));

                            LogHelper.GetLogger().Debug($"В методе {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table.Rows?.Count} строк) обрабатывается ({i}, {j}) установили RIGHT из ({i}, {j + 1}) = {rightCell.Borders.Left.Style}");
                        }
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table.Rows?.Count} строк) отработал.");
        }

        private static TableCell GetCell(TableRowCollection rows, Dictionary<(int, int), (int, int)> mergedCells, int i, int j)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(rows)} = {rows?.Count} строк, {nameof(mergedCells)} = {mergedCells?.Count} сопоставлений ячеек, {nameof(i)} = {i}, {nameof(j)} = {j})");

            var (currentI, currentJ) = mergedCells.ContainsKey((i, j)) ? mergedCells[(i, j)] : (i, j);
            var cell = rows.ElementAt(currentI).Cells.ElementAt(currentJ);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(rows)} = {rows?.Count} строк, {nameof(mergedCells)} = {mergedCells?.Count} сопоставлений ячеек, {nameof(i)} = {i}, {nameof(j)} = {j}) отработал.");

            return cell;
        }

        private static string GetNewStyleName(List<string> nameList, string sourceName, int count = 0)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(nameList)} = {nameList?.Count} строк, {nameof(sourceName)} = {sourceName}, {nameof(count)} = {count})");

            var checkName = count == 0 ? sourceName : $"{sourceName}_{count}";
            var result = !nameList.Contains(checkName) ? checkName : GetNewStyleName(nameList, sourceName, ++count);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(nameList)} = {nameList?.Count} строк, {nameof(sourceName)} = {sourceName}, {nameof(count)} = {count}) отработал. Результат: {result}");

            return result;
        }

        public static DataTable GetConditionsTableDefinition()
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var dataTable = new DataTable("Conditions");
            dataTable.Columns.Add(new DataColumn("Variable1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Condition1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Value1", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Variable2", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Condition2", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Value2", typeof(string)));

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");

            return dataTable;
        }

        public static bool CheckCondition(DataTable variables, string variable, OperatorEnum condition, string varValue)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(variables)} = {variables?.Rows?.Count} строк, {nameof(condition)} = {condition}, {nameof(varValue)} = {varValue})");

            var pass = false;
            var dataVar = string.Empty;
            var dataValue = string.Empty;

            foreach (DataRow dataRow in variables.Rows)
            {
                if (variable == dataRow["Variable"].ToString())
                {
                    dataVar = dataRow["Variable"].ToString();
                    dataValue = dataRow["Value"].ToString();
                    break;
                }
            }

            if (string.IsNullOrEmpty(dataVar))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(variables)} = {variables?.Rows?.Count} строк, {nameof(condition)} = {condition}, {nameof(varValue)} = {varValue}) отработал. Результат: {false}");

                return false;
            }

            switch (condition)
            {
                case OperatorEnum.Equal:
                    pass = string.IsNullOrEmpty(varValue) && string.IsNullOrEmpty(dataValue) || !string.IsNullOrEmpty(varValue) && varValue.Equals(dataValue);
                    break;

                case OperatorEnum.NotEqual:
                    pass = string.IsNullOrEmpty(varValue) && !string.IsNullOrEmpty(dataValue) || !string.IsNullOrEmpty(varValue) && !varValue.Equals(dataValue);
                    break;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(variables)} = {variables?.Rows?.Count} строк, {nameof(condition)} = {condition}, {nameof(varValue)} = {varValue}) отработал. Результат: {pass}");

            return pass;

        }

        private static DataTable GetDataForDocument(List<Models.Paragraph> paragraphs, DataTable variables)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(paragraphs)} = {paragraphs?.Count} строк, {nameof(variables)} = {variables?.Rows?.Count} строк)");

            var dt = new DataTable("Data");
            dt.Rows.Add();
            foreach (DataRow r in variables.Rows)
            {
                dt.Columns.Add(new DataColumn(r["Variable"].ToString(), typeof(string)));
                dt.Rows[0][r["Variable"].ToString()] = r["Value"];
            }

            int col = 0;
            foreach (var p in paragraphs.Where(p => !string.IsNullOrEmpty(p.ReferenceName)))
            {
                var colName = "Ref_" + Regex.Replace(p.ReferenceName.ToString(), "\\W", "");
                if (dt.Columns.Contains(colName))
                {
                    col++;
                    colName += col.ToString();
                }
                dt.Columns.Add(new DataColumn(colName, typeof(string)));
                dt.Rows[0][colName] = p.RelativeNo;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(variables)} = {variables?.Rows?.Count} строк, {nameof(variables)} = {variables?.Rows?.Count} строк) отработал. Результат: {dt?.Rows?.Count} строк");

            return dt;
        }

        private static DataTable CreateDocumentVariablesDataTable()
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var dataTable = new DataTable
            {
                TableName = "DocVariables"
            };
            dataTable.Columns.Add(new DataColumn("Variable", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Value", typeof(string)));

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");

            return dataTable;
        }

        private static DataTable Transpose(DataTable table)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table?.Rows?.Count} строк)");

            var dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("Variable", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Value", typeof(string)));

            for (var i = 0; i < (table?.Columns.Count ?? 0); i++)
            {
                dataTable.Rows.Add(table.Columns[i].ColumnName, table.Rows[0][i].ToString());
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table?.Rows?.Count} строк) отработал. Получено {nameof(dataTable)} = {dataTable?.Rows?.Count} строк");

            return dataTable;
        }

        private static IDocumentFormatProvider GetSaveProvider(Models.Report report)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report?.Reportname})");

            if (string.IsNullOrEmpty(report.Reportformat))
                return new DocxFormatProvider();

            if (report.Reportformat.ToLower() == "pdf")
                return new PdfFormatProvider();

            if (report.Reportformat.ToLower() == "html")
                return new HtmlFormatProvider();

            throw new ApplicationException($"Для документа задан неподдерживаемый формат - {report.Reportformat}");
        }

        private static void RecalculateParagraphsFilters(Models.Document document, DataTable variables)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(variables)} = {variables?.Rows?.Count} строк)");

            foreach (var p in document.Paragraph.Where(y => y.Deleted == false).OrderBy(x => x.OrderNo).ToList())
            {
                if (string.IsNullOrEmpty(p.Condition))
                {
                    p.PassConditions = true;
                    continue;
                }

                p.PassConditions = CheckConditionGroup(variables, p.Condition);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(variables)} = {variables?.Rows?.Count} строк) отработал");
        }

        public static RadDocument GetApproveDocument(Models.Document document, DataTable variables, DateTime filterDate)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(variables)} = {variables?.Rows?.Count} строк, {nameof(filterDate)} = {filterDate})");

            var targetDocument = new RadDocument();
            var mainStyle = default(StyleDefinition);
            var mainStyleName = string.Empty;

            var section = new Section();

            targetDocument.Sections.Add(section);

            if (!string.IsNullOrEmpty(document.Template))
            {
                var provider = new XamlFormatProvider();
                targetDocument = provider.Import(document.Template);

                (mainStyle, mainStyleName) = GetDocumentStyle(targetDocument);

                section = targetDocument.Sections.First();
            }

            var Paragraphs = document.Paragraph.Where(y => y.Deleted == false).OrderBy(x => x.OrderNo).ToList();

            foreach (var dbParagraph in Paragraphs.Where(p => p.Deleted == false).OrderBy(pp => pp.OrderNo))
            {
                var contents = dbParagraph.ParentParagraph != null
                    ? dbParagraph.ParentParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList()
                    : dbParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList();

                if (contents.Count > 0)
                {
                    var paragraph = new TelerikParagraph();
                    var span = new Span($"\nУсловия для параграфа {dbParagraph.RelativeNo}. {dbParagraph.Name}")
                    {
                        ForeColor = Color.FromRgb(0, 0, 139)
                    };
                    paragraph.Inlines.Add(span);
                    section.Blocks.Add(paragraph);
                }

                foreach (var paragraphContentDefault in contents)
                {
                    var dbParagraphContent = paragraphContentDefault;

                    var textImporter = new XamlFormatProvider();

                    if (dbParagraphContent.Content is null) continue;

                    var sourceDocument = textImporter.Import(dbParagraphContent.Content.ToString());

                    foreach (var sourceListStyle in sourceDocument.ListManager.GetAllListStyles().ToArray())
                    {
                        var targetListStyle = new ListStyle(sourceListStyle)
                        {
                            ID = dbParagraph.OrderNo.Value * 10000 + sourceListStyle.ID
                        };

                        if (!(targetListStyle.NumStyleLink is null))
                        {
                            var x = sourceDocument.ListManager.GetAllListStyles().ToArray().Where(ll => ll.StyleLink == targetListStyle.NumStyleLink).First();
                            targetListStyle.NumStyleLink = string.Empty;

                            for (int levelIndex = 0; levelIndex < 9; levelIndex++)
                            {
                                var listLevel = x.Levels[levelIndex];

                                targetListStyle.Levels.Add(listLevel);
                            }
                        }

                        targetDocument.ListManager.RegisterListStyleIfNecessary(targetListStyle);
                    }

                    var documentListDict = new Dictionary<string, DocumentList>();

                    if ((!string.IsNullOrEmpty(dbParagraphContent.Tooltip) || dbParagraphContent.DefaultVersion == true))
                    {
                        var paragraph = new TelerikParagraph();

                        var span = new Span($"\n\t{(!string.IsNullOrEmpty(dbParagraphContent.Tooltip) ? dbParagraphContent.Tooltip : "Версия по умолчанию")}");
                        if (dbParagraphContent.Approved == true)
                        {
                            span.ForeColor = Color.FromRgb(0, 0, 139);
                        }
                        else
                        {
                            span.ForeColor = Color.FromRgb(255, 00, 0);
                        }
                        paragraph.Inlines.Add(span);

                        if (!string.IsNullOrEmpty(dbParagraphContent.Comment))
                        {
                            var commentSpan = new Span($" ({dbParagraphContent.Comment})")
                            {
                                ForeColor = Color.FromRgb(0, 140, 0)
                            };
                            paragraph.Inlines.Add(commentSpan);
                        }

                        section.Blocks.Add(paragraph);
                    }

                    foreach (var localSection in sourceDocument.Sections)
                    {
                        if (dbParagraph.Numerable == true)
                        {
                            var documentElement = localSection.Children.First();
                            if (documentElement is TelerikParagraph firstParagraph)
                            {
                                if (firstParagraph.Inlines.Count > 0)
                                {
                                    var firstInline = firstParagraph.Inlines.First();
                                    if (firstInline is Span localSpan)
                                    {
                                        localSpan.Text = $"{dbParagraph.RelativeNo}.\u00A0{localSpan.Text}";
                                    }
                                }
                            }
                        }

                        foreach (DocumentElement localParagraph in localSection.Blocks.ToArray())
                        {
                            var localParagraphCopy = CopyElement(localParagraph, targetDocument, mainStyle, mainStyleName);
                            section.Children.Add(localParagraphCopy);

                            if (localParagraph is TelerikParagraph paragraph1 && paragraph1.ListId != -1)
                            {
                                var docList = sourceDocument.ListManager.GetDocumentListById(paragraph1.ListId);
                                var listStyle = sourceDocument.ListManager.GetListStyleById(docList.StyleId);
                                if (!documentListDict.ContainsKey(docList.ID.ToString()))
                                {
                                    var documentListStyle = targetDocument.ListManager.GetListStyleById(dbParagraph.OrderNo.Value * 10000 + listStyle.ID); //GetAllListStyles().ToArray().Where(l => l.StyleLink == p.OrderNo + "_" + listStyle.ID.ToString()).First();
                                    var documentList = new DocumentList(documentListStyle, targetDocument);
                                    documentListDict.Add(docList.ID.ToString(), documentList);
                                }
                                documentListDict[docList.ID.ToString()].AddParagraph((TelerikParagraph)localParagraphCopy, paragraph1.ListLevel);
                            }
                        }
                    }
                }
            }

            SetMergeSource(targetDocument, GetDataForDocument(Paragraphs, variables));

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(variables)} = {variables?.Rows?.Count} строк, {nameof(filterDate)} = {filterDate}) отработал");

            return targetDocument;
        }

        public static ObservableCollection<ConditionGroup> GetConditions(string condition)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(condition)} = {condition})");

            if (string.IsNullOrEmpty(condition))
                return new ObservableCollection<ConditionGroup>();

            if (condition.Contains("ArrayOfConditionGroup"))
                return Extensions.Deserialize<ObservableCollection<ConditionGroup>>(condition);

            var conditionsTable = GetConditionsTableDefinition();

            conditionsTable.ReadXml(new StringReader(condition));

            var result = new ObservableCollection<ConditionGroup>();

            foreach (DataRow condRow in conditionsTable.Rows)
            {
                var group = new ConditionGroup()
                {
                    GroupType = GroupTypeEnum.Or
                };

                group.Conditions.Add(new Assets.Condition
                {
                    Variable = condRow["Variable1"].ToString(),
                    ConditionOperator = condRow["Condition1"].ToString(),
                    Value = condRow["Value1"].ToString()
                });

                if (!string.IsNullOrEmpty(condRow["Variable2"].ToString()))
                {
                    group.Conditions.Add(new Assets.Condition
                    {
                        Variable = condRow["Variable2"].ToString(),
                        ConditionOperator = condRow["Condition2"].ToString(),
                        Value = condRow["Value2"].ToString()
                    });
                }

                result.Add(group);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(condition)} = {condition}) отработал. Получено {result?.Count} записей");

            return result;
        }

        public static bool CheckConditionGroup(DataTable variables, string conditionXml)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(variables)} = {variables?.Rows?.Count} cтрок, {nameof(conditionXml)} = {conditionXml})");

            foreach (var group in GetConditions(conditionXml))
            {
                var groupRes = true;

                foreach (var condition in group.Conditions)
                {
                    groupRes = CheckCondition(variables, condition.Variable, condition.Operator, condition.Value);

                    if (groupRes && group.GroupType == GroupTypeEnum.Or)
                        break;

                    if (!groupRes && group.GroupType == GroupTypeEnum.And)
                        break;
                }

                if (!groupRes)
                {
                    LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(variables)} = {variables?.Rows?.Count} cтрок, {nameof(conditionXml)} = {conditionXml}) отработал. Получено {false}");

                    return false;
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(variables)} = {variables?.Rows?.Count} cтрок, {nameof(conditionXml)} = {conditionXml}) отработал. Получено {true}");

            return true;
        }

        public static byte[] GetCompareDocumentArray(Models.Document document)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name})");

            var result = GetCompareDocument(document.CurrentVersion);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}) отработал");

            return new DocxFormatProvider().Export(result);
        }

        public static RadDocument GetCompareDocument(Models.DocumentContent document)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Document?.Name} (ver. {document.VersionName}))");

            var targetDocument = new RadDocument();
            var mainStyle = default(StyleDefinition);
            var mainStyleName = string.Empty;

            var section = new Section();

            targetDocument.Sections.Add(section);

            if (!string.IsNullOrEmpty(document.Template))
            {
                var provider = new XamlFormatProvider();
                targetDocument = provider.Import(document.Template);

                (mainStyle, mainStyleName) = GetDocumentStyle(targetDocument);

                section = targetDocument.Sections.First();
            }

            var Paragraphs = document.Paragraphs.Where(y => y.Deleted == false).OrderBy(x => x.OrderNo).ToList();

            foreach (var dbParagraph in Paragraphs.Where(p => p.Deleted == false).OrderBy(pp => pp.OrderNo))
            {
                var contents = dbParagraph.ParentParagraph != null
                    ? dbParagraph.ParentParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList()
                    : dbParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList();

                if (contents.Count > 0)
                {
                    var paragraph = new TelerikParagraph();
                    var span = new Span($"\nУсловия для параграфа {dbParagraph.RelativeNo}. {dbParagraph.Name}")
                    {
                        ForeColor = Color.FromRgb(0, 0, 139)
                    };
                    paragraph.Inlines.Add(span);
                    section.Blocks.Add(paragraph);
                }

                foreach (var paragraphContentDefault in contents)
                {
                    var dbParagraphContent = paragraphContentDefault;

                    var textImporter = new XamlFormatProvider();

                    if (dbParagraphContent.Content is null) continue;

                    var sourceDocument = textImporter.Import(dbParagraphContent.Content.ToString());

                    foreach (var sourceListStyle in sourceDocument.ListManager.GetAllListStyles().ToArray())
                    {
                        var targetListStyle = new ListStyle(sourceListStyle)
                        {
                            ID = dbParagraph.OrderNo.Value * 10000 + sourceListStyle.ID
                        };

                        if (!(targetListStyle.NumStyleLink is null))
                        {
                            var x = sourceDocument.ListManager.GetAllListStyles().ToArray().Where(ll => ll.StyleLink == targetListStyle.NumStyleLink).First();
                            targetListStyle.NumStyleLink = string.Empty;

                            for (int levelIndex = 0; levelIndex < 9; levelIndex++)
                            {
                                var listLevel = x.Levels[levelIndex];

                                targetListStyle.Levels.Add(listLevel);
                            }
                        }

                        targetDocument.ListManager.RegisterListStyleIfNecessary(targetListStyle);
                    }

                    var documentListDict = new Dictionary<string, DocumentList>();

                    if ((!string.IsNullOrEmpty(dbParagraphContent.Tooltip) || dbParagraphContent.DefaultVersion == true))
                    {
                        var paragraph = new TelerikParagraph();

                        var span = new Span($"\n\t{(!string.IsNullOrEmpty(dbParagraphContent.Tooltip) ? dbParagraphContent.Tooltip : "Версия по умолчанию")}");
                        if (dbParagraphContent.Approved == true)
                        {
                            span.ForeColor = Color.FromRgb(0, 0, 139);
                        }
                        else
                        {
                            span.ForeColor = Color.FromRgb(255, 00, 0);
                        }
                        paragraph.Inlines.Add(span);

                        if (!string.IsNullOrEmpty(dbParagraphContent.Comment))
                        {
                            var commentSpan = new Span($" ({dbParagraphContent.Comment})")
                            {
                                ForeColor = Color.FromRgb(0, 140, 0)
                            };
                            paragraph.Inlines.Add(commentSpan);
                        }

                        section.Blocks.Add(paragraph);
                    }

                    foreach (var localSection in sourceDocument.Sections)
                    {
                        if (dbParagraph.Numerable == true)
                        {
                            var documentElement = localSection.Children.First();
                            if (documentElement is TelerikParagraph firstParagraph)
                            {
                                if (firstParagraph.Inlines.Count > 0)
                                {
                                    var firstInline = firstParagraph.Inlines.First();
                                    if (firstInline is Span localSpan)
                                    {
                                        localSpan.Text = $"{dbParagraph.RelativeNo}.\u00A0{localSpan.Text}";
                                    }
                                }
                            }
                        }

                        foreach (DocumentElement localParagraph in localSection.Blocks.ToArray())
                        {
                            var localParagraphCopy = CopyElement(localParagraph, targetDocument, mainStyle, mainStyleName);
                            section.Children.Add(localParagraphCopy);

                            if (localParagraph is TelerikParagraph paragraph1 && paragraph1.ListId != -1)
                            {
                                var docList = sourceDocument.ListManager.GetDocumentListById(paragraph1.ListId);
                                var listStyle = sourceDocument.ListManager.GetListStyleById(docList.StyleId);
                                if (!documentListDict.ContainsKey(docList.ID.ToString()))
                                {
                                    var documentListStyle = targetDocument.ListManager.GetListStyleById(dbParagraph.OrderNo.Value * 10000 + listStyle.ID); //GetAllListStyles().ToArray().Where(l => l.StyleLink == p.OrderNo + "_" + listStyle.ID.ToString()).First();
                                    var documentList = new DocumentList(documentListStyle, targetDocument);
                                    documentListDict.Add(docList.ID.ToString(), documentList);
                                }
                                documentListDict[docList.ID.ToString()].AddParagraph((TelerikParagraph)localParagraphCopy, paragraph1.ListLevel);
                            }
                        }
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Document?.Name} (ver. {document.VersionName})) отработал");

            return targetDocument;
        }

        public static (string, string) GetMainCodeAndRowId(ConnectionData connectionData, Models.Report reportDS, object rowId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportDS)} = {reportDS?.Reportname}, {nameof(rowId)} = {rowId})");

            var connection = ExtractDataEngine.GetConnection(connectionData.CreateConnection(reportDS.Servername?.Trim(), reportDS.Defaultdatabase));

            var reportCode = GetMainReportCode(connection, reportDS, rowId.ToString());
            var mainRowId = GetMainRowId(connection, reportDS, rowId.ToString());

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportDS)} = {reportDS?.Reportname}, {nameof(rowId)} = {rowId}) отработал. Получен: ReportCode: {reportCode}, RowId: {mainRowId}");

            return (reportCode, mainRowId);
        }

        public static (byte[] oldDoc, byte[] newDoc, DataSet oldDS, DataSet newDS) GetCompareDSDocuments(Models.DataModel context, ConnectionData connectionData, string reportCode, string mainRowId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(mainRowId)} = {mainRowId})");

            var audit = AuditHelper.GetAuditByCodeAndRowId(context, reportCode, Guid.Parse(mainRowId));

            if (audit == null)
                throw new ApplicationException($"Формирование доп. соглашения невозможно - не найдено записей аудита для кода \"{reportCode}\" и id \"{mainRowId}\"");

            var oldDoc = new XamlFormatProvider().Import(audit.Template);
            var oldDS = audit.DataSetFromDB;

            var report = context.Reports.FirstOrDefault(x => x.Reportcode == reportCode);
            var ds = ExtractDataEngine.GetDataSet(connectionData, report, mainRowId);
            var (newDoc, dt) = PrintDocument(report.Document, Transpose(ds.Tables[0]));

            var newDS = new DataSet();
            newDS.Tables.Add(dt);

            for (var i = 1; i < ds.Tables.Count; i++)
                newDS.Tables.Add(ds.Tables[i].Copy());

            var provider = new DocxFormatProvider();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(mainRowId)} = {mainRowId}) отработал.");

            return (provider.Export(oldDoc), provider.Export(newDoc), oldDS, newDS);
        }

        private static string GetMainReportCode(DbConnection connection, Models.Report report, string rowId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(connection)} = {connection?.ConnectionString}, {nameof(rowId)} = {rowId})");

            if (string.IsNullOrEmpty(report.SqlMainReportCode))
                return string.Empty;

            var query = string.Copy(report.SqlMainReportCode);
            query = !string.IsNullOrEmpty(rowId) ? query.Replace("?", $"'{rowId}'") : query;

            return ExecSqlQueryOle(connection, query, "ReportCode");
        }

        private static string GetMainRowId(DbConnection connection, Models.Report report, string rowId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(connection)} = {connection?.ConnectionString}, {nameof(rowId)} = {rowId})");

            if (string.IsNullOrEmpty(report.SqlMainRowId))
                return string.Empty;

            var query = string.Copy(report.SqlMainRowId);
            query = !string.IsNullOrEmpty(rowId) ? query.Replace("?", $"'{rowId}'") : query;

            return ExecSqlQueryOle(connection, query, "RowId");
        }

        private static string ExecSqlQueryOle(DbConnection connection, string query, string name)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(name)} = {name})");

            if (connection == null || string.IsNullOrEmpty(connection.ConnectionString) || string.IsNullOrEmpty(query))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(name)} = {name}) отработал. Получено: пусто");

                return string.Empty;
            }

            try
            {
                var table = new DataTable(connection.DataSource);

                var command = ExtractDataEngine.GetCommand(connection, query);
                command.CommandTimeout = 300;
                connection.Open();

                var reader = command.ExecuteReader();
                table.Load(reader);

                if (table.Rows.Count != 1)
                    throw new ApplicationException($"Запрос должен возвращать только одну запись. Возвращено записей: {table.Rows.Count}");

                if (!table.Rows[0].Table.Columns.Contains(name))
                    throw new ApplicationException($"Запись, которую вернул запрос ({query}), не содержит поле \"{name}\"");

                connection.Close();

                var result = table.Rows[0][name].ToString();

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(name)} = {name}) отработал. Получено: {result}");

                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Не удалось выполнить запрос. ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(name)} = {name}), exeption = {ex}");
            }
        }

        public static void FillDocumentChanges(Models.DataModel context, ConnectionData connectionData, byte[] revDoc, string reportCode, Guid rowId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(revDoc)} = {revDoc?.Length} байт, {nameof(reportCode)} = {reportCode}, {nameof(rowId)} = {rowId})");

            var revDocument = new DocxFormatProvider().Import(revDoc);
            var report = context.Reports.FirstOrDefault(x => x.Reportcode == reportCode);
            var changed = new List<DocumentElement>();

            foreach (var rev in revDocument.GetAllRevisions())
            {
                var startElem = (RevisionRangeStart)rev.RevisionElements.FirstOrDefault();
                var endElem = (RevisionRangeEnd)startElem.End;
                var startParagraph = startElem.Parent;

                if (startParagraph.Children.Any(x => x is DeleteRangeStart) && !startParagraph.Children.Any(x => x is InsertRangeStart))
                    continue;

                if (!changed.Contains(startParagraph))
                    changed.Add(startParagraph);

                var next = startElem.NextSibling;

                while (next != null && next != endElem)
                {
                    if (startElem is DeleteRangeStart)
                    {
                        startParagraph.Children.Remove(next);
                    }

                    next = next.NextSibling;
                }

                startParagraph.Children.Remove(endElem);
                startParagraph.Children.Remove(startElem);

                var endParagraph = endElem.Parent;

                if (endParagraph == null)
                    continue;

                if (!changed.Contains(endParagraph))
                    changed.Add(endParagraph);

                var prev = startElem.PreviousSibling;

                while (prev != null && prev != startElem)
                {
                    if (endElem is DeleteRangeEnd)
                    {
                        endParagraph.Children.Remove(prev);
                    }

                    prev = prev.PreviousSibling;
                }

                endParagraph.Children.Remove(endElem);
                endParagraph.Children.Remove(startElem);
            }

            var ds = ExtractDataEngine.GetDataSet(connectionData, report, rowId);
            var compare = GetCompareElements(report.Document, Transpose(ds.Tables[0]));
            var changeVersion = context.DocumentChanges.FirstOrDefault(x => x.ReportCode == reportCode && x.RowId == rowId)?.Version;

            foreach (var change in changed)
            {
                var text = GetTextFromDocumentElement(change);
                var intersection = compare.FirstOrDefault(x => x.Text == text);

                if (intersection != default)
                {
                    var paragraph = context.Paragraphs.FirstOrDefault(x => x.Id == intersection.paragraphId);
                    var version = context.ParagraphContents.FirstOrDefault(x => x.Id == intersection.versionId);
                    context.DocumentChanges.Add(new Models.DocumentChange
                    {
                        Id = Guid.NewGuid(),
                        ReportCode = reportCode,
                        RowId = rowId,
                        Version = (changeVersion ?? 0) + 1,
                        IsApplication = paragraph.IsAttachment,
                        IsChapter = paragraph.Level == 0,
                        Order = paragraph.RelativeNo,
                        Content = new TxtFormatProvider().Export(new XamlFormatProvider().Import(version.Content))
                    });
                }
            }

            context.SaveChanges();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(revDoc)} = {revDoc?.Length} байт, {nameof(reportCode)} = {reportCode}, {nameof(rowId)} = {rowId}) отработал.");
        }

        private static string GetTextFromDocumentElement(DocumentElement element)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var radDocument = new RadDocument();
            var section = new Section();

            radDocument.Sections.Add(section);
            section.Children.Add(element);

            var res = new TxtFormatProvider().Export(radDocument);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал. Получено: {res}");

            return res;
        }

        private static List<(Guid paragraphId, Guid versionId, string Text)> GetCompareElements(Models.Document document, DataTable reportVariables)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(reportVariables)} = {reportVariables?.Rows?.Count} строк)");

            var targetDocument = new RadDocument();
            var mainStyle = default(StyleDefinition);
            var mainStyleName = string.Empty;
            var result = new List<(Guid, Guid, string)>();

            var variables = CreateDocumentVariablesDataTable();

            if (!string.IsNullOrEmpty(document.Data))
            {
                var sr = new StringReader(document.Data);
                variables.ReadXml(sr);
            }

            if (reportVariables != null && reportVariables.Rows.Count > 0)
                foreach (DataRow row in reportVariables.Rows)
                    variables.Rows.Add(row.ItemArray);

            if (!string.IsNullOrEmpty(document.Template))
            {
                var provider = new XamlFormatProvider();
                targetDocument = provider.Import(document.Template);

                (mainStyle, mainStyleName) = GetDocumentStyle(targetDocument);
            }

            RecalculateParagraphsFilters(document, variables);
            var Paragraphs = document.Paragraph.Where(y => y.Deleted == false).OrderBy(x => x.OrderNo).ToList();
            var dt = GetDataForDocument(Paragraphs, variables);

            foreach (var dbParagraph in Paragraphs.Where(p => p.PassConditions == true && p.Deleted == false).OrderBy(pp => pp.OrderNo))
            {
                var contents = dbParagraph.ParentParagraph != null
                    ? dbParagraph.ParentParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList()
                    : dbParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList();

                var paragraphContentDefault = default(Models.ParagraphContent);
                foreach (var pc in contents)
                {
                    if (pc.DefaultVersion == true)
                    {
                        paragraphContentDefault = pc;
                    }

                    if (string.IsNullOrEmpty(pc.Condition) || (pc.DefaultVersion == true))
                    {
                        pc.PassConditions = true;
                        continue;
                    }

                    if (pc.ActiveFrom.HasValue & pc.ActiveFrom > DateTime.Today)
                    {
                        pc.PassConditions = false;
                        continue;
                    }

                    if (pc.ActiveTill.HasValue & pc.ActiveTill < DateTime.Today)
                    {
                        pc.PassConditions = false;
                        continue;
                    }

                    pc.PassConditions = CheckConditionGroup(variables, pc.Condition);
                }

                var dbParagraphContent = paragraphContentDefault;

                var filteredContents = contents.Where(p => p.PassConditions == true && (p.DefaultVersion == false || p.DefaultVersion is null));

                if (filteredContents.Count() == 1)
                {
                    dbParagraphContent = filteredContents.First();
                }
                else if (filteredContents.Count() > 1)
                {
                    throw new ApplicationException($"{dbParagraph.RelativeNo}{dbParagraph.Name} - Found too many versions");
                }

                if (dbParagraphContent is null)
                {
                    if (contents.Count() == 1)
                    {
                        dbParagraphContent = contents.First();
                    }
                    else
                    {
                        throw new ApplicationException($"{dbParagraph.RelativeNo}{dbParagraph.Name} - Version not found");
                    }
                }

                var textImporter = new XamlFormatProvider();

                if (dbParagraphContent.Content is null) continue;

                var sourceDocument = textImporter.Import(dbParagraphContent.Content.ToString());

                foreach (var sourceListStyle in sourceDocument.ListManager.GetAllListStyles().ToArray())
                {

                    var targetListStyle = new ListStyle(sourceListStyle)
                    {
                        ID = dbParagraph.OrderNo.Value * 10000 + sourceListStyle.ID
                    };

                    if (!(targetListStyle.NumStyleLink is null))
                    {
                        var x = sourceDocument.ListManager.GetAllListStyles().ToArray().Where(ll => ll.StyleLink == targetListStyle.NumStyleLink).First();
                        targetListStyle.NumStyleLink = string.Empty;

                        for (int levelIndex = 0; levelIndex < 9; levelIndex++)
                        {
                            var listLevel = x.Levels[levelIndex];

                            targetListStyle.Levels.Add(listLevel);
                        }
                    }

                    targetDocument.ListManager.RegisterListStyleIfNecessary(targetListStyle);
                }

                var documentListDict = new Dictionary<string, DocumentList>();
                SetMergeSource(sourceDocument, dt, FieldDisplayMode.Result);

                foreach (var localSection in sourceDocument.Sections)
                {
                    if (dbParagraph.Numerable == true)
                    {
                        var documentElement = localSection.Children.First();
                        if (documentElement is TelerikParagraph firstParagraph)
                        {
                            if (firstParagraph.Inlines.Count > 0)
                            {
                                var firstInline = firstParagraph.Inlines.First();
                                if (firstInline is Span localSpan)
                                {
                                    localSpan.Text = $"{dbParagraph.RelativeNo}.\u00A0{localSpan.Text}";
                                }
                            }
                        }
                    }

                    foreach (DocumentElement localParagraph in localSection.Blocks.ToArray())
                    {
                        result.Add((dbParagraph.Id, dbParagraphContent.Id, GetTextFromDocumentElement(localParagraph)));
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document?.Name}, {nameof(reportVariables)} = {reportVariables?.Rows?.Count} строк) отработал. Получено: {nameof(result)} = {result?.Count} элементов");

            return result;
        }

        public static void SetMergeSource(RadDocument document, DataTable dt, FieldDisplayMode displayMode = FieldDisplayMode.DisplayName)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            document.MailMergeDataSource.ItemsSource = dt.AsEnumerable();
            document.MailMergeDataSource.MoveToLast();
            document.MailMergeDataSource.MoveToPrevious();
            document.MailMergeDataSource.MoveToFirst();
            document.ChangeAllFieldsDisplayMode(displayMode);


            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");
        }
    }
}

