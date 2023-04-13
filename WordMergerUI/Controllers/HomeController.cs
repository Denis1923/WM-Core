using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using WordMergeEngine.Assets;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergerUI.Helpers;
using WordMergerUI.Models;

namespace WordMergerUI.Controllers
{
    public class HomeController : Controller
    {
        private string QuiredType
        {
            get
            {
                if (Session["QuiredType"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(QuiredType)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return string.Empty;
                }

                var result = (string)Session["QuiredType"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(QuiredType)} из сессии (id = {Session?.SessionID}) = {result}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(QuiredType)} в сессию (id = {Session?.SessionID}) = {value}");

                Session["QuiredType"] = value;
            }
        }

        private List<Guid> IDs
        {
            get
            {
                if (Session["IDs"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(IDs)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return null;
                }

                var result = (List<Guid>)Session["IDs"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(IDs)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result.Select(x => x.ToString())) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(IDs)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value.Select(x => x.ToString())) : string.Empty)}");

                Session["IDs"] = value;
            }
        }

        private List<string> ExcludeDoc
        {
            get
            {
                if (Session["ExludeDoc"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(ExcludeDoc)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return new List<string>();
                }

                var result = (List<string>)Session["ExludeDoc"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(ExcludeDoc)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(ExcludeDoc)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value) : string.Empty)}");

                Session["ExludeDoc"] = value;
            }
        }

        private List<string> IncludeDoc
        {
            get
            {
                if (Session["IncludeDoc"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(IncludeDoc)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return new List<string>();
                }

                var result = (List<string>)Session["IncludeDoc"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(IncludeDoc)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(IncludeDoc)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value) : string.Empty)}");

                Session["IncludeDoc"] = value;
            }
        }

        private List<string> IncludeRep
        {
            get
            {
                if (Session["IncludeRep"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(IncludeRep)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return new List<string>();
                }

                var result = (List<string>)Session["IncludeRep"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(IncludeRep)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(IncludeRep)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value) : string.Empty)}");

                Session["IncludeRep"] = value;
            }
        }

        private List<string> ExcludeRep
        {
            get
            {
                if (Session["ExcludeRep"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(ExcludeRep)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return new List<string>();
                }

                var result = (List<string>)Session["ExcludeRep"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(ExcludeRep)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(ExcludeRep)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value) : string.Empty)}");

                Session["ExcludeRep"] = value;
            }
        }

        private new List<TempContainer> Items
        {
            get
            {
                if (Session["Items"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(Items)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return new List<TempContainer>();
                }

                var result = (List<TempContainer>)Session["Items"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(Items)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result.Select(x => x.Name)) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(Items)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value.Select(x => x.Name)) : string.Empty)}");

                Session["Items"] = value;
            }
        }

        private new List<string> ParamsName
        {
            get
            {
                if (Session["ParamsName"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(ParamsName)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return null;
                }

                var result = (List<string>)Session["ParamsName"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(ParamsName)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(ParamsName)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value) : string.Empty)}");

                Session["ParamsName"] = value;
            }
        }

        private Guid UserId
        {
            get
            {
                if (Session["UserId"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(UserId)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return Guid.Empty;
                }

                var result = (Guid)Session["UserId"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(UserId)} из сессии (id = {Session?.SessionID}) = {result}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(UserId)} в сессию (id = {Session?.SessionID}) = {value}");

                Session["UserId"] = value;
            }
        }

        private bool AutoUpload
        {
            get
            {
                if (Session["AutoUpload"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(AutoUpload)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return false;
                }

                var result = (bool)Session["AutoUpload"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(AutoUpload)} из сессии (id = {Session?.SessionID}) = {result}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(AutoUpload)} в сессию (id = {Session?.SessionID}) = {value}");

                Session["AutoUpload"] = value;
            }
        }

        private List<MergeService.Filter> Filters
        {
            get
            {
                if (Session["Filters"] == null)
                {
                    LogHelper.GetLogger().Debug($"Получаем значение {nameof(Filters)} из сессии (id = {Session?.SessionID}) = сессия пуста");

                    return new List<MergeService.Filter>();
                }

                var result = (List<MergeService.Filter>)Session["Filters"];

                LogHelper.GetLogger().Debug($"Получаем значение {nameof(Filters)} из сессии (id = {Session?.SessionID}) = {(result != null ? string.Join(",", result.Select(x => x.Name)) : string.Empty)}");

                return result;
            }
            set
            {
                LogHelper.GetLogger().Debug($"Сохраняем значение {nameof(Filters)} в сессию (id = {Session?.SessionID}) = {(value != null ? string.Join(",", value.Select(x => x.Name)) : string.Empty)}");

                Session["Filters"] = value;
            }
        }

        public ActionResult Index()
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var model = new WmDataViewModel();

            try
            {
                Session.Clear();

                model.LstView = new List<TempContainer>();

                ParamsName = new List<string>();

                var reportType = HttpUtility.UrlDecode(Request.Params["reportType"] ?? Request.Params["amp;reportType"]);
                var quiredType = HttpUtility.UrlDecode(Request.Params["quiredType"] ?? Request.Params["amp;quiredType"]);
                var excludeDoc = HttpUtility.UrlDecode(Request.Params["excludeDoc"]);
                var includeDoc = HttpUtility.UrlDecode(Request.Params["includeDoc"]);
                var excludeRep = HttpUtility.UrlDecode(Request.Params["excludeRep"]);
                var includeRep = HttpUtility.UrlDecode(Request.Params["includeRep"]);
                var entityName = Request.Params["entityName"];
                var idsString = Request.Params["ids"];

                model.EnableIgnoreCondition = bool.TryParse(Request.Params["enableIgnoreCondition"], out bool enable) ? enable : false;

                QuiredType = quiredType;
                UserId = Guid.Parse(Request.Params["userId"]);
                AutoUpload = bool.Parse(Request.Params["autoUpload"] ?? "false");

                if (string.IsNullOrEmpty(idsString))
                    throw new Exception("Не удалось получить идентификатор.");

                var ids = new List<Guid>();
                idsString.Split(',').ToList().ForEach(x => ids.Add(Guid.Parse(x)));
                IDs = ids;

                if (!string.IsNullOrEmpty(excludeDoc))
                {
                    var doc = new List<string>();
                    excludeDoc.Split(',').ToList().ForEach(x => doc.Add(x));
                    ExcludeDoc = doc;
                }

                if (!string.IsNullOrEmpty(includeDoc))
                {
                    var doc = new List<string>();
                    includeDoc.Split(',').ToList().ForEach(x => doc.Add(x));
                    IncludeDoc = doc;
                }

                if (!string.IsNullOrEmpty(excludeRep))
                {
                    var rep = new List<string>();
                    excludeRep.Split(',').ToList().ForEach(x => rep.Add(x));
                    ExcludeRep = rep;
                }

                if (!string.IsNullOrEmpty(includeRep))
                {
                    var rep = new List<string>();
                    includeRep.Split(',').ToList().ForEach(x => rep.Add(x));
                    IncludeRep = rep;
                }

                var reportTypeValue = ReportTypes.Unknown;

                if (!string.IsNullOrEmpty(reportType))
                {
                    if (reportType.ToLower() == "document")
                        reportTypeValue = ReportTypes.Documents;
                    else if (reportType.ToLower() == "package")
                        reportTypeValue = ReportTypes.Packages;
                    else if (reportType.ToLower() == "all")
                        reportTypeValue = ReportTypes.All;
                }

                GetWordMergerTemplate(reportTypeValue, entityName);

                model.LstView = Items;
                model.Filters = Filters.ConvertAll(x => new FilterModel
                {
                    Id = x.Id,
                    DisplayName = x.DisplayName,
                    List = x.List,
                    Name = x.Name,
                    ParentFilterId = x.ParentFilterId,
                    Type = x.Type,
                    ParentOperator = x.ParentOperator,
                    ParentValue = x.ParentValue,
                    IsVisible = x.ParentFilterId == null || !x.ParentValue.Any()
                });

                if (model.LstView.Count == 0)
                    ViewBag.msg = "Документы не найдены.";

                if (AutoUpload && model.LstView.Count == 1)
                {
                    LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");

                    return UploadDocument(model, model.LstView.First());
                }

            }
            catch (Exception ex)
            {
                ViewBag.msg = $"При получении документов произошла ошибка:\n{ex.Message}";
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");

            return View(model);
        }

        public ActionResult ApplyFilters(WmDataViewModel wmData)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

                wmData.LstView = Items;
                wmData.Filters = GetFilters();
                var t = Request.Form.AllKeys;

                foreach (var item in wmData.LstView)
                {
                    item.IsVisible = wmData.Filters.All(x => !x.IsUse) ||
                                     item.Report != null &&
                                     item.Report.Filters.Any() &&
                                     item.Report.Filters.All(x => !(wmData.Filters.FirstOrDefault(y => y.Id == x.Id)?.IsUse ?? false) ||
                                                                  (x.Value ?? string.Empty) == (wmData.Filters.FirstOrDefault(y => y.Id == x.Id)?.Value ?? string.Empty));
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = $"При применении фильтров произошла ошибка:\n{ex.Message}";
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал.");

            return View("Index", wmData);
        }

        private List<FilterModel> GetFilters()
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

            var res = new List<FilterModel>();

            foreach (var filterGroup in Request.Form.AllKeys.Where(y => y != "ID" && y.StartsWith("filter_")).GroupBy(x => x.Split('_')[1], y => y))
            {
                var filter = Filters.FirstOrDefault(x => x.Id.ToString() == filterGroup.Key);
                var item = new FilterModel
                {
                    Id = filter.Id,
                    DisplayName = filter.DisplayName,
                    List = filter.List,
                    Name = filter.Name,
                    ParentFilterId = filter.ParentFilterId,
                    Type = filter.Type,
                    ParentOperator = filter.ParentOperator,
                    ParentValue = filter.ParentValue,
                    IsVisible = filter.ParentFilterId == null || !filter.ParentValue.Any()
                };

                foreach (var field in filterGroup)
                {
                    switch (field.Split('_')[2].ToLower())
                    {
                        case "isuse":
                            item.IsUse = bool.Parse(Request.Params[field].Split(',')[0]);
                            break;

                        case "value":
                            item.Value = item.Type == "Bool" ? Request.Params[field].Split(',')[0] : Request.Params[field];
                            break;
                    }
                }

                res.Add(item);
            }

            foreach (var item in res)
            {
                if (item.ParentFilterId != null && item.ParentValue.Any())
                {
                    item.IsVisible = item.ParentOperator == "<>" ^ item.ParentValue.Contains(res.FirstOrDefault(x => x.Id == item.ParentFilterId)?.Value);
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал. Получено {res?.Count} записей");

            return res;
        }

        public ActionResult CreateDocument(WmDataViewModel wmData)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

                wmData.LstView = Items;

                var selected = Items[int.Parse(wmData.ID)];

                MergedDocument doc = null;

                if (!IDs.Any())
                    throw new Exception("Не удалось получить ни одного идентификатора.");

                var wWork = new WordMergerHelper(QuiredType);

                if (selected.Report != null)
                {
                    if (AutoUpload)
                        return UploadDocument(wmData, selected);

                    if (IDs.Count > 1)
                    {
                        var result = wWork.GetSuiteErrors(false, selected.Report.ReportCode, IDs.ToArray(), UserId);

                        if (result.Errors.Length > 0)
                        {
                            ViewBag.msg = result.Errors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            return View("Index", wmData);
                        }

                        if (!wmData.IsIgnoreCondition && result.ConditionErrors.Length > 0)
                        {
                            ViewBag.msg = result.ConditionErrors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            ViewBag.isConditionError = "True";
                            return View("Index", wmData);
                        }

                        doc = OpenDocument(wWork, false, selected, wmData.SaveFormat, wmData.IsIgnoreCondition);
                    }
                    else
                    {
                        var result = wWork.GetErrors(false, selected.Report.ReportCode, IDs.First(), UserId);

                        if (result.Errors.Length > 0)
                        {
                            ViewBag.msg = result.Errors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            return View("Index", wmData);
                        }

                        if (!wmData.IsIgnoreCondition && result.ConditionErrors.Length > 0)
                        {
                            ViewBag.msg = result.ConditionErrors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            ViewBag.isConditionError = "True";
                            return View("Index", wmData);
                        }

                        doc = OpenDocument(wWork, false, selected, wmData.SaveFormat, wmData.IsIgnoreCondition);
                    }
                }

                if (selected.Package != null)
                {
                    if (IDs.Count > 1)
                    {
                        var result = wWork.GetSuiteErrors(true, selected.Package.PackageCode, IDs.ToArray(), UserId);

                        if (result.Errors.Length > 0)
                        {
                            ViewBag.msg = result.Errors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            return View("Index", wmData);
                        }

                        if (!wmData.IsIgnoreCondition && result.ConditionErrors.Length > 0)
                        {
                            ViewBag.msg = result.ConditionErrors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            ViewBag.isConditionError = "True";
                            return View("Index", wmData);
                        }

                        doc = OpenDocument(wWork, true, selected, wmData.SaveFormat, wmData.IsIgnoreCondition);
                    }
                    else
                    {
                        var result = wWork.GetErrors(true, selected.Package.PackageCode, IDs.First(), UserId);

                        if (result.Errors.Length > 0)
                        {
                            ViewBag.msg = result.Errors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            return View("Index", wmData);
                        }

                        if (!wmData.IsIgnoreCondition && result.ConditionErrors.Length > 0)
                        {
                            ViewBag.msg = result.ConditionErrors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n"));
                            ViewBag.isConditionError = "True";
                            return View("Index", wmData);
                        }

                        doc = OpenDocument(wWork, true, selected, wmData.SaveFormat, wmData.IsIgnoreCondition);
                    }
                }

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} () отработал. Получено {doc?.Data?.Length} байт, {doc?.DocumentName}");

                return File(doc.Data, MediaTypeNames.Application.Octet, doc.DocumentName);
            }
            catch (ConditionException ex)
            {
                ViewBag.msg = ex.Message;
                ViewBag.isConditionError = "True";
                return View("Index", wmData);
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View("Index", wmData);
            }
        }

        public ActionResult IsParameterItem(string lstView)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(lstView)} = {lstView})");

            var selected = Items.ElementAtOrDefault(int.Parse(lstView));

            if (selected == null)
            {
                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(lstView)} = {lstView}) отработал. Запись не найдена");

                throw new ApplicationException("Запись не найдена");
            }

            if (selected.Report != null)
            {
                var reportParameters = new WordMergerHelper(QuiredType).GetReportParameters(selected.Report.ReportCode, IDs.First(), UserId);

                if (reportParameters.Any())
                {
                    LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(lstView)} = {lstView}) отработал. Получено {reportParameters?.Length} записей");

                    return PartialView("Params", new WmDataViewModel() { ReportParams = reportParameters });
                }
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(lstView)} = {lstView}) отработал. Получено {0} записей");

            return PartialView("Params", null);
        }

        public bool Equals(string str, IEnumerable<string> toCompare)
        {
            return toCompare.Any(s => str.Equals(s, StringComparison.InvariantCultureIgnoreCase));
        }

        public ActionResult PreviewDocument(WmDataViewModel wmData, string lstView)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(lstView)} = {lstView})");

                wmData.LstView = Items;

                var selected = Items[int.Parse(lstView)];

                if (selected.Report != null)
                {
                    var guidList = new List<Guid>() { IDs.First() };

                    var paramValues = new List<LabelValueEntry>() { new LabelValueEntry { Label = "userid", Value = UserId.ToString() } };

                    if (Request.QueryString.AllKeys.Count() > 1)
                    {
                        foreach (var paramName in Request.QueryString.AllKeys)
                        {
                            if (paramName != "lstView")
                                paramValues.Add(new LabelValueEntry { Label = paramName, Value = Request.Params[paramName] });
                        }
                    }

                    var mergedDocument = new WordMergerHelper(QuiredType).GetDocumentForPreview(selected.Report.ReportCode, guidList, paramValues);

                    if (mergedDocument == null)
                        throw new Exception("Произошла ошибка при формировании документа для предварительного просмотра");

                    if (mergedDocument.Errors != null && mergedDocument.Errors.Length > 0)
                        throw new Exception(mergedDocument.Errors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n")));

                    ViewBag.ImageData = mergedDocument.Data;
                }

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(lstView)} = {lstView}) отработал.");

                return PartialView("_Preview");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
                return PartialView("_Preview", null);
            }
        }

        private void GetWordMergerTemplate(ReportTypes reportType, string entityName)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportType)} = {reportType}, {nameof(entityName)} = {entityName})");

            if (reportType == ReportTypes.Unknown)
            {
                ViewBag.msg = "Не задан тип загружаемых документов.";
                return;
            }

            var reports = new List<Report>();
            var packages = new List<ReportPackage>();
            var wWork = new WordMergerHelper(QuiredType);

            if (reportType == ReportTypes.All || reportType == ReportTypes.Documents)
            {
                var availableReport = wWork.GetReports(entityName, IDs.ToArray(), UserId);
                reports = availableReport.reports.ToList();
                ViewBag.msg += availableReport.Message;
            }

            if (reportType == ReportTypes.All || reportType == ReportTypes.Packages)
                packages = wWork.GetPackages(entityName);

            var filters = reports.SelectMany(x => x.Filters).GroupBy(x => x.Id).Select(f => f.First());

            foreach (var item in filters)
            {
                var parentFilter = filters.FirstOrDefault(x => x.Id == item.ParentFilterId);

                if (parentFilter != null)
                    item.Order = parentFilter.Order + item.Order / 10;
            }

            Filters = filters.OrderBy(x => x.Order).ToList();

            UpdateListBox(reports, packages);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reportType)} = {reportType}, {nameof(entityName)} = {entityName}) отработал.");
        }

        private void UpdateListBox(List<Report> reports, List<ReportPackage> packages)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(reports)} = {reports?.Count} записей, {nameof(packages)} = {packages?.Count} записей)");

                var items = new List<TempContainer>();

                if (ExcludeDoc.Any())
                {
                    var doc = reports.Where(x => !Equals(x.ReportCode, ExcludeDoc) && x.ReportType == "doc").ToList();
                    reports = reports.Where(x => x.ReportType != "doc").ToList();
                    reports.AddRange(doc);
                }

                if (IncludeDoc.Any())
                {
                    var doc = reports.Where(x => Equals(x.ReportCode, IncludeDoc) && x.ReportType == "doc").ToList();
                    reports = reports.Where(x => x.ReportType != "doc").ToList();
                    reports.AddRange(doc);
                }

                if (ExcludeRep.Any())
                {
                    var rep = reports.Where(x => !Equals(x.ReportCode, ExcludeRep) && x.ReportType == "rep").ToList();
                    reports = reports.Where(x => x.ReportType != "rep").ToList();
                    reports.AddRange(rep);
                }

                if (IncludeRep.Any())
                {
                    var rep = reports.Where(x => Equals(x.ReportCode, IncludeRep) && x.ReportType == "rep").ToList();
                    reports = reports.Where(x => x.ReportType != "rep").ToList();
                    reports.AddRange(rep);
                }

                if (reports != null)
                {
                    foreach (var x in reports)
                        items.Add(new TempContainer { Name = x.ReportName, Report = x, Package = null, IsChooseFormat = x.IsChooseFormat });
                }

                if (packages != null)
                {
                    foreach (var x in packages)
                        items.Add(new TempContainer { Name = x.PackageName, Package = x, Report = null });
                }

                Items = items.OrderBy(i => i.Name).ToList();

                foreach (var item in Items)
                    item.ID = Items.IndexOf(item).ToString();

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reports)} = {reports?.Count} записей, {nameof(packages)} = {packages?.Count} записей) отработал.");
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(reports)} = {reports?.Count} записей, {nameof(packages)} = {packages?.Count} записей) отработал с ошибкой: {ex.Message}");
            }
        }

        private MergedDocument OpenDocument(WordMergerHelper wWork, bool isPackage, TempContainer selected, string chooseFormat, bool isIgnoreCondition)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(isPackage)} = {isPackage}, {nameof(selected)} = {selected?.Name}, {nameof(chooseFormat)} = {chooseFormat}, {nameof(isIgnoreCondition)} = {isIgnoreCondition})");

            MergedDocument doc = null;

            var paramValues = new List<LabelValueEntry>() { new LabelValueEntry { Label = "userid", Value = UserId.ToString() } };

            if (isPackage)
                doc = wWork.OpenDocumentPackage(selected.Package.PackageCode, IDs, paramValues);
            else
            {
                if (Request.Form.AllKeys.Count() > 1)
                {
                    foreach (var paramName in Request.Form.AllKeys)
                    {
                        if (paramName != "ID" && paramName != "SaveFormat" && !paramName.StartsWith("filter_"))
                            paramValues.Add(new LabelValueEntry { Label = paramName, Value = Request.Params[paramName] });
                    }

                    doc = wWork.MergeParametrizedDocument(selected.Report.ReportCode, IDs.First(), paramValues, chooseFormat, isIgnoreCondition);

                }
                else
                    doc = wWork.MergeParametrizedDocument(selected.Report.ReportCode, IDs.First(), paramValues, chooseFormat, isIgnoreCondition);
            }

            if (doc.IsConditionError && doc.Errors.Any())
                throw new ConditionException(doc.Errors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n")));

            if (doc.Errors.Any())
                throw new Exception(doc.Errors.Aggregate(string.Empty, (current, err) => "" + current + (err + "\n")));

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(isPackage)} = {isPackage}, {nameof(selected)} = {selected?.Name}, {nameof(chooseFormat)} = {chooseFormat}, {nameof(isIgnoreCondition)} = {isIgnoreCondition}) отработал.");

            return doc;
        }

        private ActionResult UploadDocument(WmDataViewModel model, TempContainer item)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(item)} = {item?.Name})");

                var paramValues = new List<LabelValueEntry>() { new LabelValueEntry { Label = "userid", Value = UserId.ToString() } };

                var result = new WordMergerHelper(QuiredType).MergeParametrizedDocumentWithUpload(item.Report.ReportCode, IDs.First(), paramValues, UserId.ToString(), model.SaveFormat, model.IsIgnoreCondition);

                if (result.IsConditionError && result.Errors.Any())
                    throw new ConditionException(string.Join(Environment.NewLine, result.Errors));

                if (result.Errors.Any())
                    throw new Exception(string.Join(Environment.NewLine, result.Errors));

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(item)} = {item?.Name}) отработал.");

                return File(result.Data, MediaTypeNames.Application.Octet, result.DocumentName);
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;

                return View("Index", model);
            }
        }
    }
}