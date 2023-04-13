using System.Data;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Net;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Security.Principal;
using Aspose.BarCode.Generation;
using Aspose.Cells;
using Aspose.Cells.Rendering;
using Aspose.Words;
using Aspose.Words.Fields;
using Aspose.Words.Layout;
using Aspose.Words.MailMerging;
using Aspose.Words.Replacing;
using Aspose.Words.Tables;
using CrmWordAddIn.Models;
using CrmWordAddIn.Models.Concrete;
using DocumentsPackage;
using LicenseVerifierLib;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine.Models.Helpers;
using AsposeDocument = Aspose.Words.Document;
using AsposeParagraph = Aspose.Words.Paragraph;
using AsposeFormat = Aspose.Words.SaveFormat;
using WordMergeEngine.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
//System.Data.Entity.Infrastructure, System.Data.Entity.EntityState

namespace WordMergeEngine
{
    public static class DocBuilder
    {
        private static Stream _asposeLicense;

        private static (AsposeDocument document, string spFolder) MergeDocument(DataModel dbContext, ConnectionData connectionData, Report report, object rowId, AsposeFormat format, Guid? auditId = null, bool isWaterMarkRequired = false)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(format)} = {format}, {nameof(auditId)} = {auditId})");

            var dataSet = ExtractDataEngine.GetDataSet(connectionData, report, rowId);

            SetWordLic();

            var doc = new AsposeDocument(report.Reportpath);
            var barCode = auditId != null ? dbContext.Audits.FirstOrDefault(x => x.Id == auditId)?.BarCode : null;
            var settings = dbContext.GlobalSettings.FirstOrDefault();
            var result = BuildDocument(report, dataSet, doc, format, barCode, settings);

            if (isWaterMarkRequired)
                CheckLicenseAddWaterMark(result);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(format)} = {format}, {nameof(auditId)} = {auditId}) отработал.");

            return (result, GetSpFolder(report, dataSet));
        }

        private static void CheckLicenseAddWaterMark(AsposeDocument result)
        {
            LogHelper.GetLogger().Debug("Запущен метод проверки лицензии");

            var isLicenseValid = false;

            try
            {
                isLicenseValid = new LicenseVerifier().IsLicenseValid("WordMerger");
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger().Debug($"Произошла ошибка. Лицензия не валидна. {ex.Message}");
            }

            if (isLicenseValid)
            {
                LogHelper.GetLogger().Debug("Метод проверки лицензии отработал успешно. Лицензия валидна");
                return;
            }

            result.Protect(Aspose.Words.ProtectionType.ReadOnly);

            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WordMergeEngine.Assets.Watermark.demo.png");
            var image = Image.FromStream(imageStream);

            result.Watermark.SetImage(image);

            SetVisibleWatermark(result);

            result.Protect(Aspose.Words.ProtectionType.ReadOnly);

            LogHelper.GetLogger().Debug("Метод проверки лицензии отработал успешно. Лицензия не валидна");
        }

        private static void SetVisibleWatermark(AsposeDocument doc)
        {
            var builder = new DocumentBuilder(doc);

            var smallRuns = doc.GetChildNodes(NodeType.Run, true);
            var collector = new LayoutCollector(doc);

            int pageIndex = 1;
            foreach (var run in smallRuns)
            {
                if (collector.GetStartPageIndex(run) == pageIndex)
                {
                    var watermark = new Aspose.Words.Drawing.Shape(doc, Aspose.Words.Drawing.ShapeType.Image);

                    builder.MoveTo(run);
                    builder.InsertNode(watermark);

                    pageIndex++;
                }
            }
        }

        private static void CheckLicenseAddWaterMark(Workbook workbook)
        {
            LogHelper.GetLogger().Debug($"Запущен метод проверки лицензии");

            var isLicenseValid = false;

            try
            {
                isLicenseValid = new LicenseVerifier().IsLicenseValid("WordMerger");
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger().Debug($"Произошла ошибка. Лицензия не валидна. {ex.Message}");
            }

            if (isLicenseValid)
            {
                LogHelper.GetLogger().Debug($"Метод проверки лицензии отработал успешно. Лицензия валидна");
                return;
            }

            SetWatermarkAsImage(workbook);

            LogHelper.GetLogger().Debug($"Метод проверки лицензии отработал успешно. Лицензия не валидна");
        }

        private static AsposeDocument BuildDocument(Report report, DataSet dataSet, AsposeDocument document, AsposeFormat? format = null, string barCode = null, GlobalSetting settings = null)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц)");

            document.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedRegions;
            document.MailMerge.TrimWhitespaces = false;
            document.MailMerge.FieldMergingCallback = new HandleMergeFieldAlternatingRows();
            document.MailMerge.ExecuteWithRegions(dataSet);

            if (dataSet.Tables.Count > 0)
                document.MailMerge.Execute(dataSet.Tables[0]);

            InsertImagesAndBarcode(document, dataSet, barCode);

            try
            {
                document.MailMerge.DeleteFields();
                document.UpdateFields();
            }
            catch (Exception ex)
            {
                if (ex.Source == "Aspose.Words" && ex.Message == "Unrecognized node type.")
                    throw new ApplicationException($"{ex.Message} Проверьте разметку. Ошибка вызвана:\n1. Неверным форматом поля слияния или ошибки в названии.\n2. Незавершенным перемещением или вставкой поля слияния (необходимо \"Принять вставку\" или \"Принять перемещение\" для поля).", ex);
            }

            if (report.Replacefieldswithstatictext == true)
                FieldsHelper.ConvertFieldsToStaticText(document, FieldType.FieldIf);


            foreach (Table table in document.GetChildNodes(NodeType.Table, true))
            {
                SetUnbreakableTable(table);
                SetUnbreakableRows(table);
            }

            SetIsolatedNumberedList(document);
            SetKeepWithNextParagraph(document);

            if (settings != null && settings.IsRemoveHighlight && (bool)report.IsChangeColor && format == AsposeFormat.Pdf)
            {
                var visitor = new ChangeHighlightColor(ColorTranslator.FromHtml(settings.DefaultColorText));
                document.Accept(visitor);
            }

            document.UpdatePageLayout();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц) отработал.");

            return document;
        }

        private static void SetUnbreakableTable(Table table)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table})");

            if (!CheckTagInTableNode(table, Constants.Unbreakable))
                return;

            foreach (Aspose.Words.Tables.Cell cell in table.GetChildNodes(NodeType.Cell, true))
            {
                cell.EnsureMinimum();
                foreach (Aspose.Words.Paragraph paragraph in cell.Paragraphs.Where(p => !(cell.ParentRow.IsLastRow && ((Aspose.Words.Paragraph)p).IsEndOfCell)))
                    paragraph.ParagraphFormat.KeepWithNext = true;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table}) отработал.");
        }

        private static void SetUnbreakableRows(Table table)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table})");

            foreach (Aspose.Words.Tables.Row row in table.Rows)
            {
                if (!CheckTagInTableNode(row, Constants.UnbreakableRow))
                    continue;

                row.RowFormat.AllowBreakAcrossPages = false;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(table)} = {table}) отработал.");
        }

        private static bool CheckTagInTableNode(CompositeNode node, string tag)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(node)} = {node}, {nameof(tag)} = {tag})");

            var isContains = false;

            foreach (Aspose.Words.Tables.Cell cell in node.GetChildNodes(NodeType.Cell, true))
            {
                if (!cell.Range.Text.Contains(tag))
                    continue;

                isContains = true;
                cell.Range.Replace(tag, string.Empty);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(node)} = {node}, {nameof(tag)} = {tag}) отработал.");

            return isContains;
        }

        private static void SetIsolatedNumberedList(AsposeDocument document) => document.Range.Replace(Constants.IsolatedNumberedList, "{0}.", new FindReplaceOptions(new NumberedReplacementRecorder()));

        private static void SetKeepWithNextParagraph(AsposeDocument document)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document})");

            foreach (Aspose.Words.Paragraph paragraph in document.GetChildNodes(NodeType.Paragraph, true))
            {
                if (!paragraph.GetText().Contains(Constants.KeepWithNext))
                    continue;

                paragraph.Range.Replace(Constants.KeepWithNext, string.Empty);
                paragraph.ParagraphFormat.KeepWithNext = true;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document}) отработал.");
        }

        private static void InsertImagesAndBarcode(AsposeDocument document, DataSet dataSet, string barCode)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц)");

            SetBarCodeLic();

            var tables = dataSet.Tables.Cast<DataTable>();

            foreach (var field in document.Range.Fields.OfType<FieldMergeField>().Where(x => x.FieldName.Contains("Barcode")))
            {
                if (string.IsNullOrEmpty(barCode))
                {
                    var columnName = field.FieldName.Replace("Barcode:", "");

                    var table = tables.FirstOrDefault(x => x.Columns.Cast<DataColumn>().Any(z => z.ColumnName == columnName));
                    if (table == null)
                        continue;

                    barCode = table.Rows.Cast<DataRow>().Select(x => x.Field<string>(columnName)).FirstOrDefault();

                    if (string.IsNullOrEmpty(barCode))
                        continue;
                }

                var barCodeBuilder = new BarcodeGenerator(EncodeTypes.EAN13, barCode);
                barCodeBuilder.Parameters.AutoSizeMode = AutoSizeMode.Interpolation;
                barCodeBuilder.Parameters.ImageWidth.Millimeters = 30f;
                barCodeBuilder.Parameters.ImageHeight.Millimeters = 20f;
                barCodeBuilder.Parameters.Barcode.Padding.Left.Millimeters = 1;
                barCodeBuilder.Parameters.Barcode.Padding.Right.Millimeters = 1;
                barCodeBuilder.Parameters.Barcode.Padding.Bottom.Millimeters = 1;
                barCodeBuilder.Parameters.Barcode.Padding.Top.Millimeters = 1;
                barCodeBuilder.Parameters.Barcode.CodeTextParameters.FontMode = FontMode.Manual;
                barCodeBuilder.Parameters.Barcode.CodeTextParameters.Font.Size.Point = 8f;

                var documentBuilder = new DocumentBuilder(document);
                documentBuilder.MoveToField(field, false);
                documentBuilder.InsertImage(barCodeBuilder.GenerateBarCodeImage());
            }

            foreach (var group in document.Range.Fields.Cast<Field>().Where(x => x.Result.Contains("ImageFromLinkTagHeight") || x.Result.Contains("ImageFromLinkTag") || x.Result.Contains("ImageFromLinkTagBoth")).GroupBy(x => x.Result))
            {
                var groupList = group.ToList();

                for (var i = 0; i < groupList.Count; i++)
                {
                    var isRelativeToHeight = groupList[i].Result.Contains("ImageFromLinkTagHeight");
                    var isRelativeToBoth = groupList[i].Result.Contains("ImageFromLinkTagBoth");

                    var columnName = groupList[i].Result.Replace("«ImageFromLinkTagHeight:", "").Replace("«ImageFromLinkTagBoth:", "").Replace("«ImageFromLinkTag:", "").Replace("»", "");

                    var table = tables.FirstOrDefault(x => x.Columns.Cast<DataColumn>().Any(z => z.ColumnName == columnName));
                    if (table == null)
                        continue;

                    var urls = table.Rows.Cast<DataRow>().Select(x => x.Field<string>(columnName)).ToList();

                    if (urls.Count < i + 1 || string.IsNullOrEmpty(urls[i]))
                        continue;

                    using (var webClient = new WebClient())
                    {
                        webClient.Credentials = CredentialCache.DefaultNetworkCredentials;

                        var documentBuilder = new DocumentBuilder(document);
                        documentBuilder.MoveToField(groupList[i], false);
                        var imageShape = documentBuilder.InsertImage(webClient.DownloadData(urls[i]));

                        if (isRelativeToHeight)
                        {
                            var imageHeight = GetImageDimension(groupList[i], true);

                            if (!imageHeight.HasValue)
                                continue;

                            imageShape.Width = imageHeight.Value * (imageShape.Width / imageShape.Height);
                            imageShape.Height = imageHeight.Value;

                            continue;
                        }

                        if (isRelativeToBoth)
                        {
                            var height = GetImageDimension(groupList[i], true);
                            var width = GetImageDimension(groupList[i], false);

                            if (!height.HasValue || !width.HasValue)
                                continue;

                            imageShape.Height = height.Value;
                            imageShape.Width = width.Value;

                            continue;
                        }

                        var imageWidth = GetImageDimension(groupList[i], false);

                        if (!imageWidth.HasValue)
                            continue;

                        imageShape.Height = imageWidth.Value / (imageShape.Width / imageShape.Height);
                        imageShape.Width = imageWidth.Value;
                    }
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document}, {nameof(dataSet)} = {dataSet?.Tables?.Count} таблиц) отработал.");
        }

        private static double? GetImageDimension(Field field, bool isHeight)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(field)} = {field}, {nameof(isHeight)} = {isHeight})");

            if (field.Start == null || field.Start.ParentNode == null || !(field.Start.ParentNode.ParentNode is Aspose.Words.Tables.Cell))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(field)} = {field}, {nameof(isHeight)} = {isHeight}) отработал.");

                return null;
            }

            var currentCell = (Aspose.Words.Tables.Cell)field.Start.ParentNode.ParentNode;

            var result = isHeight ? currentCell.ParentRow.RowFormat.Height - currentCell.CellFormat.TopPadding - currentCell.CellFormat.BottomPadding
                                  : currentCell.CellFormat.Width - currentCell.CellFormat.LeftPadding - currentCell.CellFormat.RightPadding;

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(field)} = {field}, {nameof(isHeight)} = {isHeight}) отработал. Результат: {result}");

            return result;
        }

        private static void SetBarCodeLic()
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var lic = new Aspose.BarCode.License();

            var assembly = Assembly.GetExecutingAssembly();
            _asposeLicense = assembly.GetManifestResourceStream("WordMergeEngine.Aspose.BarCode.lic");

            lic.SetLicense(_asposeLicense);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");
        }

        private static void SetWordLic()
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var lic = new Aspose.Words.License();

            var assembly = Assembly.GetExecutingAssembly();
            _asposeLicense = assembly.GetManifestResourceStream("WordMergeEngine.Aspose.Words.lic");

            lic.SetLicense(_asposeLicense);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");
        }

        private static void SetCellLic()
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var lic = new Aspose.Cells.License();

            var assembly = Assembly.GetExecutingAssembly();
            _asposeLicense = assembly.GetManifestResourceStream("WordMergeEngine.Aspose.Cells.lic");

            lic.SetLicense(_asposeLicense);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");
        }

        private static Workbook MergeWorkbook(ConnectionData connectionData, Report report, Object rowId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId})");

            SetCellLic();

            var dsTables = ExtractDataEngine.GetDataSet(connectionData, report, rowId).Tables;

            var workbook = Helper.ShiftAreas(new Workbook(report.Reportpath), dsTables);

            var sourceCellList = new List<SourceCell>();

            var tempworkbook = new Workbook(report.Reportpath);
            tempworkbook.Worksheets.Clear();

            var tables = new List<string>();
            var errorName = string.Empty;
            var ignoreDeleteSheets = new List<string>();

            for (var j = 0; j < workbook.Worksheets.Count; j++)
            {
                if (workbook.Worksheets[j].Name.StartsWith("{") && workbook.Worksheets[j].Name.EndsWith("}"))
                {
                    if (!dsTables.Contains(workbook.Worksheets[j].Name.Substring(1, workbook.Worksheets[j].Name.Length - 2)))
                    {
                        if (string.IsNullOrEmpty(errorName))
                            errorName = workbook.Worksheets[j].Name.Substring(1, workbook.Worksheets[j].Name.Length - 2);
                    }

                    foreach (DataTable t in dsTables)
                    {
                        if ("{" + t.TableName + "}" == workbook.Worksheets[j].Name)
                        {
                            if (!tables.Contains(t.TableName))
                                tables.Add(t.TableName);

                            var rowindex = 1;

                            foreach (DataRow tablerow in t.Rows)
                            {
                                var newsheet = tempworkbook.Worksheets.Add(t.TableName + (t.Rows.Count > 1 ? " " + rowindex++ : ""));
                                newsheet.Copy(workbook.Worksheets[j]);

                                foreach (DataRelation item in t.ChildRelations)
                                {
                                    var tagName = item.ChildTable.TableName;
                                    var childRows = tablerow.GetChildRows(item);

                                    var cell = newsheet.Cells.Find("{" + tagName + "}", null, new FindOptions() { LookInType = LookInType.Values, LookAtType = LookAtType.StartWith });

                                    if (cell != null)
                                    {
                                        sourceCellList.Add(new SourceCell(tagName, cell.Name, newsheet.Name, childRows.Count()));

                                        var startCol1 = cell.Column;
                                        var startRow1 = cell.Row;

                                        if (childRows.Count() == 0)
                                        {
                                            var style = newsheet.Cells[startRow1, startCol1].GetStyle();

                                            newsheet.Cells[startRow1, startCol1].PutValue("");
                                            newsheet.Cells[startRow1, startCol1].SetStyle(style);

                                            continue;
                                        }

                                        for (var k = 0; k < childRows.Count(); k++)
                                        {
                                            var r = childRows[k];

                                            var colPos = 0;

                                            for (var i = 0; i < r.ItemArray.Count(); i++)
                                            {
                                                if (!item.ChildTable.Columns[i].ColumnName.StartsWith("ignore_", StringComparison.CurrentCultureIgnoreCase) && item.ChildTable.Columns[i].ColumnName != "mcdsoft__flag_reserved")
                                                {
                                                    if (!item.ChildTable.Columns[i].ColumnName.StartsWith("skipcolumn_", StringComparison.CurrentCultureIgnoreCase))
                                                        newsheet.Cells[startRow1 + k, startCol1 + colPos].PutValue(r.ItemArray.ElementAt(i));

                                                    colPos++;
                                                }
                                            }
                                        }
                                    }
                                }

                                for (int i = 0; i < t.Columns.Count; i++)
                                {
                                    var rowmember = t.Columns[i];

                                    for (var was = true; was;)
                                    {
                                        was = false;
                                        var cell = newsheet.Cells.Find("{" + rowmember.ColumnName + "}", null, new FindOptions() { LookInType = LookInType.Values, LookAtType = LookAtType.EntireContent });

                                        if (cell != null)
                                        {
                                            cell.PutValue(tablerow.ItemArray.ElementAt(i));
                                            was = true;
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                else
                    ignoreDeleteSheets.Add(workbook.Worksheets[j].Name);
            }

            var workSheetList = new Dictionary<int, string>();

            foreach (Worksheet sheet in workbook.Worksheets)
                workSheetList.Add(sheet.Index, sheet.Name);

            for (int i = 0; i < workbook.Worksheets.Count; i++)
            {
                if (ignoreDeleteSheets.FindIndex(x => x == workbook.Worksheets[i].Name) == -1)
                {
                    workbook.Worksheets.RemoveAt(i);
                    i = -1;
                }
            }

            var localOffset = 0;
            var currentIndex = 0;

            foreach (Worksheet sheet in tempworkbook.Worksheets)
            {
                if (workSheetList.Where(x => sheet.Name.IndexOf(x.Value.Replace("}", "").Replace("{", "")) != -1).Count() != 0)
                {
                    var insertSheet = workSheetList.Where(x => sheet.Name.IndexOf(x.Value.Replace("}", "").Replace("{", "")) != -1).FirstOrDefault();

                    if (localOffset == 0)
                        currentIndex = insertSheet.Key;

                    if (currentIndex != insertSheet.Key)
                        localOffset--;

                    workbook.Worksheets.Insert(insertSheet.Key + localOffset, sheet.Type, sheet.Name).Copy(sheet);

                    if (currentIndex == insertSheet.Key)
                        localOffset++;
                    else
                    {
                        currentIndex = insertSheet.Key;
                        localOffset++;
                    }
                }
            }

            foreach (DataTable t in dsTables)
            {
                foreach (Worksheet sheet in workbook.Worksheets)
                {
                    var cell = sheet.Cells.Find("{" + t.TableName + "}", null, new FindOptions() { LookInType = LookInType.Values, LookAtType = LookAtType.StartWith });

                    if (cell != null)
                    {
                        sourceCellList.Add(new SourceCell(t.TableName, cell.Name, sheet.Name, t.Rows.Count));

                        sheet.Cells[cell.Row, cell.Column].PutValue("");

                        if (!tables.Contains(t.TableName))
                            tables.Add(t.TableName);

                        var startCol = cell.Column;
                        var startRow = cell.Row;

                        for (var j = 0; j < t.Rows.Count; j++)
                        {
                            var r = t.Rows[j];

                            var colPos = 0;

                            for (var i = 0; i < r.ItemArray.Count(); i++)
                            {
                                if (!t.Columns[i].ColumnName.StartsWith("ignore_", StringComparison.CurrentCultureIgnoreCase) && t.Columns[i].ColumnName != "mcdsoft__flag_reserved")
                                {
                                    if (!t.Columns[i].ColumnName.StartsWith("skipcolumn_", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (t.Columns[i].ColumnName.EndsWith("_url"))
                                        {
                                            string url;
                                            string displayText;

                                            var cellValue = r.ItemArray.ElementAt(i).ToString();

                                            Helper.SplitHyperlinkValue(cellValue, out url, out displayText);

                                            sheet.Cells[startRow + j, startCol + colPos].PutValue(displayText);
                                            sheet.Hyperlinks.Add(startRow + j, startCol + colPos, 1, 1, url);
                                        }
                                        else
                                            sheet.Cells[startRow + j, startCol + colPos].PutValue(r.ItemArray.ElementAt(i));
                                    }
                                    colPos++;
                                }
                            }
                        }
                    }
                }
            }

            foreach (DataTable item in dsTables)
            {
                if (!tables.Contains(item.TableName) && item.ParentRelations != null && item.ParentRelations.Count != 0 && !string.IsNullOrEmpty(errorName))
                    throw new ApplicationException($"Не задан источник данных \"{errorName}\".");
            }

            try
            {
                workbook = Helper.FillCharts(workbook, sourceCellList);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при работе с графиками: {ex.Message}");
            }

            workbook.CalculateFormula(true);

            if (!new LicenseVerifier().IsLicenseValid("WordMerger"))
                CheckLicenseAddWaterMark(workbook);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}) отработал.");

            return workbook;
        }

        public static PrintableDocument PrintDocument(DataModel dbContext, ConnectionData connectionData, string reportCode, object rowId, bool forPackageMerging = false, bool isWaterMarkRequired = true)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(rowId)} = {rowId}, {nameof(forPackageMerging)} = {forPackageMerging})");

            PrintableDocument doc;

            var report = (from p in dbContext.Reports where p.Reportcode == reportCode orderby p.Reportname select p).FirstOrDefault();

            if (report == null)
            {
                doc = new PrintableDocument();

                doc.TemplateId = reportCode;
                doc.PrintErrors.Add($"Не удалось найти документ с кодом '{reportCode}'");

                return doc;
            }
            else
            {
                doc = PrintDocument(dbContext, connectionData, report, rowId, null, forPackageMerging, isWaterMarkRequired: isWaterMarkRequired).document;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportCode)} = {reportCode}, {nameof(rowId)} = {rowId}, {nameof(forPackageMerging)} = {forPackageMerging}) отработал. Результат: ({nameof(doc.Data)} = {doc?.Data?.Length} байт, {nameof(doc.FileName)} = {doc?.FileName}, {nameof(doc.NeedSaveToSharePoint)} = {doc?.NeedSaveToSharePoint}, {nameof(doc.PrintErrors)} = {doc?.PrintErrors?.Count} ошибки, {nameof(doc.TemplateId)} = {doc?.TemplateId})");

            return doc;
        }

        public static (PrintableDocument document, Guid? auditId) PrintDocument(DataModel dbContext, ConnectionData connectionData, Report report, object rowId, Dictionary<string, string> parameters, bool forPackageMerging = false, string chooseFormat = null, Action<Guid> auditAction = null, bool isIgnoreCondition = false, bool isWaterMarkRequired = true)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(parameters)} = {parameters?.Count} параметров, {nameof(forPackageMerging)} = {forPackageMerging})");

            var resultDoc = new PrintableDocument();

            resultDoc.TemplateId = report.Reportcode;
            resultDoc.NeedSaveToSharePoint = report.SharepointDosave ?? false;

            if (!isIgnoreCondition)
                resultDoc.PrintErrors.AddRange(ExtractDataEngine.GetErrors(connectionData, report, rowId, parameters != null && parameters.ContainsKey("userid") && Guid.TryParse(parameters["userid"], out Guid id) ? id : Guid.Empty));

            var auditId = default(Guid?);

            if (resultDoc.PrintErrors.Count == 0)
            {
                var connection = ExtractDataEngine.GetConnection(connectionData.CreateConnection(report.Servername, report.Defaultdatabase));
                var fileName = GetFileName(report, connection, $"'{rowId.ToString()}'", chooseFormat, forPackageMerging);

                UnchangedContext(dbContext);

                auditId = AuditHelper.AddAuditItem(dbContext, report.Reportcode, Guid.Parse(rowId.ToString()), parameters);
                auditAction?.Invoke(auditId.Value);

                if (report.IsCustomTemplate)
                {
                    var format = GetSaveFormat(report, chooseFormat);
                    printWordDocument(MergeCustomDocument(dbContext, connectionData, report, rowId, auditId.Value, format, isWaterMarkRequired: isWaterMarkRequired), fileName, resultDoc, format);
                }
                else if (report.Reportpath.EndsWith("doc") || report.Reportpath.EndsWith("docx") || report.Reportpath.EndsWith("docm"))
                {
                    var format = GetSaveFormat(report, chooseFormat);
                    printWordDocument(MergeDocument(dbContext, connectionData, report, rowId, format, auditId, isWaterMarkRequired: isWaterMarkRequired), fileName, resultDoc, format);
                }
                else
                    printWorkbook(MergeWorkbook(connectionData, report, rowId), fileName, resultDoc, GetFileFormatType(report));
            }
            else if (!forPackageMerging)
                throw new ConditionException(resultDoc.PrintErrors.Aggregate(string.Empty, (current, err) => "" + current + err + "\n"));

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(forPackageMerging)} = {forPackageMerging}) отработал. Результат: ({nameof(resultDoc.Data)} = {resultDoc?.Data?.Length} байт, {nameof(resultDoc.FileName)} = {resultDoc?.FileName}, {nameof(resultDoc.NeedSaveToSharePoint)} = {resultDoc?.NeedSaveToSharePoint}, {nameof(resultDoc.PrintErrors)} = {resultDoc?.PrintErrors?.Count} ошибки, {nameof(resultDoc.TemplateId)} = {resultDoc?.TemplateId})");

            return (resultDoc, auditId);
        }

        private static void UnchangedContext(DataModel context)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var collection = context.ChangeTracker.Entries().Where(x => x.State == EntityState.Modified); //разные числовые значения, должен быть System.Data.Entity
            collection.ToList().ForEach(x => x.State = EntityState.Unchanged);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал. Результат: ()");
        }

        public static (AsposeDocument, string) MergeCustomDocument(DataModel dbContext, ConnectionData connectionData, Report report, object rowId, Guid auditId, AsposeFormat format, bool isWaterMarkRequired = false)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(auditId)} = {auditId}, {nameof(format)} = {format}, {nameof(isWaterMarkRequired)} = {isWaterMarkRequired})");

            SetWordLic();

            var dataSet = ExtractDataEngine.GetDataSet(connectionData, report, rowId);
            var mainTable = dataSet.Tables[0];

            var (docBA, dt) = report.IsDs ? PrintDSWordDocument(dbContext, connectionData, report, rowId, mainTable, auditId) : TelerikBuilder.PrintWordDocument(dbContext, report, rowId, mainTable, auditId);

            var newDataSet = new DataSet();
            newDataSet.Tables.Add(dt);

            for (var i = 1; i < dataSet.Tables.Count; i++)
                newDataSet.Tables.Add(dataSet.Tables[i].Copy());

            AuditHelper.AddDataSetInAudit(dbContext, auditId, newDataSet);

            var barCode = dbContext.Audits.First(x => x.Id == auditId).BarCode;
            var settings = dbContext.GlobalSettings.FirstOrDefault();

            using (var ms = new MemoryStream(docBA))
            {
                var result = BuildDocument(report, newDataSet, new AsposeDocument(ms), format, barCode, settings);

                if (isWaterMarkRequired)
                    CheckLicenseAddWaterMark(result);

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(auditId)} = {auditId}, {nameof(format)} = {format}) отработал.");

                return (result, GetSpFolder(report, dataSet));
            }
        }

        private static string GetSpFolder(Report report, DataSet dataSet)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {dataSet?.Tables?.Count} таблиц)");

            var result = string.Empty;

            if (!string.IsNullOrEmpty(report.PathKeyFieldName) && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                result = dataSet.Tables[0].Rows[0][report.PathKeyFieldName]?.ToString();
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {dataSet?.Tables?.Count} таблиц) отработал. Получено: {result}");

            return result;
        }

        private static AsposeFormat GetSaveFormat(Report report, string chooseFormat)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(chooseFormat)} = {chooseFormat})");

            AsposeFormat result;

            if (report.IsChooseFormat && !string.IsNullOrEmpty(chooseFormat))
            {
                switch (chooseFormat)
                {
                    case "docx":
                        result = AsposeFormat.Docx;
                        break;

                    case "pdf":
                        result = AsposeFormat.Pdf;
                        break;

                    case "xps":
                        result = AsposeFormat.Xps;
                        break;

                    case "html":
                        result = AsposeFormat.Html;
                        break;

                    default:
                        throw new ApplicationException($"Выбран неизвестный формат - {chooseFormat}");
                }
            }
            else if (string.IsNullOrEmpty(report.Reportformat) && report.IsCustomTemplate)
                result = AsposeFormat.Docx;
            else if (string.IsNullOrEmpty(report.Reportformat))
            {
                var docExt = Path.GetExtension(report.Reportpath).ToLower();

                switch (docExt)
                {
                    case ".doc":
                        result = AsposeFormat.Doc;
                        break;

                    case ".docx":
                        result = AsposeFormat.Docx;
                        break;

                    case ".docm":
                        result = AsposeFormat.Docm;
                        break;

                    default:
                        throw new ApplicationException($"Файл документа имеет неизвестное расширение - {docExt}");
                }
            }
            else if (report.Reportformat.ToLower() == "pdf")
                result = AsposeFormat.Pdf;
            else if (report.Reportformat.ToLower() == "xps")
                result = AsposeFormat.Xps;
            else if (report.Reportformat.ToLower() == "html")
                result = AsposeFormat.Html;
            else
                throw new ApplicationException($"Для документа задан неподдерживаемый формат - {report.Reportformat}");

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(chooseFormat)} = {chooseFormat}) отработал. Результат: {result}");

            return result;
        }

        private static Aspose.Cells.SaveFormat GetFileFormatType(Report report)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname})");

            var fileFormat = new Aspose.Cells.SaveFormat();

            if (string.IsNullOrEmpty(report.Reportformat))
            {
                var docExt = Path.GetExtension(report.Reportpath).ToLower();

                if (docExt == ".xls")
                    fileFormat = Aspose.Cells.SaveFormat.Excel97To2003;
                else if (docExt == ".xlsx")
                    fileFormat = Aspose.Cells.SaveFormat.Xlsx;
                else
                    throw new ApplicationException($"Файл документа имеет неизвестное расширение - {docExt}");
            }
            else if (report.Reportformat.ToLower() == "pdf")
                fileFormat = Aspose.Cells.SaveFormat.Pdf;
            else if (report.Reportformat.ToLower() == "xps")
                throw new ApplicationException("Для excel-шаблонов не поддерживается формат xps");
            else if (report.Reportformat.ToLower() == "html")
                fileFormat = Aspose.Cells.SaveFormat.Html;
            else
                throw new ApplicationException($"Для документа задан неподдерживаемый формат - {report.Reportformat}");

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}) отработал. Результат: {fileFormat}");

            return fileFormat;
        }

        private static void printWorkbook(Workbook wb, string fileName, PrintableDocument resultDoc, Aspose.Cells.SaveFormat fileFormat)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(fileName)} = {fileName}, {nameof(resultDoc.FileName)} = {resultDoc?.FileName}, {nameof(resultDoc.Data)} = {resultDoc?.Data?.Length} байт, {nameof(fileFormat)} = {fileFormat})");

            var ms = new MemoryStream();
            resultDoc.FileName = fileName;
            wb.Save(ms, fileFormat);
            resultDoc.Data = ms.ToArray();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(fileName)} = {fileName}, {nameof(resultDoc.FileName)} = {resultDoc?.FileName}, {nameof(resultDoc.Data)} = {resultDoc?.Data?.Length} байт, {nameof(fileFormat)} = {fileFormat}) отработал.");
        }

        private static void printWordDocument((AsposeDocument doc, string spFolder) document, string fileName, PrintableDocument resultDoc, Aspose.Words.SaveFormat saveFormat)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(fileName)} = {fileName}, {nameof(resultDoc.FileName)} = {resultDoc?.FileName}, {nameof(resultDoc.Data)} = {resultDoc?.Data?.Length} байт, {nameof(saveFormat)} = {saveFormat})");

            var ms = new MemoryStream();
            resultDoc.FileName = fileName;
            resultDoc.SpFolder = document.spFolder;

            if (saveFormat == Aspose.Words.SaveFormat.Html)
            {
                var saveOptions = new Aspose.Words.Saving.HtmlSaveOptions(saveFormat);
                saveOptions.ExportImagesAsBase64 = true;

                document.doc.Save(ms, saveOptions);
            }
            else
                document.doc.Save(ms, saveFormat);

            resultDoc.Data = ms.ToArray();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(fileName)} = {fileName}, {nameof(resultDoc.FileName)} = {resultDoc?.FileName}, {nameof(resultDoc.Data)} = {resultDoc?.Data?.Length} байт, {nameof(saveFormat)} = {saveFormat}) отработал.");
        }

        public static byte[] ConvertToXps(byte[] filebytes, FileFormat format)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(filebytes)} = {filebytes?.Length} байт, {nameof(format)} = {format})");

            switch (format)
            {
                case FileFormat.doc:
                    {
                        SetWordLic();

                        var ms = new MemoryStream(filebytes);
                        var doc = new AsposeDocument(ms);
                        var output = new MemoryStream();

                        doc.Save(output, Aspose.Words.SaveFormat.Xps);

                        var data = output.ToArray();
                        output.Close();
                        ms.Close();

                        LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(filebytes)} = {filebytes?.Length} байт, {nameof(format)} = {format}) отработал. Получено: {data?.Length} байт");

                        return data;
                    }

                default:
                    LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(filebytes)} = {filebytes?.Length} байт, {nameof(format)} = {format}) отработал. Получено: {0} байт");

                    return new byte[] { };
            }
        }

        public static PrintableDocument Stick(DocumentPackage pkg, string DisplayName, PackagePrintFormat packagePrintFormat)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(DisplayName)} = {DisplayName}, {nameof(packagePrintFormat)} = {packagePrintFormat})");

            var docList = pkg.Documents;
            var mergedDoc = new AsposeDocument();

            if (docList.Count > 0)
            {
                mergedDoc = new AsposeDocument(new MemoryStream(docList[0].Data));
                docList.RemoveAt(0);

                foreach (var d in docList)
                    mergedDoc.AppendDocument(new AsposeDocument(new MemoryStream(d.Data)), ImportFormatMode.KeepSourceFormatting);
            }

            var ms = new MemoryStream();
            PrintableDocument result;

            if (packagePrintFormat == PackagePrintFormat.pdf)
            {
                mergedDoc.Save(ms, Aspose.Words.SaveFormat.Pdf);
                result = new PrintableDocument { FileName = $"{DisplayName}.pdf", Data = ms.ToArray() };
            }
            else
            {
                mergedDoc.Save(ms, Aspose.Words.SaveFormat.Xps);
                result = new PrintableDocument { FileName = $"{DisplayName}.xps", Data = ms.ToArray() };
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(DisplayName)} = {DisplayName}, {nameof(packagePrintFormat)} = {packagePrintFormat}) отработал. Результат: ({nameof(result.Data)} = {result?.Data?.Length} байт, {nameof(result.FileName)} = {result?.FileName}, {nameof(result.NeedSaveToSharePoint)} = {result?.NeedSaveToSharePoint}, {nameof(result.PrintErrors)} = {result?.PrintErrors?.Count} ошибки, {nameof(result.TemplateId)} = {result?.TemplateId})");

            return result;
        }

        public static string GetFileNameQuery(Report report)
        {
            return report.Sqlqueryfilename;
        }

        public static string ExecSqlQueryOle(DbConnection connection, string query, string fieldName)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(fieldName)} = {fieldName})");

            if (connection == null || string.IsNullOrEmpty(connection.ConnectionString) || string.IsNullOrEmpty(query))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(fieldName)} = {fieldName}) отработал. Получено: пусто");

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
                    throw new ApplicationException($"Запрос должен возвращать только одну запись. Возвращено записей: {table.Rows.Count.ToString()}");

                if (!table.Rows[0].Table.Columns.Contains(fieldName))
                    throw new ApplicationException($"Запись, которую вернул запрос ({query}), не содержит поле \"{fieldName}\"");

                connection.Close();

                var result = table.Rows[0][fieldName].ToString();

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(fieldName)} = {fieldName}) отработал. Получено: {result}");

                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Не удалось выполнить запрос. ({nameof(connection)} = {connection?.ConnectionString}, {nameof(query)} = {query}, {nameof(fieldName)} = {fieldName}), exeption = {ex.ToString()}");
            }
        }

        public static string GetConString(string aServerName, string aDatabaseName)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(aServerName)} = {aServerName}, {nameof(aDatabaseName)} = {aDatabaseName})");

            var sqlBuilder = new SqlConnectionStringBuilder();

            sqlBuilder.DataSource = aServerName;
            sqlBuilder.InitialCatalog = aDatabaseName;
            sqlBuilder.IntegratedSecurity = true;
            sqlBuilder.MultipleActiveResultSets = true;

            var result = sqlBuilder.ToString();

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(aServerName)} = {aServerName}, {nameof(aDatabaseName)} = {aDatabaseName}) отработал. Получено: {result}");

            return result;
        }

        public static string GetFileName(Report report, DbConnection connection, string param, string chooseFormat, bool forPackageMerging = false)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(connection)} = {connection?.ConnectionString}, {nameof(param)} = {param}, {nameof(chooseFormat)} = {chooseFormat}, {nameof(forPackageMerging)} = {forPackageMerging})");

            var query = GetFileNameQuery(report);
            if (!string.IsNullOrEmpty(query))
            {
                query = !string.IsNullOrEmpty(param) ? query.Replace("?", param) : query;

                var fileName = ExecSqlQueryOle(connection, query, "FileName");
                if (!string.IsNullOrEmpty(fileName) && !forPackageMerging)
                {
                    var timeStamp = (bool)report.Setdate ? $"_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}" : string.Empty;

                    var res = $"{Helper.ReplacingIllegalCharacters(fileName)}{timeStamp}{GetFileExtension(report, chooseFormat)}";

                    LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(connection)} = {connection?.ConnectionString}, {nameof(param)} = {param}, {nameof(chooseFormat)} = {chooseFormat}, {nameof(forPackageMerging)} = {forPackageMerging}) отработал. Получено: {res}");

                    return res;
                }
            }
            var result = GetFileNameFromReportName(report, chooseFormat);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(connection)} = {connection?.ConnectionString}, {nameof(param)} = {param}, {nameof(chooseFormat)} = {chooseFormat}, {nameof(forPackageMerging)} = {forPackageMerging}) отработал. Получено: {result}");

            return result;
        }

        public static string GetFileNameFromReportName(Report report, string chooseFormat = null)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(chooseFormat)} = {chooseFormat})");

            var fileName = Helper.ReplacingIllegalCharacters(report.Reportname);

            var timeStamp = (bool)report.Setdate ? $"_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}" : string.Empty;

            var result = $"{fileName}{timeStamp}{GetFileExtension(report, chooseFormat)}";

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(chooseFormat)} = {chooseFormat}) отработал. Получено: {result}");

            return result;
        }

        private static string GetFileExtension(Report report, string chooseFormat)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(chooseFormat)} = {chooseFormat})");

            var docExt = string.Empty;
            var extension = string.Empty;

            if (report.IsChooseFormat && !string.IsNullOrEmpty(chooseFormat))
            {
                if (chooseFormat == "docx" || chooseFormat == "pdf" || chooseFormat == "xps" || chooseFormat == "html")
                    extension = $".{chooseFormat}";
                else
                    throw new ApplicationException($"Для документа выбран неподдерживаемый формат - {chooseFormat}");
            }
            else if (string.IsNullOrEmpty(report.Reportformat))
            {
                docExt = report.IsCustomTemplate ? ".docx" : Path.GetExtension(report.Reportpath).ToLower();

                if (docExt == ".doc" || docExt == ".docx" || docExt == ".docm" || docExt == ".xls" || docExt == ".xlsx")
                    extension = docExt;
                else
                    throw new ApplicationException($"Файл документа имеет неизвестное расширение - {docExt}");
            }
            else
            {
                docExt = report.Reportformat.ToLower();

                if (docExt == "pdf" || docExt == "xps" || docExt == "html")
                    extension = "." + docExt;
                else
                    throw new ApplicationException($"Для документа задан неподдерживаемый формат - {docExt}");
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(chooseFormat)} = {chooseFormat}) отработал. Получено: {extension}");

            return extension;
        }

        public static int GetCountChar(string str, char find)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(str)} = {str}, {nameof(find)} = {find})");

            var count = 0;

            if (string.IsNullOrEmpty(str))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(str)} = {str}, {nameof(find)} = {find}) отработал. Получено: {0}");

                return 0;
            }

            for (var i = 0; i < str.Length; ++i)
            {
                if (str[i] == find)
                    count++;
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(str)} = {str}, {nameof(find)} = {find}) отработал. Получено: {count}");

            return count;
        }

        public static bool SetDataForWordAddIn(ConnectionData connectionData, Report report, string id)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(id)} = {id})");

            if (!IsWordDocument(report))
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(id)} = {id}) отработал.");

                return true;
            }

            SetWordLic();

            var node = CreateNodes(connectionData, report, id);

            SetData(report, node);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(id)} = {id}) отработал.");

            return true;
        }

        private static bool IsWordDocument(Report report)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname})");

            if (report == null)
                throw new ArgumentNullException("report");

            var result = IsWordDocument(report.Reportpath);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}) отработал. Получено: {result}");

            return result;
        }

        private static bool IsWordDocument(string report)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report})");

            if (report == null)
                throw new ArgumentNullException("report");

            var result = report.EndsWith(".doc") || report.EndsWith(".docx");

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report)} = {report}) отработал. Получено: {result}");

            return result;
        }

        private static bool SetData(Report report, Nodee node)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(node)} = {node.Title})");

            var docPath = report.Reportpath;
            var doc = new AsposeDocument(docPath);

            var docProc = new DocProc(new JProc());
            var parts = docProc.GetJson(node);

            if (doc.Variables.Contains(NodeSettings.StoreName))
                doc.Variables.Remove(NodeSettings.StoreName);

            doc.Variables.Add(NodeSettings.StoreName, parts);
            doc.Save(report.Reportpath);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(node)} = {node?.Title}) отработал.");

            return true;
        }

        private static Nodee CreateNodes(ConnectionData connectionData, Report report, string id)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(id)} = {id})");

            var dataset = ExtractDataEngine.GetDataSet(connectionData, report, id);

            var node = new Nodee
            {
                Title = NodeSettings.RootName,
                Type = NodeeType.Reference
            };

            foreach (DataTable table in dataset.Tables)
            {
                var nodee = new Nodee { Title = table.TableName, Type = NodeeType.Reference };

                foreach (DataColumn column in table.Columns)
                {
                    var nn = new Nodee { Title = column.ColumnName, Type = NodeeType.Field };
                    nodee.Nodes.Add(nn);
                }

                node.Nodes.Add(nodee);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report.Reportname}, {nameof(id)} = {id}) отработал. Получено: {node?.Title}");

            return node;
        }

        public static List<byte[]> GetDocumentAsImages(DataModel ctx, ConnectionData connectionData, Report report, string id, int? resolution = null, bool isWaterMarkRequired = false)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(id)} = {id}, {nameof(resolution)} = {resolution})");

            if (string.IsNullOrEmpty(report.Reportpath) || IsWordDocument(report.Reportpath))
            {
                var result = Doc2Image(MergeDocument(ctx, connectionData, report, id, AsposeFormat.Png, isWaterMarkRequired: isWaterMarkRequired).document, resolution);

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(id)} = {id}, {nameof(resolution)} = {resolution}) отработал. Получено: {result?.Count} записей");

                return result;
            }

            if (!IsExcelDocument(report.Reportpath))
                throw new Exception("I don't know that kind of document!");

            var res = Xls2Image(MergeWorkbook(connectionData, report, id));

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(id)} = {id}, {nameof(resolution)} = {resolution}) отработал. Получено: {res?.Count} записей");

            return res;
        }

        private static List<byte[]> Doc2Image(AsposeDocument document, int? resolution)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document}, {nameof(resolution)} = {resolution})");

            if (document == null)
                throw new ArgumentNullException("document");

            SetWordLic();

            var doc = document.Clone();

            var options = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png);
            options.PageCount = 1;
            options.Resolution = 100;
            options.UseHighQualityRendering = true;

            if (resolution != null)
                options.Resolution = resolution.Value;

            var imageDataList = new List<byte[]>();

            for (var pageIndex = 0; pageIndex < doc.PageCount; pageIndex++)
            {
                using (var stream = new MemoryStream())
                {
                    options.PageIndex = pageIndex;
                    doc.Save(stream, options);

                    imageDataList.Add(stream.ToArray());
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(document)} = {document}, {nameof(resolution)} = {resolution}) отработал. Получено: {imageDataList?.Count} записей");

            return imageDataList;
        }

        public static bool IsExcelDocument(string docPath)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(docPath)} = {docPath})");

            if (string.IsNullOrEmpty(docPath))
                throw new ArgumentNullException(docPath);

            var result = docPath.EndsWith(".xls") || docPath.EndsWith(".xlsx");

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(docPath)} = {docPath}) отработал. Получено: {result}");

            return result;
        }

        public static List<byte[]> Xls2Image(Workbook workbook)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(workbook)} = {workbook})");

            if (workbook == null)
                throw new ArgumentNullException("workbook");

            var images = new List<byte[]>();

            for (var i = 0; i < workbook.Worksheets.Count; i++)
            {
                var sheet = workbook.Worksheets[i];

                var imgOptions = new ImageOrPrintOptions
                {
                    ImageFormat = ImageFormat.Tiff,
                    OnePagePerSheet = true,
                };

                var sr = new SheetRender(sheet, imgOptions);

                var bitmap = sr.ToImage(0);

                byte[] byteArray;

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    stream.Close();
                    byteArray = stream.ToArray();
                }

                images.Add(byteArray);
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(workbook)} = {workbook}) отработал. Получено: {images?.Count} записей");

            return images;
        }

        public static Dictionary<string, string> GetIntegrationDataSet(DataModel context, ConnectionData connectionData, Report report, Integration integration, object rowId, Dictionary<string, string> inputParameters)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname})");

            if (integration.Source == null)
            {
                throw new ApplicationException("В отчете не указан источник для интеграции");
            }

            var dataSet = new DataSet();
            var connection = ExtractDataEngine.GetConnection(connectionData.CreateConnection(report.Servername?.Trim(), report.Defaultdatabase));

            connection.Open();

            using (connection)
                ExtractDataEngine.LoadRootDataTable(connection, Helper.SubstituteParams(integration.Source, report, inputParameters), dataSet, rowId);

            var result = new Dictionary<string, string>();

            foreach (DataColumn column in dataSet.Tables[0].Columns)
            {
                result.Add(column.ColumnName, dataSet.Tables[0].Rows[0][column].ToString());
            }

            UnchangedContext(context);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}) отработал. Получено: {result?.Count} записей");

            return result;
        }

        public static (Guid? newParagraphId, Guid? newParagraphContentId) SaveChanges(DataModel context, Guid? paragraphId, Guid? paragraphContentId)
        {
            var entities = context.ChangeTracker.Entries().Where(x => new[] { EntityState.Modified, EntityState.Deleted, EntityState.Added }.Contains(x.State));
            var paragraphs = entities.Where(x => x.Entity is Models.Paragraph).ToList();
            var paragraphContents = entities.Where(x => x.Entity is ParagraphContent).ToList();
            var newParagraphId = paragraphId;
            var newParagraphContentId = paragraphContentId;

            foreach (var contentState in entities.Where(x => x.Entity is DocumentContent && x.State == EntityState.Modified))
            {
                var docContent = contentState.Entity as DocumentContent;

                if (!docContent.Document.TrackChanges)
                    continue;

                var newVersion = new DocumentContent
                {
                    Id = Guid.NewGuid(),
                    Template = docContent.Template,
                    Data = docContent.Data,
                    DefaultVersion = true,
                    Version = docContent.Version + 1,
                    Document = docContent.Document,
                    CreatedBy = WindowsIdentity.GetCurrent().Name,
                    CreatedOn = DateTime.Now
                };

                context.DocumentContents.Add(newVersion);

                docContent.DefaultVersion = false;

                foreach (var paragraph in docContent.Paragraphs)
                {
                    var newParagraph = new Models.Paragraph
                    {
                        Id = Guid.NewGuid(),
                        ActiveFrom = paragraph.ActiveFrom,
                        ActiveTill = paragraph.ActiveTill,
                        Condition = paragraph.Condition,
                        Deleted = paragraph.Deleted,
                        Errors = paragraph.Errors,
                        Level = paragraph.Level,
                        Name = paragraph.Name,
                        NewPage = paragraph.NewPage,
                        Numerable = paragraph.Numerable,
                        OrderNo = paragraph.OrderNo,
                        PassConditions = paragraph.PassConditions,
                        ReferenceName = paragraph.ReferenceName,
                        Tooltip = paragraph.Tooltip,
                        IsFixNumeration = paragraph.IsFixNumeration,
                        ParentParagraph = paragraph.ParentParagraph,
                        Tag = paragraph.Tag
                    };

                    newVersion.Paragraphs.Add(newParagraph);

                    if (paragraph.Id == paragraphId)
                        newParagraphId = newParagraph.Id;

                    foreach (var content in paragraph.ParagraphContents)
                    {
                        var newContent = new ParagraphContent
                        {
                            Id = Guid.NewGuid(),
                            ActiveFrom = content.ActiveFrom,
                            ActiveTill = content.ActiveTill,
                            Condition = content.Condition,
                            Deleted = content.Deleted,
                            Name = content.Name,
                            PassConditions = content.PassConditions,
                            Tooltip = content.Tooltip,
                            Approved = content.Approved,
                            CreatedOn = content.CreatedOn,
                            Content = content.Content,
                            DefaultVersion = content.DefaultVersion,
                            DiffSource = content.DiffSource,
                            DiffTarget = content.DiffTarget
                        };

                        newParagraph.ParagraphContents.Add(newContent);

                        if (content.Id == paragraphContentId)
                            newParagraphContentId = newContent.Id;
                    }
                }

                foreach (var paragraph in docContent.Paragraphs)
                {
                    foreach (var contextParagraph in paragraphs.Where(x => (x.Entity as Models.Paragraph)?.Id == paragraph.Id))
                        RollbackEntry(contextParagraph);

                    foreach (var paragraphContent in paragraph.ParagraphContents)
                        foreach (var contextParagraphContent in paragraphContents.Where(x => (x.Entity as ParagraphContent)?.Id == paragraphContent.Id))
                            RollbackEntry(contextParagraphContent);
                }
            }

            context.SaveChanges();

            return (newParagraphId, newParagraphContentId);
        }

        private static void RollbackEntry(EntityEntry entry) //DbEntityEntry
        {
            switch (entry.State)
            {
                case EntityState.Modified: //System.Data.Entity
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = EntityState.Unchanged;
                    break;

                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }

        public static byte[] CompareDocuments(string name, byte[] firstDocx, byte[] secondDocx, DataSet firstDS = null, DataSet secondDS = null, Report report = null)
        {
            SetWordLic();

            var firstDocument = default(AsposeDocument);
            var secondDocument = default(AsposeDocument);

            using (var ms = new MemoryStream(firstDocx))
                firstDocument = new AsposeDocument(ms);

            using (var ms = new MemoryStream(secondDocx))
                secondDocument = new AsposeDocument(ms);

            if (firstDS != null && report != null)
                firstDocument = BuildDocument(report, firstDS, firstDocument);

            if (secondDS != null && report != null)
                secondDocument = BuildDocument(report, secondDS, secondDocument);

            firstDocument.AcceptAllRevisions();
            secondDocument.AcceptAllRevisions();

            firstDocument.Compare(secondDocument, name, DateTime.Now);

            var result = default(byte[]);

            using (var ms = new MemoryStream())
            {
                firstDocument.Save(ms, Aspose.Words.SaveFormat.Docx);

                result = ms.ToArray();
            }

            return result;
        }

        private static (byte[] docBA, DataTable dt) PrintDSWordDocument(DataModel dbContext, ConnectionData connectionData, Report report, object rowId, DataTable mainTable, Guid auditId)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(auditId)} = {auditId})");

            SetWordLic();

            var (reportCode, mainRowId) = TelerikBuilder.GetMainCodeAndRowId(connectionData, report, rowId);
            var (oldDoc, newDoc, oldDS, newDS) = TelerikBuilder.GetCompareDSDocuments(dbContext, connectionData, reportCode, mainRowId);
            var revDoc = CompareDocuments("Сравнение", oldDoc, newDoc, oldDS, newDS, report);
            TelerikBuilder.FillDocumentChanges(dbContext, connectionData, revDoc, reportCode, Guid.Parse(mainRowId));
            var res = TelerikBuilder.PrintWordDocument(dbContext, report, rowId, mainTable, auditId);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(report.Reportname)} = {report?.Reportname}, {nameof(rowId)} = {rowId}, {nameof(auditId)} = {auditId}) отработал.");

            return res;
        }

        private static void SetWatermarkAsImage(Workbook workbook)
        {
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WordMergeEngine.Assets.Watermark.demo.png");

            byte[] imageArray = null;

            using (MemoryStream ms = new MemoryStream())
            {
                imageStream.CopyTo(ms);
                imageArray = ms.ToArray();
            }

            foreach (var sheet in workbook.Worksheets)
            {
                var watermark = sheet.Shapes.AddRectangle(0, 0, 0, 0, 1000, 1000);
                watermark.Fill.ImageData = imageArray;
                watermark.Fill.Transparency = 0.9;

                var pass = RandomPassword();
                sheet.Protect(Aspose.Cells.ProtectionType.All, pass, pass);
                sheet.Protection.AllowSelectingLockedCell = false;
            }

            workbook.Protect(Aspose.Cells.ProtectionType.All, RandomPassword());
        }

        private static void SetWatermarkAsBackground(Workbook workbook)
        {
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WordMergeEngine.Assets.Watermark.demo.png");

            byte[] imageArray = null;

            using (MemoryStream ms = new MemoryStream())
            {
                imageStream.CopyTo(ms);
                imageArray = ms.ToArray();
            }

            foreach (var sheet in workbook.Worksheets)
            {
                sheet.BackgroundImage = imageArray;

                var pass = RandomPassword();
                sheet.Protect(Aspose.Cells.ProtectionType.All, pass, pass);
            }

            workbook.Protect(Aspose.Cells.ProtectionType.All, RandomPassword());
        }



        private static readonly Random _random = new Random();

        private static string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26;

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        private static string RandomPassword()
        {
            var passwordBuilder = new StringBuilder();

            passwordBuilder.Append(RandomString(4, true));

            passwordBuilder.Append(_random.Next(1000, 9999));

            passwordBuilder.Append(RandomString(2));
            return passwordBuilder.ToString();
        }
    }
}

