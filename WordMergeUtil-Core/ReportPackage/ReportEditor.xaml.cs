using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WordMergeEngine;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;
using Condition = WordMergeEngine.Models.Condition;
using EngineHelper = WordMergeEngine.Helpers.Helper;
using RReport = WordMergeEngine.Models.Report;
using Enums = WordMergeEngine.Assets.Enums;
using System.Data;
using WordMergeEngine.Models.Helpers;
using System.Net.Sockets;
using WordMergeUtil_Core.AgreementTool;

namespace WordMergeUtil_Core
{

    public enum TemplateFormat
    {
        Word,
        Excel,
        Undefined
    };

    public enum EditPageType
    {
        Report,
        DataSource,
        Condition,
        Parameter,
        Filter
    }

    public partial class EditReport : Page
    {
        public RReport CurrentReport { get; set; }

        public DataSource CurrentDataSource { get; set; }

        public Condition CurrentCondition { get; set; }

        public Parameter CurrentParameter { get; set; }

        public Filter CurrentFilter { get; set; }

        public EditPageType CurrentType { get; set; }

        public List<BusinessRole> Roles { get; set; }

        public string[] TempReportquery { get; set; }

        public bool ReportIsChanged { get; set; }

        public bool TextSettingModeOn { get; set; }

        public bool IsGlobal { get; set; }

        public TemplateFormat TemplateFormat
        {
            get
            {
                if (CurrentReport.Reportpath != null)
                {
                    if (CurrentReport.Reportpath.EndsWith("doc") || CurrentReport.Reportpath.EndsWith("docx") || CurrentReport.Reportpath.EndsWith("docm"))
                        return TemplateFormat.Word;
                    else if (CurrentReport.Reportpath.EndsWith("xls") || CurrentReport.Reportpath.EndsWith("xlsx"))
                        return TemplateFormat.Excel;
                }

                return TemplateFormat.Undefined;
            }
        }

        public EditReport(DataSource dataSource)
        {
            CurrentType = EditPageType.DataSource;
            IsGlobal = true;

            InitializeComponent();

            CurrentReport = new RReport();
            tabControl1.SelectedIndex = 1;

            if (dataSource != null)
                ExtractDataEngine.LoadNestedDataSources(dataSource);

            foreach (TabItem item in tabControl1.Items)
            {
                if (item.Name == "tabItem3")
                    item.Header = "Данные источника";
                else
                    item.Visibility = Visibility.Collapsed;
            }

            IsInregrationRequestLabel.Visibility = Visibility.Visible;
            IsIntegrationRequestCheckBox.Visibility = Visibility.Visible;

            RunQuery.Visibility = Visibility.Collapsed;
            DoDesign.Visibility = Visibility.Collapsed;
            DoTest.Visibility = Visibility.Collapsed;
            RunConditionsCheck.Visibility = Visibility.Collapsed;
            RunQuerySeparator.Visibility = Visibility.Collapsed;
            ResultGridSplitter.Visibility = Visibility.Collapsed;
            ResultGrid.Visibility = Visibility.Collapsed;
            GridRowDef3.Height = GridLength.Auto;
            CaptionLabel.Content = "Редактирование источника данных";

            CusomizeSQLEditor(SQLDSQueryEditor);

            var dataSourceItems = new List<DataSource>() { dataSource };
            SortDataSourceByPosition(dataSourceItems.FirstOrDefault());
            treeView1.ItemsSource = dataSourceItems;
            dataSource.InverseParentDataSource.CollectionChanged += OnCollectionAssociationChanged;

            var connectionData = ((App)Application.Current).GetConnection;
            Application.Current.MainWindow.Title = $"[{connectionData.ServerName}.{connectionData.DatabaseName}] {Title}";

            CurrentDataSource = dataSource;
            LogHelper.Log(GetType().Name, CurrentType.ToString());
        }

        public EditReport(Condition condition)
        {
            CurrentType = EditPageType.Condition;
            IsGlobal = true;

            InitializeComponent();

            CurrentReport = new RReport();
            tabControl1.SelectedIndex = 2;

            foreach (TabItem item in tabControl1.Items)
            {
                if (item.Name == "tabItem2")
                    item.Header = "Данные проверки";
                else
                    item.Visibility = Visibility.Collapsed;
            }

            RunQuery.Visibility = Visibility.Collapsed;
            DoDesign.Visibility = Visibility.Collapsed;
            DoTest.Visibility = Visibility.Collapsed;
            RunConditionsCheck.Visibility = Visibility.Collapsed;
            RunQuerySeparator.Visibility = Visibility.Collapsed;
            toolBar2.Visibility = Visibility.Collapsed;
            condtionListBox.Visibility = Visibility.Collapsed;
            ConditionLeftColumn.Width = GridLength.Auto;
            CaptionLabel.Content = "Редактирование проверки отчета";
            CusomizeSQLEditor(SQLConditionQueryEditor);
            CusomizeSQLEditor(SQLConditionQueryEditor2);

            SetTitle();

            CurrentCondition = condition;

            var conditions = new List<Condition> { condition };
            condtionListBox.ItemsSource = conditions;
            condtionListBox.DisplayMemberPath = "conditionname";
            condtionListBox.SelectedItem = condition;

            ConditionSelect(CurrentCondition);
            LogHelper.Log(GetType().Name, CurrentType.ToString());
        }

        public EditReport(Parameter parameter)
        {
            CurrentType = EditPageType.Parameter;
            IsGlobal = true;

            InitializeComponent();

            CurrentReport = new RReport();
            tabControl1.SelectedIndex = 5;

            foreach (TabItem item in tabControl1.Items)
            {
                if (item.Name == "tabRepParams")
                    item.Header = "Данные параметра";
                else
                    item.Visibility = Visibility.Collapsed;
            }

            RunQuery.Visibility = Visibility.Collapsed;
            DoDesign.Visibility = Visibility.Collapsed;
            DoTest.Visibility = Visibility.Collapsed;
            RunConditionsCheck.Visibility = Visibility.Collapsed;
            RunQuerySeparator.Visibility = Visibility.Collapsed;
            toolBarRepParams.Visibility = Visibility.Collapsed;
            RepParamListBox.Visibility = Visibility.Collapsed;
            ParamLeftColumn.Width = GridLength.Auto;
            CaptionLabel.Content = "Редактирование параметра";
            CusomizeSQLEditor(SQLRepParamQueryEditor);

            SetTitle();

            CurrentParameter = parameter;

            var parameters = new List<Parameter> { parameter };
            RepParamListBox.ItemsSource = parameters;
            RepParamListBox.DisplayMemberPath = "name";
            RepParamListBox.SelectedItem = parameter;

            ParameterSelect(CurrentParameter);
            LogHelper.Log(GetType().Name, CurrentType.ToString());
        }

        public EditReport(Filter filter)
        {
            CurrentType = EditPageType.Filter;
            IsGlobal = true;

            InitializeComponent();

            CurrentReport = new RReport();
            tabControl1.SelectedIndex = 6;

            foreach (TabItem item in tabControl1.Items)
            {
                if (item.Name == "tabFiltration")
                    item.Header = "Данные фильтра";
                else
                    item.Visibility = Visibility.Collapsed;
            }

            RunQuery.Visibility = Visibility.Collapsed;
            DoDesign.Visibility = Visibility.Collapsed;
            DoTest.Visibility = Visibility.Collapsed;
            RunConditionsCheck.Visibility = Visibility.Collapsed;
            GlobalFilter.Visibility = Visibility.Collapsed;
            PinFilter.Visibility = Visibility.Collapsed;
            UnpinFilter.Visibility = Visibility.Collapsed;
            FilterListBox.Visibility = Visibility.Collapsed;
            FilterLeftColumn.Width = GridLength.Auto;
            CaptionLabel.Content = "Редактирование фильтра";
            CusomizeSQLEditor(SQLFilterQueryEditor);

            SetTitle();

            CurrentFilter = filter;

            var filters = new List<Filter> { filter };
            FilterListBox.ItemsSource = filters;
            FilterListBox.DisplayMemberPath = "Name";
            FilterListBox.SelectedItem = filter;

            VisibleFilterField(true);

            FilterSelect(CurrentFilter);
            LogHelper.Log(GetType().Name, CurrentType.ToString());
        }

        public EditReport(RReport report)
        {
            CurrentType = EditPageType.Report;
            IsGlobal = false;
            CurrentReport = report;

            DataContext = CurrentReport;
            ExtractDataEngine.Loader(CurrentReport);

            InitializeComponent();

            CusomizeSQLEditor(SQLDSQueryEditor);
            CusomizeSQLEditor(SQLConditionQueryEditor);
            CusomizeSQLEditor(SQLConditionQueryEditor2);
            CusomizeSQLEditor(SQLRepParamQueryEditor);
            CusomizeSQLEditor(SQLQueryFileName);
            CusomizeSQLEditor(SQLFilterQueryEditor);

            CusomizeSQLEditor(SqlMainReportCode);
            CusomizeSQLEditor(SqlMainRowId);

            CusomizeSQLEditor(SqlQueryUseCondition);

            var dataSourceItems = new List<DataSource>();
            var currentDataSource = CurrentReport.Datasource;

            if (currentDataSource == null)
            {
                CurrentReport.Datasource = new DataSource()
                {
                    Datasourceid = Guid.NewGuid(),
                    Datasourcename = "Новый источник данных",
                    Position = 0
                };
                currentDataSource = CurrentReport.Datasource;
                ReportIsChanged = true;
            }

            dataSourceItems.Add(currentDataSource);
            SortDataSourceByPosition(dataSourceItems[0]);
            treeView1.ItemsSource = dataSourceItems;
            currentDataSource.InverseParentDataSource.CollectionChanged += OnCollectionAssociationChanged;

            SetTitle();

            ReloadRoles();
            ReloadExistingBusinessRoles();
            ReloadConditions();
            ReloadRepParams();
            ReloadReportFilters();
            GetGlobals();

            if (CurrentReport.Useglobal == true)
            {
                var context = ((App)Application.Current).GetDBContext;
                var globalDataSource = context.DataSources.Where(x => x.Isglobal == true && x.Datasourceid.Equals(currentDataSource.Datasourceid)).FirstOrDefault();

                if (globalDataSource != null)
                {
                    GlobalDataSources.SelectedItem = new KeyValuePair<Guid, string>(globalDataSource.Datasourceid, globalDataSource.Datasourcename);
                    ExtractDataEngine.LoadNestedDataSources(globalDataSource);
                }
            }

            useGlobal.IsChecked = CurrentReport.Useglobal == true;
            CheckVisibility();
            LogHelper.Log(GetType().Name, $"Тип: {CurrentType}, название: {report?.Reportname}");

            ChangeLockReport(true);
            LockingForm();
            VisibleFilterField(false);
        }

        private void OnReportPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "defaultdatabase" && e.PropertyName != "dataquery" && e.PropertyName != "query")
                ReportIsChanged = true;
        }

        void OnCollectionAssociationChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ReportIsChanged = true;
        }

        private void CusomizeSQLEditor(TextEditor editor)
        {
            editor.Options.EnableEmailHyperlinks = true;
            editor.Options.ConvertTabsToSpaces = true;
            editor.ShowLineNumbers = false;

            var scheme = new MemoryStream((byte[])Properties.Resources.ResourceManager.GetObject("tsql"));

            using (XmlTextReader reader = new XmlTextReader(scheme))
                editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

            editor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(editor.TextArea));
        }

        private void ReloadRoles()
        {
            var context = ((App)Application.Current).GetDBContext;
            Roles = (from p in context.BusinessRoles orderby p.Rolename select p).ToList();
            AvalableRolesListBox.ItemsSource = Roles;
            AvalableRolesListBox.DisplayMemberPath = "rolename";
        }

        private void ReloadConditions(ReportCondition rc = null)
        {
            var context = ((App)Application.Current).GetDBContext;
            context.Entry(CurrentReport).Reload();

            foreach (var reportCondition in CurrentReport.ReportConditions)
            {
                if (context.Entry(reportCondition).State != EntityState.Added && context.Entry(reportCondition).State != EntityState.Deleted)
                    context.Entry(reportCondition).Reload();
            }

            var conditions = CurrentReport.ReportConditions.Select(x => x.Condition).Where(c => c.Conditiontype == null && c.ReportPackageEntry == null).ToList();
            condtionListBox.ItemsSource = conditions;
            condtionListBox.DisplayMemberPath = "conditionname";
            condtionListBox2.ItemsSource = CurrentReport.ReportConditions.Select(x => x.Condition).Where(c => c.Conditiontype == 1);
            condtionListBox2.DisplayMemberPath = "conditionname";

            CurrentReport.ReportConditions.CollectionChanged += OnCollectionAssociationChanged;
        }

        private void ReloadRepParams(ReportParameter parameter = null)
        {
            RepParamListBox.ItemsSource = CurrentReport.ReportParameters.Select(x => x.Parameter).ToList();
            RepParamListBox.DisplayMemberPath = "name";

            RepParamsGrid.DataContext = null;

            if (parameter != null)
                RepParamListBox.SelectedItem = parameter.Parameter;

            CurrentReport.ReportParameters.CollectionChanged += OnCollectionAssociationChanged;
        }

        private string SubstituteParams(string query)
        {
            var newquery = query;
            var report = CurrentReport;

            var parameters = CurrentReport.ReportParameters.Select(x => x.Parameter);

            foreach (var parameter in parameters)
            {
                if (string.IsNullOrEmpty(parameter.testval))
                    throw new ApplicationException($"Ошибка при формировании печатной формы.\nВ параметре \"{parameter.name}\" отсутствует тестовое значение.");
            }

            newquery = EngineHelper.SubstituteParams(newquery, report, parameters.ToDictionary(x => x.name, y => y.testval));

            return newquery;
        }

        public void SetGetTempQuery(bool IsSet)
        {
            if (IsSet)
            {
                TempReportquery = new string[CurrentReport.Datasource.InverseParentDataSource.Count + 1 + CurrentReport.ReportConditions.Count];

                var i = 1;

                TempReportquery[0] = CurrentReport.Datasource.Dataquery;

                foreach (var ds in CurrentReport.Datasource.InverseParentDataSource)
                {
                    TempReportquery[i] = ds.Dataquery;
                    i++;
                }

                foreach (var con in CurrentReport.ReportConditions)
                {
                    TempReportquery[i] = con.Condition.Dataquery;
                    i++;
                }
            }
            else
            {
                var i = 1;

                CurrentReport.Datasource.Dataquery = TempReportquery[0];

                foreach (var ds in CurrentReport.Datasource.InverseParentDataSource)
                {
                    ds.Dataquery = TempReportquery[i];
                    i++;
                }

                foreach (var con in CurrentReport.ReportConditions)
                {
                    con.Condition.dataquery = TempReportquery[i];
                    i++;
                }
            }
        }

        private RReport FillParams()
        {
            var report = new RReport();

            var connectionData = ((App)Application.Current).GetConnection;
            var context = ((App)Application.Current).GetOtherTempDBContext(connectionData.ServerName, connectionData.DatabaseName, connectionData.UserName, connectionData.Password);

            if ((CurrentReport.Reporttype.ToLower() == "rep" || CurrentReport.Reporttype.ToLower() == "doc") && CurrentReport.ReportParameters.Count > 0)
            {
                report = (from p in context.Reports where p.Reportcode == CurrentReport.Reportcode orderby p.Reportname select p).FirstOrDefault();

                ExtractDataEngine.Loader(report);

                report.Sqlqueryfilename = SubstituteParams(report.Sqlqueryfilename);
                report.Datasource.Dataquery = SubstituteParams(report.Datasource.Dataquery);
                report.SqlQueryUseCondition = SubstituteParams(report.SqlQueryUseCondition);

                foreach (var datasource in report.Datasource.InverseParentDataSource)
                    datasource.Dataquery = SubstituteParams(datasource.Dataquery);

                foreach (var condition in report.ReportConditions)
                    condition.Condition.ChangedDataQuery = SubstituteParams(condition.Condition.DataQuery);

                SaveData();
            }
            else
            {
                report = (from p in context.Reports where p.Reportcode == CurrentReport.Reportcode orderby p.Reportname select p).ToArray().FirstOrDefault();
                ExtractDataEngine.Loader(report);
            }

            if (!string.IsNullOrEmpty(report.Testuserid))
            {
                report.Datasource.Dataquery = report.Datasource.Dataquery.Replace("@userid", $"'{report.Testuserid}'");

                replaceUserID(report.Datasource, report.Testuserid);

                foreach (var reportCondition in report.ReportConditions)
                    reportCondition.Condition.ChangedDataQuery = reportCondition.Condition.DataQuery.Replace("@userid", $"'{report.Testuserid}'");
            }

            if (!string.IsNullOrEmpty(report.Reportcode))
            {
                report.Datasource.Dataquery = report.Datasource.Dataquery.Replace("@reportcode", $"'{report.Reportcode}'");

                replaceReportCode(report.Datasource, report.Reportcode);

                foreach (var reportCondition in report.ReportConditions)
                    reportCondition.Condition.ChangedDataQuery = reportCondition.Condition.DataQuery.Replace("@reportcode", $"'{report.Reportcode}'");
            }

            return report;
        }

        private void UnFillParams(Stack<string> oldValues)
        {
            var report = CurrentReport;

            if (report.ReportParameters.Count > 0)
            {
                foreach (var reportCondition in report.ReportConditions)
                    reportCondition.Condition.Dataquery = oldValues.Pop();

                foreach (var datasource in report.Datasource.InverseParentDataSource)
                    datasource.Dataquery = oldValues.Pop();

                report.Datasource.Dataquery = oldValues.Pop();
                SaveData();
            }
        }

        private void DoTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentReport.Testid))
                {
                    if (MessageBox.Show("Отсутствует тестовый идентификатор. Продолжить?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                }

                SaveData();
                RunTest();

            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RunTest(bool RemoveButtons = false)
        {
            try
            {
                var connection = ((App)Application.Current).GetConnection.CreateConnection(ServerTextBox.Text, BaseTextComboBox.SelectedValue == null ? null : BaseTextComboBox.SelectedValue.ToString());

                if (string.IsNullOrEmpty(connection.ServerName))
                {
                    MessageBox.Show(string.Format("Не заполнен адрес сервера БД"));
                    return;
                }

                if (BaseTextComboBox.SelectedValue == null || string.IsNullOrEmpty(connection.DatabaseName))
                {
                    MessageBox.Show(string.Format("Не заполнено имя БД"));
                    return;
                }

                SetGetTempQuery(true);

                var context = ((App)Application.Current).GetDBContext;
                var result = DocBuilder.PrintDocument(context, ((App)Application.Current).GetConnection, FillParams(), CurrentReport.testid == null ? string.Empty : CurrentReport.testid, GetParams(), isWaterMarkRequired: false).document;

                if (result.PrintErrors != null && result.PrintErrors.Count > 0)
                {
                    var errors = new StringBuilder();

                    foreach (var e in result.PrintErrors)
                        errors.AppendLine(e);

                    MessageBox.Show($"По тествому идентификатору присутствуют следующие замечания:\n{errors}\n(Решение: изменить тестовый идентификатор.)");

                    return;
                }

                var fileName = $"{Path.GetTempPath()}{Path.GetFileNameWithoutExtension(result.FileName)}{Path.GetExtension(result.FileName)}";

                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (result.Data != null)
                    {
                        fs.Write(result.Data, 0, result.Data.Length);
                        fs.Close();
                    }
                }

                Process.Start(fileName);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private Dictionary<string, string> GetParams()
        {
            var parameters = CurrentReport.ReportParameters.Select(x => x.Parameter);

            return parameters.ToDictionary(x => x.name, y => y.testval);
        }

        private void DoStore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveData();

                if (Settings.Default.ShowSaveSuccessMessage)
                    MessageBox.Show("Настройки успешно сохранены", "Сохранение данных", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void SaveData()
        {
            if (!IsGlobal)
            {
                CurrentReport.Reporttype = reporttype.SelectedValue == null ? string.Empty : reporttype.SelectedValue.ToString();
                CurrentReport.Sqlqueryfilename = SQLQueryFileName.Text;
                CurrentReport.Modifiedon = DateTime.Now;
                CurrentReport.Modifiedby = WindowsIdentity.GetCurrent().Name;
                CurrentReport.Reportcode = ReportCodeTextBox.Text;
                CurrentReport.SqlQueryUseCondition = SqlQueryUseCondition.Text;

                if (reportformat.SelectedIndex == 0)
                    CurrentReport.Reportformat = null;
                else
                    CurrentReport.Reportformat = reportformat.SelectedValue.ToString();

                CurrentReport.SpLoadType = SpLoadTypeComboBox.SelectedValue?.ToString();
                CurrentReport.NamePostfix = ChangeNameTypeComboBox.SelectedValue?.ToString();

                CurrentReport.SqlMainReportCode = SqlMainReportCode.Text;
                CurrentReport.SqlMainRowId = SqlMainRowId.Text;

                if (CurrentReport.Sequencenumber == null)
                    CurrentReport.Sequencenumber = 0;
            }

            if (treeView1.SelectedItem != null)
            {
                var dataSource = treeView1.SelectedItem as DataSource;
                dataSource.Dataquery = SQLDSQueryEditor.Text;
                dataSource.Modifiedon = DateTime.Now;
                dataSource.Modifiedby = WindowsIdentity.GetCurrent().Name;
            }

            if (condtionListBox.SelectedItem != null)
            {
                var condition = condtionListBox.SelectedItem as Condition;
                condition.Dataquery = SQLConditionQueryEditor.Text;
                condition.Modifiedon = DateTime.Now;
                condition.Modifiedby = WindowsIdentity.GetCurrent().Name;
                condition.ChangedDataQuery = string.Empty;
            }

            if (condtionListBox2.SelectedItem != null)
            {
                var condition2 = condtionListBox2.SelectedItem as Condition;
                condition2.Dataquery = SQLConditionQueryEditor2.Text;
                condition2.Modifiedon = DateTime.Now;
                condition2.Modifiedby = WindowsIdentity.GetCurrent().Name;
                condition2.ChangedDataQuery = string.Empty;
            }

            if (RepParamListBox.SelectedItem != null)
            {
                var parameter = RepParamListBox.SelectedItem as Parameter;
                parameter.Query = SQLRepParamQueryEditor.Text;
                parameter.Modifiedon = DateTime.Now;
                parameter.Modifiedby = WindowsIdentity.GetCurrent().Name;
            }

            if (FilterListBox.SelectedItem != null)
            {
                var filter = FilterListBox.SelectedItem as Filter;
                filter.Query = SQLFilterQueryEditor.Text;
                filter.ModifiedOn = DateTime.Now;
                filter.ModifiedBy = WindowsIdentity.GetCurrent().Name;
            }

            var context = ((App)Application.Current).GetDBContext;
            context.SaveChanges();

            ReportIsChanged = false;
        }

        private bool CheckData()
        {
            var result = false;

            if (!CheckValues(CurrentReport.Reporttype, reporttype.SelectedValue))
                return true;

            if (!CheckValues(CurrentReport.Sqlqueryfilename, SQLQueryFileName.Text))
                return true;

            if (!CheckValues(reportformat.SelectedValue, CurrentReport.Reportformat))
                return true;

            if (!CheckValues(ReportCodeTextBox.Text, CurrentReport.Reportcode))
                return true;

            if (!CheckValues(SpLoadTypeComboBox.SelectedValue, CurrentReport.SpLoadType))
                return true;

            if (!CheckValues(CurrentReport.SqlQueryUseCondition, SqlQueryUseCondition.Text))
                return true;

            if (treeView1.SelectedItem != null)
            {
                var ds = treeView1.SelectedItem as DataSource;

                if (!CheckValues(ds.Dataquery, CurrentReport.Datasource.Dataquery))
                    return false;
            }

            if (condtionListBox.SelectedItem != null)
            {
                var condition = condtionListBox.SelectedItem as Condition;

                if (CurrentReport.ReportConditions == null || !CurrentReport.ReportConditions.Any())
                    return false;

                var currentCondition = CurrentReport.ReportConditions.Select(x => x.Condition).Where(s => s.Conditionid == condition.Conditionid).FirstOrDefault();

                if (currentCondition == null)
                    return false;

                if (!CheckValues(condition.Dataquery, currentCondition.Dataquery))
                    return false;
            }

            if (condtionListBox2.SelectedItem != null)
            {
                var condition2 = condtionListBox2.SelectedItem as Condition;

                if (CurrentReport.ReportConditions == null || !CurrentReport.ReportConditions.Any())
                    return false;

                var currentCondition = CurrentReport.ReportConditions.Select(x => x.Condition).Where(s => s.Conditionid == condition2.Conditionid).FirstOrDefault();

                if (currentCondition == null)
                    return false;

                if (!CheckValues(condition2.Dataquery, currentCondition.Dataquery))
                    return false;
            }

            if (RepParamListBox.SelectedItem != null)
            {
                var parameter = RepParamListBox.SelectedItem as Parameter;

                if (CurrentReport.ReportParameters == null || !CurrentReport.ReportParameters.Any())
                    return false;

                var currentParam = CurrentReport.ReportParameters.Select(x => x.Parameter).FirstOrDefault(s => s.id == parameter.Id);

                if (currentParam == null)
                    return false;

                if (!CheckValues(parameter.Query, currentParam.query))
                    return false;
            }

            if (!CheckValues(CurrentReport.SqlMainReportCode, SqlMainReportCode.Text))
                return true;

            if (!CheckValues(CurrentReport.SqlMainRowId, SqlMainRowId.Text))
                return true;

            return result;
        }

        private bool CheckValues(object val1, object val2)
        {
            if (val1 == null && (val2 != null && !string.IsNullOrEmpty(val2.ToString())))
                return false;
            else if (val2 == null && (val1 != null && !string.IsNullOrEmpty(val1.ToString())))
                return false;

            if (val1 is string && val2 is string)
            {
                if (!val1.Equals(val2))
                    return false;
            }

            return true;
        }

        private void SelectDocPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog();
                dlg.DefaultExt = ".doc";
                dlg.Filter = "Документ Word (*.doc)|*.doc|Документ Word 2007-2010 (*.docx)|*.docx|Документ Word с поддержкой макросов(*.docm)|*.docm|Книга Excel (*.xls)|*.xls|Книга Excel 2007-2010 (*.xlsx)|*.xlsx";

                if (dlg.ShowDialog() == true)
                    CurrentReport.Reportpath = dlg.FileName;
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DoDesign_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentReport.IsCustomTemplate)
            {
                RunAgreementTool();
                return;
            }

            if (string.IsNullOrEmpty(CurrentReport.Reportpath))
            {
                MessageBox.Show("Не указан путь к документу слияния", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var useaddin = ConfigurationManager.AppSettings["UseWordAddIn"];

            if (!string.IsNullOrEmpty(useaddin) && Convert.ToBoolean(useaddin))
                DocBuilder.SetDataForWordAddIn(((App)Application.Current).GetConnection, FillParams(), CurrentReport.Testid ?? string.Empty);

            try
            {
                Process.Start(CurrentReport.Reportpath);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.OldValue != null)
            {
                if (e.OldValue is DataSource)
                {
                    var ds = e.OldValue as DataSource;
                    ds.Dataquery = SQLDSQueryEditor.Text;
                    ds.PropertyChanged -= OnReportPropertyChanged;
                }
            }

            DataSourceGrid.DataContext = e.NewValue;

            if (e.NewValue is DataSource)
            {
                var dataSource = e.NewValue as DataSource;
                TextSettingModeOn = true;
                SQLDSQueryEditor.Text = dataSource.Dataquery;
                TextSettingModeOn = false;
                SQLDSQueryEditor.IsEnabled = true;
                DataSourceNameTextBox.IsEnabled = IsUnlocked && CurrentReport.Useglobal != true;
                KeyFieldNameTextBox.IsEnabled = IsUnlocked && CurrentReport.Useglobal != true;
                ForeignKeyFieldNameTextBox.IsEnabled = IsUnlocked && dataSource.ParentDataSource != null && CurrentReport.Useglobal != true;
                SQLDSQueryEditor.Select(0, 0);
                dataSource.PropertyChanged += OnReportPropertyChanged;

                if (CurrentType == EditPageType.Report)
                {
                    if (dataSource.Datasourceid.Equals(CurrentReport.Datasource?.Datasourceid))
                    {
                        GlobalDataSources.Visibility = Visibility.Visible;
                        useGlobal.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GlobalDataSources.Visibility = Visibility.Collapsed;
                        useGlobal.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    GlobalDataSources.Visibility = Visibility.Collapsed;
                    useGlobal.Visibility = Visibility.Collapsed;
                }
            }

            Up.Opacity = 0.6;
            Down.Opacity = 0.6;

            Up.IsEnabled = false;
            Down.IsEnabled = false;

            var selectReport = treeView1.SelectedItem as DataSource;
            if (selectReport.ParentDataSource == null)
                return;

            var currentNestedDataSources = selectReport.ParentDataSource.InverseParentDataSource.ToList();
            currentNestedDataSources.Sort(CompareDinosByLength);

            for (int i = 0; i < currentNestedDataSources.Count; i++)
            {
                if (currentNestedDataSources[i].Datasourceid == selectReport.Datasourceid)
                {
                    if (i != 0)
                    {
                        Up.Opacity = 1;
                        Up.IsEnabled = true;
                    }

                    if (i < currentNestedDataSources.Count - 1)
                    {
                        Down.Opacity = 1;
                        Down.IsEnabled = true;
                    }
                }
            }
        }

        private void AddNestedDSButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (treeView1.SelectedItem != null && ((DataSource)treeView1.SelectedItem).ParentDataSource != null)
                {
                    if (!IsGlobal && CurrentReport.Reportpath == null)
                    {
                        MessageBox.Show("Задайте путь к шаблону на сервере, прежде чем добавлять дочерний источник к дочернему источнику", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (!IsGlobal && ((DataSource)treeView1.SelectedItem).ParentDataSource.ParentDataSource != null && (CurrentReport.Reportpath.EndsWith("xlsx") || CurrentReport.Reportpath.EndsWith("xls")))
                    {
                        MessageBox.Show("Для excel-шаблона не предусмотрено более двух уровней дочерних источников данных", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                if (treeView1.SelectedItem == null)
                {
                    MessageBox.Show("Выберите родительский источник данных", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dataSource = treeView1.SelectedItem as DataSource;

                var newDataSource = new DataSource()
                {
                    Datasourceid = Guid.NewGuid(),
                    Datasourcename = "Новый источник данных",
                    Position = dataSource.InverseParentDataSource.Count,
                    ParentDataSource = dataSource
                };

                newDataSource.InverseParentDataSource.CollectionChanged += OnCollectionAssociationChanged;

                dataSource.InverseParentDataSource.Add(newDataSource);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveNestedDSButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (treeView1.SelectedItem == null)
                {
                    MessageBox.Show("Выберите источник данных", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedDataSource = treeView1.SelectedItem as DataSource;

                if (selectedDataSource.InverseParentDataSource.Count() > 0)
                {
                    MessageBox.Show("Источник данных содержит вложенные источники данных. Удаление невозможно", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (selectedDataSource.ParentDataSource == null)
                {
                    MessageBox.Show("Невозможно удалить родительский источник данных. Удаление невозможно", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Удалить источник данных?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var parentDataSource = selectedDataSource.ParentDataSource;
                    parentDataSource.InverseParentDataSource.Remove(selectedDataSource);

                    var context = ((App)Application.Current).GetDBContext;

                    if (context.Entry(selectedDataSource).State != EntityState.Detached)
                        context.DataSources.Remove(selectedDataSource);
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CustomTemplateCheckBox.Checked -= CustomTemplateChecked;
            CustomTemplateCheckBox.Unchecked -= CustomTemplateChecked;

            ShareSaveCheckBox.Checked -= ShareSaveCheckBoxChecked;
            ShareSaveCheckBox.Unchecked -= ShareSaveCheckBoxChecked;

            SendRequestCheckBox.Checked -= SendRequestCheckBoxChecked;
            SendRequestCheckBox.Unchecked -= SendRequestCheckBoxChecked;

            DSCheckBox.Checked -= DSCheckBoxChecked;
            DSCheckBox.Unchecked -= DSCheckBoxChecked;

            ChangeLockReport(false);
        }

        private void AddBusinessRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AvalableRolesListBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите бизнес роль слева, чтобы добавить ее к отчету", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var cnt = (from p in CurrentReport.BusinessRoleReports where p.BusinessRole == (BusinessRole)AvalableRolesListBox.SelectedItem select p.BusinessRole).Count();

                if (cnt > 0)
                {
                    MessageBox.Show("Данная бизнес роль уже добавлена к отчету", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                CurrentReport.BusinessRoleReports.Add(new BusinessRoleReport()
                {
                    BusinessRoleReportId = Guid.NewGuid(),
                    BusinessRole = (BusinessRole)AvalableRolesListBox.SelectedItem,
                    Report = CurrentReport
                });

                ReloadExistingBusinessRoles();
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ReloadExistingBusinessRoles()
        {
            var existsBusinessRoles = from p in CurrentReport.BusinessRoleReports select p;
            ExistsBusinessRoleListBox.ItemsSource = existsBusinessRoles;
            ExistsBusinessRoleListBox.DisplayMemberPath = "BusinessRole.rolename";
            CurrentReport.BusinessRoleReports.CollectionChanged += OnCollectionAssociationChanged;
        }

        private void AddAllBusinessRoles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Добавить все бизнес роли в отчет?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    foreach (var r in Roles)
                    {
                        var cnt = (from p in CurrentReport.BusinessRoleReports where p.BusinessRole.Businessroleid.Equals(r.Businessroleid) select p.BusinessRole).Count();

                        if (cnt == 0)
                        {
                            CurrentReport.BusinessRoleReports.Add(new BusinessRoleReport()
                            {
                                BusinessRoleReportId = Guid.NewGuid(),
                                BusinessRole = r,
                                Report = CurrentReport
                            });
                        }

                    }

                    ReloadExistingBusinessRoles();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveBusinessRole_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ExistsBusinessRoleListBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите бизнес роль справа, чтобы удалить ее из отчета", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Удалить роль из отчета?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var ctx = ((App)Application.Current).GetDBContext;
                    ctx.BusinessRoles.Remove(ExistsBusinessRoleListBox.SelectedItem as BusinessRole);
                    ReloadExistingBusinessRoles();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveAllBusinessRoles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Удалить все роли из отчета?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var ctx = ((App)Application.Current).GetDBContext;

                    var rr = (from p in CurrentReport.BusinessRoleReports select p).ToList();

                    foreach (var r in rr)
                        ctx.BusinessRoleReports.Remove(r);

                    ReloadExistingBusinessRoles();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl1.SelectedItem == tabItem3)
            {
                treeView1.Focus();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveData();
                RunTest(true);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void AddCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var condition = new Condition();
                condition.Conditionid = Guid.NewGuid();
                condition.Conditionname = "Новая проверка";
                condition.Conditionoperator = "=";
                condition.Dataquery = "select 1 as pk";
                condition.Recordcount = 1;
                condition.Precondition = false;
                condition.Errormessage = "";
                condition.Isglobal = false;
                condition.Createdon = DateTime.Now;
                condition.Modifiedon = DateTime.Now;
                condition.Createdby = WindowsIdentity.GetCurrent().Name;
                condition.Modifiedby = WindowsIdentity.GetCurrent().Name;

                var reportCondition = new ReportCondition();
                reportCondition.Id = Guid.NewGuid();
                reportCondition.Report = CurrentReport;
                reportCondition.Condition = condition;

                CurrentReport.ReportConditions.Add(reportCondition);
                ReloadConditions(reportCondition);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void condtionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (conditionGrid.DataContext is Condition)
                {
                    var condition = conditionGrid.DataContext as Condition;

                    if (SQLConditionQueryEditor.Text != null)
                        condition.Dataquery = SQLConditionQueryEditor.Text;

                    condition.PropertyChanged -= OnReportPropertyChanged;
                }

                ConditionSelect(condtionListBox.SelectedItem as Condition);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ConditionSelect(Condition condition = null)
        {
            if (condition != null)
            {
                conditionGrid.DataContext = condition;
                TextSettingModeOn = true;
                SQLConditionQueryEditor.Text = condition.Dataquery;
                TextSettingModeOn = false;
                SQLConditionQueryEditor.IsEnabled = true;
                ConditionNameTextBox.IsEnabled = true;
                RecordCountTextBox.IsEnabled = true;
                SQLConditionQueryEditor.Select(0, 0);
                condition.PropertyChanged += OnReportPropertyChanged;

                if (CurrentType == EditPageType.Report && condition.Isglobal == true)
                    conditionGrid.IsEnabled = false;
                else
                    conditionGrid.IsEnabled = true;
            }
        }

        private void RunConditionsCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveData();

                foreach (var con in CurrentReport.ReportConditions)
                    con.Condition.ChangedDataQuery = con.Condition.DataQuery.Replace("@reportcode", $"'{CurrentReport.Reportcode}'");

                foreach (var con in CurrentReport.ReportConditions)
                    con.Condition.ChangedDataQuery = con.Condition.DataQuery.Replace("@userid", $"'{CurrentReport.Testuserid}'");

                var errors = ExtractDataEngine.GetErrors(((App)Application.Current).GetConnection, CurrentReport, CurrentReport.Testid, !string.IsNullOrEmpty(CurrentReport.Testuserid) ? Guid.Parse(CurrentReport.Testuserid) : Guid.Empty);

                if (errors.Count > 0)
                {
                    var errmsg = new StringBuilder();

                    foreach (var err in errors)
                        errmsg.AppendLine(err);

                    MessageBox.Show(errmsg.ToString(), "Список ошибок", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                    MessageBox.Show("Проверка отчета прошла успешно. Ошибок не обнаружено", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
            finally
            {
                ReportIsChanged = false;
            }
        }

        private void RemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (condtionListBox.SelectedItem == null)
                {
                    if (CurrentType != EditPageType.Condition)
                        MessageBox.Show("Выберите условие, чтобы удалить его из отчета", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else
                        MessageBox.Show("Выберите условие, чтобы удалить его", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                if (MessageBox.Show("Удалить условие?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var context = ((App)Application.Current).GetDBContext;
                    var condition = (Condition)condtionListBox.SelectedItem;
                    condtionListBox.SelectedItem = null;

                    var reportCondition = condition.ReportConditions.FirstOrDefault(x => x.Report.Reportid == CurrentReport.Reportid);
                    if (reportCondition != null)
                    {
                        context.ReportConditions.Remove(reportCondition);
                        CurrentReport.ReportConditions.Remove(reportCondition);
                    }

                    if (condition.Isglobal != true)
                        context.Conditions.Remove(condition);

                    conditionGrid.DataContext = null;

                    ReloadConditions();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (CurrentType)
                {
                    case EditPageType.Report:
                        ImportExport.Export(CurrentReport);
                        break;

                    case EditPageType.DataSource:
                        ImportExport.Export(CurrentDataSource);
                        break;

                    case EditPageType.Condition:
                        ImportExport.Export(CurrentCondition);
                        break;

                    case EditPageType.Parameter:
                        ImportExport.Export(CurrentParameter);
                        break;
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DocName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CaptionLabel.Content = $"Редактирование документа слияния - {DocName.Text}";
        }

        private void AddCondition2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var condition = new Condition();
                condition.Conditionid = Guid.NewGuid();
                condition.Conditionname = "Новая проверка";
                condition.Conditionoperator = "=";
                condition.Dataquery = "select 1 as pk";
                condition.Recordcount = 1;
                condition.Conditiontype = 1;
                condition.Errormessage = "";
                condition.Isglobal = false;
                condition.Createdon = DateTime.Now;
                condition.Modifiedon = DateTime.Now;
                condition.Createdby = WindowsIdentity.GetCurrent().Name;
                condition.Modifiedby = WindowsIdentity.GetCurrent().Name;

                var reportCondition = new ReportCondition();
                reportCondition.Id = Guid.NewGuid();
                reportCondition.Report = CurrentReport;
                reportCondition.Condition = condition;

                CurrentReport.ReportConditions.Add(reportCondition);
                ReloadConditions();
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveCondition2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (condtionListBox2.SelectedItem == null)
                {

                    if (CurrentType != EditPageType.Condition)
                        MessageBox.Show("Выберите условие, чтобы удалить его из отчета", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else
                        MessageBox.Show("Выберите условие, чтобы удалить его", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                if (MessageBox.Show("Удалить условие?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var context = ((App)Application.Current).GetDBContext;
                    var condition = (Condition)condtionListBox2.SelectedItem;

                    var reportCondition = condition.ReportConditions.FirstOrDefault(x => x.Report.Reportid == CurrentReport.Reportid);
                    if (reportCondition != null)
                    {
                        context.ReportConditions.Remove(reportCondition);
                        CurrentReport.ReportConditions.Remove(reportCondition);
                    }

                    if (condition.Isglobal != true)
                        context.Conditions.Remove(condition);

                    ReloadConditions();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void condtionListBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (conditionGrid2.DataContext is Condition)
                {
                    var condition = conditionGrid2.DataContext as Condition;

                    if (SQLConditionQueryEditor2.Text != null)
                        condition.Dataquery = SQLConditionQueryEditor2.Text;
                }
                if (condtionListBox2.SelectedItem != null)
                {
                    conditionGrid2.DataContext = condtionListBox2.SelectedItem;

                    if (condtionListBox2.SelectedItem is Condition)
                    {
                        var condition = condtionListBox2.SelectedItem as Condition;
                        SQLConditionQueryEditor2.Text = condition.Dataquery;
                        SQLConditionQueryEditor2.IsEnabled = true;
                        ConditionNameTextBox2.IsEnabled = true;
                        RecordCountTextBox2.IsEnabled = true;
                        SQLConditionQueryEditor2.Select(0, 0);
                    }
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ServerTextBox.Text))
            {
                if (!string.IsNullOrEmpty(CurrentReport.Defaultdatabase))
                    InitDataBaseList(ServerTextBox.Text, CurrentReport.Defaultdatabase);
                else
                    InitDataBaseList(ServerTextBox.Text, null);
            }

            BaseTextComboBox.SelectedValue = CurrentReport.Defaultdatabase;

            if (CurrentReport.SharepointDosave == null)
                CurrentReport.SharepointDosave = false;

            if (CurrentReport.Removeemptyregions == null)
                CurrentReport.Removeemptyregions = true;

            if (CurrentReport.Replacefieldswithstatictext == null)
                CurrentReport.replacefieldswithstatictext = true;

            if (!string.IsNullOrEmpty(CurrentReport.Reporttype))
            {
                switch (CurrentReport.Reporttype.ToLower())
                {
                    case "doc":
                        reporttype.SelectedIndex = 0;
                        break;

                    case "rep":
                        reporttype.SelectedIndex = 1;
                        break;

                    case "eml":
                        reporttype.SelectedIndex = 2;
                        break;

                    case "ins":
                        reporttype.SelectedIndex = 3;
                        break;
                }
            }

            if (string.IsNullOrEmpty(CurrentReport.Reportformat))
                reportformat.SelectedIndex = 0;
            else if (CurrentReport.Reportformat.ToLower() == "pdf")
                reportformat.SelectedIndex = 1;
            else if (CurrentReport.Reportformat.ToLower() == "html")
                reportformat.SelectedIndex = 3;
            else
                reportformat.SelectedIndex = 2;

            if (CurrentReport.IsShow == null)
                CurrentReport.IsShow = true;

            if (!string.IsNullOrEmpty(CurrentReport.sqlqueryfilename))
                SQLQueryFileName.Text = CurrentReport.sqlqueryfilename;

            if (!string.IsNullOrEmpty(CurrentReport.SqlQueryUseCondition))
                SqlQueryUseCondition.Text = CurrentReport.SqlQueryUseCondition;

            InitCustomTemplates();
            InitSpComboBoxes();
            InitIntegrationSources();
            InitDSFields();

            CheckSpFields();
            CheckIntegrationFields();
            CheckDSFields();

            ChangeCustomTemplateVisibility(CurrentReport != null & CurrentReport.IsCustomTemplate);
            CustomTemplateCheckBox.Checked += CustomTemplateChecked;
            CustomTemplateCheckBox.Unchecked += CustomTemplateChecked;

            SendRequestCheckBox.Checked += SendRequestCheckBoxChecked;
            SendRequestCheckBox.Unchecked += SendRequestCheckBoxChecked;

            ShareSaveCheckBox.Checked += ShareSaveCheckBoxChecked;
            ShareSaveCheckBox.Unchecked += ShareSaveCheckBoxChecked;

            DSCheckBox.Checked += DSCheckBoxChecked;
            DSCheckBox.Unchecked += DSCheckBoxChecked;

            CurrentReport.PropertyChanged += OnReportPropertyChanged;
            NavigationService.Navigating += NavigationService_Navigating;
        }

        private void tabItem3_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void AddRepParam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var parameter = new Parameter();
                parameter.Id = Guid.NewGuid();
                parameter.Name = "Новый параметр";
                parameter.Datatype = "String";
                parameter.Isglobal = false;
                parameter.Createdon = DateTime.Now;
                parameter.Modifiedon = DateTime.Now;
                parameter.Createdby = WindowsIdentity.GetCurrent().Name;
                parameter.Modifiedby = WindowsIdentity.GetCurrent().Name;

                var reportParameter = new ReportParameter();
                reportParameter.Id = Guid.NewGuid();
                reportParameter.Report = CurrentReport;
                reportParameter.Parameter = parameter;

                CurrentReport.ReportParameters.Add(reportParameter);
                ReloadRepParams();
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveRepParam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RepParamListBox.SelectedItem == null)
                {
                    if (CurrentType != EditPageType.Parameter)
                        MessageBox.Show("Выберите параметр, чтобы удалить его из отчета", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    else
                        MessageBox.Show("Выберите параметр, чтобы удалить его", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                if (MessageBox.Show("Удалить параметр?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var context = ((App)Application.Current).GetDBContext;
                    var parameter = (Parameter)RepParamListBox.SelectedItem;

                    var reportParameter = parameter.ReportParameters.FirstOrDefault(x => x.Report.Reportid == CurrentReport.Reportid);
                    if (reportParameter != null)
                    {
                        context.ReportParameters.Remove(reportParameter);
                        CurrentReport.ReportParameters.Remove(reportParameter);
                    }

                    if (parameter.Isglobal != true)
                    {
                        while (parameter.ParameterConditions.Any())
                            context.ParameterConditions.Remove(parameter.ParameterConditions.First());

                        context.Parameters.Remove(parameter);
                    }

                    context.SaveChanges();

                    ReloadRepParams();
                }
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void GetGlobals()
        {
            var context = ((App)Application.Current).GetDBContext;

            var dataSourcesMap = new Dictionary<Guid, string>();
            var globals = context.DataSources.Where(x => x.Isglobal == true).OrderBy(i => i.Datasourcename);

            foreach (var item in globals)
                dataSourcesMap.Add(item.Datasourceid, item.Datasourcename);

            GlobalDataSources.ItemsSource = dataSourcesMap;

            var conditionsMap = new Dictionary<Guid, string>();
            var globalsCond = context.Conditions.Where(x => x.Isglobal == true).OrderBy(i => i.Conditionname);

            foreach (var item in globalsCond)
                conditionsMap.Add(item.Conditionid, item.Conditionname);

            GlobalConditions.ItemsSource = conditionsMap;

            var parametersMap = new Dictionary<Guid, string>();
            var globalParameters = context.Parameters.Where(x => x.Isglobal == true).OrderBy(i => i.Name);

            foreach (var item in globalParameters)
                parametersMap.Add(item.Id, item.Name);

            GlobalParameters.ItemsSource = parametersMap;

            var filtersMap = new Dictionary<Guid, string>();
            var globalFilters = context.Filters.OrderBy(f => f.Name);

            foreach (var item in globalFilters)
                filtersMap.Add(item.Id, item.Name);

            GlobalFilter.ItemsSource = filtersMap;
        }

        private void RepParamListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (RepParamsGrid.DataContext is Parameter)
                {
                    var parameter = RepParamsGrid.DataContext as Parameter;

                    if (SQLRepParamQueryEditor.Text != null)
                        parameter.Query = SQLRepParamQueryEditor.Text;

                    parameter.PropertyChanged -= OnReportPropertyChanged;
                }
                ParameterSelect(RepParamListBox.SelectedItem as Parameter);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ParameterSelect(Parameter parameter = null)
        {
            if (parameter != null)
            {
                RepParamsGrid.DataContext = parameter;
                TextSettingModeOn = true;
                SQLRepParamQueryEditor.Text = parameter.Query;
                TextSettingModeOn = false;
                SQLRepParamQueryEditor.Select(0, 0);
                parameter.PropertyChanged += OnReportPropertyChanged;

                if (CurrentType == EditPageType.Report && parameter.Isglobal == true)
                    RepParamsGrid.IsEnabled = false;
                else
                    RepParamsGrid.IsEnabled = true;
            }
        }

        private void RepParamDataTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void reporttype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reporttype.SelectedValue != null && (reporttype.SelectedValue.ToString().Equals(StringComparison.InvariantCultureIgnoreCase, "rep", "doc")))
                tabRepParams.Visibility = Visibility.Visible;
            else
                tabRepParams.Visibility = Visibility.Hidden;

            if (reporttype.SelectedValue != null && reporttype.SelectedValue.ToString().ToLower() == "eml")
            {
                reportformat.IsEnabled = false;

                var docType = reportformat.Items.Cast<ComboBoxItem>().Where(i => i.GetValue(TagProperty).ToString() == "html").FirstOrDefault();
                var index = reportformat.Items.IndexOf(docType);

                reportformat.SelectedIndex = index;
                SubjectLabel.IsEnabled = true;
                SubjectTextBox.IsEnabled = true;
            }
            else
            {
                reportformat.IsEnabled = true;
                SubjectLabel.IsEnabled = false;
                SubjectTextBox.IsEnabled = false;
            }
        }

        private void SlideDown(object sender, RoutedEventArgs e)
        {
            if (treeView1.SelectedItem == null)
                return;

            var currentDataSources = treeView1.ItemsSource as List<DataSource>;
            var selectedDataSource = treeView1.SelectedItem as DataSource;

            SortDataSourceByPosition(currentDataSources.FirstOrDefault(), selectedDataSource.Datasourceid, false);
        }

        private void SlideUp(object sender, RoutedEventArgs e)
        {
            if (treeView1.SelectedItem == null)
                return;

            var currentDataSources = treeView1.ItemsSource as List<DataSource>;
            var selectedDataSource = treeView1.SelectedItem as DataSource;

            SortDataSourceByPosition(currentDataSources.FirstOrDefault(), selectedDataSource.Datasourceid, true);
        }

        private static int CompareDinosByLength(DataSource x, DataSource y)
        {
            if (x.Position == null && y.Position == null)
                return 0;
            else if (x.Position == null)
                return -1;
            else if (y.Position == null)
                return 1;
            else
                return x.Position.Value.CompareTo(y.Position.Value);
        }

        private void SortDataSourceByPosition(DataSource dataSource, Guid dataSourceId, Boolean Up)
        {
            var currentNested = dataSource.InverseParentDataSource.ToList();
            currentNested.Sort(CompareDinosByLength);

            for (int i = 0; i < currentNested.Count; i++)
            {
                if (currentNested[i].InverseParentDataSource.Count != 0)
                    SortDataSourceByPosition(currentNested[i], dataSourceId, Up);

                if (currentNested[i].Datasourceid == dataSourceId)
                {
                    if (Up)
                    {
                        currentNested[i].Position -= 1;
                        currentNested[i - 1].Position += 1;
                    }
                    else
                    {
                        currentNested[i].Position += 1;
                        currentNested[i + 1].Position -= 1;
                    }

                    currentNested.Sort(CompareDinosByLength);
                    dataSource.InverseParentDataSource.Clear();

                    foreach (var item in currentNested)
                        dataSource.InverseParentDataSource.Add(item);

                    break;
                }
            }
        }

        private void SortDataSourceByPosition(DataSource dataSource)
        {
            var currentNested = dataSource.InverseParentDataSource.ToList();

            if (currentNested.Count != 0)
            {
                currentNested.Sort(CompareDinosByLength);
                dataSource.InverseParentDataSource.Clear();

                for (int i = currentNested.Count - 1; i >= 0; i--)
                {
                    currentNested[i].Position = i;
                    dataSource.InverseParentDataSource.Add(currentNested[i]);

                    if (currentNested[i].InverseParentDataSource.Count != 0)
                        SortDataSourceByPosition(currentNested[i]);

                    currentNested[i].InverseParentDataSource.CollectionChanged += OnCollectionAssociationChanged;
                }
            }
        }

        private static void replaceUserID(DataSource Source, string userId)
        {
            foreach (var ds in Source.InverseParentDataSource)
            {
                ds.Dataquery = ds.Dataquery.Replace("@userid", $"'{userId}'");

                if (ds.InverseParentDataSource != null)
                    replaceUserID(ds, userId);
            }
        }

        private static void replaceReportCode(DataSource Source, string reportCode)
        {
            foreach (var ds in Source.InverseParentDataSource)
            {
                ds.Dataquery = ds.Dataquery.Replace("@reportcode", $"'{reportCode}'");

                if (ds.InverseParentDataSource != null)
                    replaceReportCode(ds, reportCode);
            }
        }

        private void ReportCodeTextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ReportCodeTextBox.Text))
                return;

            var context = ((App)Application.Current).GetDBContext;

            if (context.Reports.Any(i => i.Reportcode == ReportCodeTextBox.Text && i.Reportid != CurrentReport.Reportid))
            {
                ReportCodeTextBox.Text = string.Empty;

                MessageBox.Show("Данный код интеграции документа уже используется.\nВведите другой код.");

                e.Handled = true;
            }
        }

        private void ServerTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var selectedValue = string.Empty;

            if (BaseTextComboBox.SelectedValue != null)
                selectedValue = BaseTextComboBox.SelectedValue.ToString();

            BaseTextComboBox.ItemsSource = null;
            InitDataBaseList(ServerTextBox.Text, selectedValue);
        }

        private void InitDataBaseList(string serverName, string selectedValue)
        {
            if (!TestForServer(serverName))
                return;

            var databases = new Dictionary<string, string>();
            var sqlConn = ExtractDataEngine.GetConnection(((App)Application.Current).GetConnection.CreateConnection(serverName, null));

            sqlConn.Open();

            var tblDatabases = sqlConn.GetSchema("Databases");

            sqlConn.Close();

            foreach (DataRow row in tblDatabases.Rows)
            {
                var strDatabaseName = row["database_name"].ToString();

                if (!strDatabaseName.ToUpper().Contains("_WORDMERGERDB") && !strDatabaseName.ToUpper().Contains("TEMPLATE"))
                    databases.Add(strDatabaseName, strDatabaseName);
            }

            BaseTextComboBox.ItemsSource = databases.OrderBy(i => i.Key);

            if (!string.IsNullOrEmpty(selectedValue))
                BaseTextComboBox.SelectedValue = selectedValue;
        }

        private void InitCustomTemplates()
        {
            if (CurrentReport == null)
                return;

            var context = ((App)Application.Current).GetDBContext;
            var documents = context.Documents.OrderBy(x => x.Name).ToDictionary(x => x.Id, y => y.Name);

            CustomTemplateComboBox.ItemsSource = documents;

            if (CurrentReport.Document != null)
                CustomTemplateComboBox.SelectedValue = CurrentReport.Document.Id;
        }

        private void InitSpComboBoxes()
        {
            SpLoadTypeComboBox.ItemsSource = GetEnumValues<Enums.SpLoadEnum>();

            if (!string.IsNullOrEmpty(CurrentReport.SpLoadType))
                SpLoadTypeComboBox.SelectedValue = CurrentReport.SpLoadType;

            ChangeNameTypeComboBox.ItemsSource = GetEnumValues<Enums.ChangeNameTypeEnum>();

            if (!string.IsNullOrEmpty(CurrentReport.NamePostfix))
                ChangeNameTypeComboBox.SelectedValue = CurrentReport.NamePostfix;
            else
                ChangeNameTypeComboBox.SelectedValue = Enums.ChangeNameTypeEnum.WithDateAndTime.ToString();
        }

        private List<KeyValuePair<string, string>> GetEnumValues<T>() where T : Enum
        {
            var source = new List<KeyValuePair<string, string>>();

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                source.Add(new KeyValuePair<string, string>(value.GetDisplayName(), value.ToString()));
            }

            return source;
        }

        private bool TestForServer(string address)
        {
            var timeout = 500;
            var result = false;
            var port = ((App)Application.Current).GetConnection.ConnectionType == ConnectionType.PostgresDbConnection ? 5432 : 1433;

            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    var asyncResult = socket.BeginConnect(address, port, null, null);
                    result = asyncResult.AsyncWaitHandle.WaitOne(timeout, true);
                    socket.Close();
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        private void RunQuery_Click(object sender, RoutedEventArgs e)
        {
            GetQueryResult();
        }

        private void GetQueryResult()
        {
            var connection = ((App)Application.Current).GetConnection.CreateConnection(ServerTextBox.Text, BaseTextComboBox.SelectedValue == null ? null : BaseTextComboBox.SelectedValue.ToString());

            if (string.IsNullOrEmpty(SQLDSQueryEditor.Text))
            {
                MessageBox.Show(string.Format("Не задан текст запроса"));
                return;
            }

            if (string.IsNullOrEmpty(connection.ServerName))
            {
                MessageBox.Show(string.Format("Не заполнен адрес сервера БД"));
                return;
            }

            if (BaseTextComboBox.SelectedValue == null || string.IsNullOrEmpty(connection.DatabaseName))
            {
                MessageBox.Show(string.Format("Не заполнено имя БД"));
                return;
            }

            var strConn = connection.ToString();

            try
            {
                if (SQLDSQueryEditor.Text.Contains("?") && CurrentReport.Testid == null)
                    throw new Exception("В запросе используется параметр \"?\", но в текущем отчете не задан тестовый идентификатор. Заполните тестовый идентификатор, и попробуйте выполнить запрос.");

                var queryWithParam = Helper.InsertParametrValueInQuery(SQLDSQueryEditor.Text, CurrentReport.Testid);

                queryWithParam = queryWithParam.Replace("@userid", $"'{CurrentReport.Testuserid}'");
                queryWithParam = queryWithParam.Replace("@reportcode", $"'{CurrentReport.Reportcode}'");
                queryWithParam = SubstituteParams(queryWithParam);

                var objConnection = ExtractDataEngine.GetConnection(connection);
                var objCommand = ExtractDataEngine.GetCommand(objConnection, queryWithParam);
                var objAdapater = ExtractDataEngine.GetAdapter(objCommand);
                var objDataset = new DataSet();
                objConnection.Open();
                objAdapater.Fill(objDataset);
                objConnection.Close();

                if (objDataset == null || objDataset.Tables == null || objDataset.Tables.Count <= 0 || objDataset.Tables[0].Rows == null || objDataset.Tables[0].Rows.Count <= 0)
                {
                    DataGridResultQuery.ItemsSource = (new DataTable()).AsDataView();
                    CountResultQuery.Content = 0;
                }
                else
                {
                    objDataset.ChangeQuotes();

                    var resQuery = objDataset.Tables[0];
                    DataGridResultQuery.ItemsSource = resQuery.AsDataView();
                    CountResultQuery.Content = resQuery.Rows.Count;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Не удалось выполнить запрос \n строка соединения с БД: {0}\n Ex:{1}", strConn, ex.Message));
            }
        }

        private bool IsActiveIntervalCorrect()
        {
            if (ActiveOn == null || DeactiveOn == null || ActiveOn.SelectedDate == null || DeactiveOn.SelectedDate == null)
                return true;

            return (ActiveOn.SelectedDate <= DeactiveOn.SelectedDate);
        }

        private void ActiveOn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsActiveIntervalCorrect())
            {
                MessageBox.Show("Дата начала действия не может быть больше даты окончания документа!");

                ActiveOn.SelectedDate = null;
                ActiveOn.Focus();
            }
        }

        private void DeactiveOn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsActiveIntervalCorrect())
            {
                MessageBox.Show("Дата начала действия не может быть больше даты окончания документа!");

                DeactiveOn.SelectedDate = null;
                DeactiveOn.Focus();
            }
        }

        private void NavigationService_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            Keyboard.ClearFocus();

            var unsubscribe = true;

            if (!IsUnlocked)
            {
                RollbackChanges();
            }
            else if (ReportIsChanged)
            {
                var name = GetStringName(true);
                var res = MessageBox.Show(name + " был изменен, вы хотите сохранить изменения?", "Подтвердите действие", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (res == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    unsubscribe = false;
                }
                else if (res == MessageBoxResult.Yes)
                    SaveData();
                else if (res == MessageBoxResult.No)
                    RollbackChanges();
            }

            if (unsubscribe)
            {
                NavigationService.Navigating -= NavigationService_Navigating;
                CurrentReport.PropertyChanged -= OnReportPropertyChanged;
            }
        }

        private void RollbackChanges()
        {
            var context = ((App)Application.Current).GetDBContext;
            var collection = context.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged);

            foreach (var e in collection)
            {
                switch (e.State)
                {
                    case EntityState.Modified:
                        e.CurrentValues.SetValues(e.OriginalValues);
                        e.State = EntityState.Unchanged;
                        break;

                    case EntityState.Added:
                        e.State = EntityState.Detached;
                        break;

                    case EntityState.Deleted:
                        e.State = EntityState.Unchanged;
                        break;
                }
            }
        }

        private void SQLQuery_TextChanged(object sender, EventArgs e)
        {
            if (!TextSettingModeOn)
                ReportIsChanged = true;
        }

        private string GetStringName(bool capitalized)
        {
            var result = "отчет";

            switch (CurrentType)
            {
                case EditPageType.Report:
                    result = "отчет";
                    break;

                case EditPageType.DataSource:
                    result = "источник";
                    break;

                case EditPageType.Condition:
                    result = "условие";
                    break;

                case EditPageType.Parameter:
                    result = "параметр";
                    break;
            }

            if (capitalized)
                result = $"{result.First().ToString().ToUpper()}{result.Substring(1)}";

            return result;
        }

        private void AddGlobalCondition_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentType != EditPageType.Report)
                return;

            if (GlobalConditions.SelectedItem == null)
            {
                MessageBox.Show("Не выбрана глобальная проверка из списка!");
                return;
            }

            var selectedGlobalCondition = (KeyValuePair<Guid, string>)GlobalConditions.SelectedItem;

            var context = ((App)Application.Current).GetDBContext;

            var existedReportCondition = CurrentReport.ReportConditions.FirstOrDefault(x => x.Report.Reportid.Equals(CurrentReport.Reportid) && x.Condition.Conditionid.Equals(selectedGlobalCondition.Key));

            if (existedReportCondition != null)
            {
                MessageBox.Show("Глобальная проверка уже добавлена!");
                return;
            }

            var condition = context.Conditions.FirstOrDefault(x => x.Isglobal == true && x.Conditionid.Equals(selectedGlobalCondition.Key));

            if (condition == null)
            {
                MessageBox.Show("Не найдена глобальная проверка! Обратитесь к админитратору системы");
                return;
            }

            GlobalConditions.SelectedItem = null;

            var reportCondition = new ReportCondition
            {
                Id = Guid.NewGuid(),
                Report = CurrentReport,
                Condition = condition,
            };

            CurrentReport.ReportConditions.Add(reportCondition);
            ReloadConditions(reportCondition);
        }

        private void AddGlobalParameter_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentType != EditPageType.Report)
                return;

            if (GlobalParameters.SelectedItem == null)
            {
                MessageBox.Show("Не выбран глобальный параметр из списка!");
                return;
            }

            var selectedGlobalParameter = (KeyValuePair<Guid, string>)GlobalParameters.SelectedItem;

            var context = ((App)Application.Current).GetDBContext;

            var existedReportParameter = CurrentReport.ReportParameters.FirstOrDefault(x => x.Report.Reportid.Equals(CurrentReport.Reportid) && x.Parameter.Id.Equals(selectedGlobalParameter.Key));

            if (existedReportParameter != null)
            {
                MessageBox.Show("Глобальный параметр уже добавлен!");
                return;
            }

            var parameter = context.Parameters.Where(x => x.Isglobal == true && x.Id.Equals(selectedGlobalParameter.Key)).FirstOrDefault();

            if (parameter == null)
            {
                MessageBox.Show("Не найден глобальный параметр! Обратитесь к админитратору системы");
                return;
            }

            GlobalParameters.SelectedItem = null;

            var reportParameter = new ReportParameter
            {
                Id = Guid.NewGuid(),
                Report = CurrentReport,
                Parameter = parameter,
            };

            CurrentReport.ReportParameters.Add(reportParameter);
            ReloadRepParams(reportParameter);
        }

        private void GlobalDataSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalDataSources.SelectedItem == null)
                return;

            var selectedDataSourceItem = (KeyValuePair<Guid, string>)GlobalDataSources.SelectedItem;

            var context = ((App)Application.Current).GetDBContext;

            var selectedDataSource = context.DataSources.FirstOrDefault(x => x.Isglobal == true && x.Datasourceid.Equals(selectedDataSourceItem.Key));

            if (selectedDataSource != null)
                ExtractDataEngine.LoadNestedDataSources(selectedDataSource);

            DataSource deleted = null;

            if (CurrentReport.Datasource != null)
                deleted = context.DataSources.FirstOrDefault(x => x.Isglobal != true && x.Datasourceid.Equals(CurrentReport.Datasource.Datasourceid));

            if (deleted != null)
                context.DataSources.Remove(deleted);

            CurrentReport.Datasource = selectedDataSource;
            UpdateVisualData();
            ReportIsChanged = true;
        }

        private void useGlobal_Click(object sender, RoutedEventArgs e)
        {
            CurrentReport.Useglobal = useGlobal.IsChecked;

            if (useGlobal.IsChecked == true)
            {
                if (GlobalDataSources.SelectedItem == null)
                {
                    CurrentReport.Datasource = new DataSource
                    {
                        Datasourceid = Guid.NewGuid(),
                        Datasourcename = "Новый источник данных",
                        Position = 0
                    };

                    ReportIsChanged = true;
                }
                else
                {
                    var selectedDataSourceItem = (KeyValuePair<Guid, string>)GlobalDataSources.SelectedItem;
                    var context = ((App)Application.Current).GetDBContext;
                    var selectedDataSource = context.DataSources.FirstOrDefault(x => x.Isglobal == true && x.Datasourceid.Equals(selectedDataSourceItem.Key));
                    CurrentReport.Datasource = selectedDataSource;
                    ReportIsChanged = true;
                }
            }
            else
            {
                CurrentReport.Datasource = new DataSource()
                {
                    Datasourceid = Guid.NewGuid(),
                    Datasourcename = "Новый источник данных",
                    Position = 0
                };

                ReportIsChanged = true;
            }

            CheckVisibility();
            UpdateVisualData();
        }

        private void UpdateVisualData()
        {
            var dataSources = new List<DataSource> { CurrentReport.Datasource };
            SortDataSourceByPosition(dataSources.FirstOrDefault());
            treeView1.ItemsSource = dataSources;
        }

        private void CheckVisibility()
        {
            var noGlobal = CurrentReport.Useglobal != true;
            DataSourceNameTextBox.IsEnabled = IsUnlocked && noGlobal;
            KeyFieldNameTextBox.IsEnabled = IsUnlocked && noGlobal;
            ForeignKeyFieldNameTextBox.IsEnabled = IsUnlocked && noGlobal;
            OneRow.IsEnabled = IsUnlocked && noGlobal;
            SQLDSQueryEditor.TextArea.IsEnabled = noGlobal;
            useGlobal.IsEnabled = true;
            GlobalDataSources.IsEnabled = !noGlobal;
            AddNestedDSButton.IsEnabled = noGlobal;
            RemoveNestedDSButton.IsEnabled = noGlobal;
            Up.IsEnabled = noGlobal;
            Down.IsEnabled = noGlobal;
        }

        private DataTable RunMainSourceQuery() => Transpose(RunSourceQuery((treeView1.ItemsSource as List<DataSource>).FirstOrDefault()));

        private List<DataTable> RunChildSourceQuery()
        {
            var ds = new List<DataTable>();

            var mainSource = (treeView1.ItemsSource as List<DataSource>).FirstOrDefault();

            if (mainSource == null)
                return ds;

            foreach (var childSource in mainSource.InverseParentDataSource)
                ds.Add(RunSourceQuery(childSource));

            ds.RemoveAll(x => x == null);

            return ds;
        }

        private DataTable RunSourceQuery(DataSource source)
        {
            var connection = ((App)Application.Current).GetConnection.CreateConnection(ServerTextBox.Text, BaseTextComboBox.SelectedValue == null ? null : BaseTextComboBox.SelectedValue.ToString());

            if (source == null)
            {
                MessageBox.Show($"Не найден источник");

                return null;
            }

            var sqlText = source.Dataquery;

            if (string.IsNullOrEmpty(sqlText))
            {
                MessageBox.Show($"Не задан текст запроса");
                return null;
            }

            if (string.IsNullOrEmpty(connection.ServerName))
            {
                MessageBox.Show($"Не заполнен адрес сервера БД");
                return null;
            }

            if (string.IsNullOrEmpty(connection.DatabaseName))
            {
                MessageBox.Show($"Не заполнено имя БД");
                return null;
            }

            try
            {
                if (sqlText.Contains("?") && CurrentReport.Testid == null)
                    throw new Exception("В запросе используется параметр \"?\", но в текущем отчете не задан тестовый идентификатор. Заполните тестовый идентификатор, и попробуйте выполнить запрос.");

                var queryWithParam = Helper.InsertParametrValueInQuery(sqlText, CurrentReport.Testid);

                queryWithParam = queryWithParam.Replace("@userid", $"'{CurrentReport.Testuserid}'");
                queryWithParam = queryWithParam.Replace("@reportcode", $"'{CurrentReport.Reportcode}'");
                queryWithParam = SubstituteParams(queryWithParam);

                var objConnection = ExtractDataEngine.GetConnection(connection);
                var objCommand = ExtractDataEngine.GetCommand(objConnection, queryWithParam);
                var objAdapater = ExtractDataEngine.GetAdapter(objCommand);
                var objDataset = new DataSet();
                objConnection.Open();
                objAdapater.Fill(objDataset);
                objConnection.Close();

                if (objDataset == null || objDataset.Tables == null || objDataset.Tables.Count <= 0 || objDataset.Tables[0].Rows == null || objDataset.Tables[0].Rows.Count <= 0)
                {
                    MessageBox.Show($"Не удалось получить данные для источника '{source.Datasourcename}'");

                    var resQuery = objDataset?.Tables?[0];

                    if (resQuery != null)
                        resQuery.TableName = source.Datasourcename;

                    return resQuery;
                }
                else
                {
                    objDataset.ChangeQuotes();

                    var resQuery = objDataset.Tables[0];
                    resQuery.TableName = source.Datasourcename;
                    return resQuery;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось выполнить запрос \n БД: {connection.DatabaseName}\n Ex:{ex.Message}");
                return null;
            }
        }

        private DataTable Transpose(DataTable table)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn("Variable", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Value", typeof(string)));

            if (table?.Rows.Count <= 0)
                return dataTable;

            for (var i = 0; i < (table?.Columns.Count ?? 0); i++)
            {
                dataTable.Rows.Add(table.Columns[i].ColumnName, table.Rows[0][i].ToString());
            }

            return dataTable;
        }

        private void RunAgreementTool()
        {
            if (IsUnlocked)
                SaveData();

            var agreementTool = new AgreementWindow(CurrentReport.Document, RunMainSourceQuery(), RunChildSourceQuery());

            agreementTool.ShowDialog();

            ChangeDocument(agreementTool.SelectedDocument);
        }

        private void ChangeDocument(Document document)
        {
            var context = ((App)Application.Current).GetDBContext;
            CustomTemplateComboBox.ItemsSource = context.Documents.ToDictionary(x => x.Id, y => y.Name);

            if (document == null)
                return;

            CustomTemplateComboBox.SelectedValue = document.Id;
            CurrentReport.Document = document;
            ReportIsChanged = true;
        }

        private void CustomTemplateChecked(object sender, RoutedEventArgs e) => ChangeCustomTemplateVisibility((sender as CheckBox).IsChecked.GetValueOrDefault());

        private void ChangeCustomTemplateVisibility(bool isCustom)
        {
            TemplatePathLabel.Visibility = isCustom ? Visibility.Collapsed : Visibility.Visible;
            TemplatePathTextBox.Visibility = isCustom ? Visibility.Collapsed : Visibility.Visible;
            SelectDocPath.Visibility = isCustom ? Visibility.Collapsed : Visibility.Visible;

            CustomTemplateLabel.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
            CustomTemplateComboBox.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CustomTemplateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomTemplateComboBox.SelectedItem == null)
                return;

            var selectedTemplateItem = (KeyValuePair<Guid, string>)CustomTemplateComboBox.SelectedItem;

            if (CurrentReport.Document?.Id == selectedTemplateItem.Key)
                return;

            var context = ((App)Application.Current).GetDBContext;

            var selectedDocument = context.Documents.FirstOrDefault(x => x.Id.Equals(selectedTemplateItem.Key));

            CurrentReport.Document = selectedDocument;
            ReportIsChanged = true;
        }

        private readonly string CurrentUser =
#if DEBUG
            @"HQMCDSOFT\Test_DEBUG";
#else
            System.Security.Principal.WindowsIdentity.GetCurrent().Name;
#endif

        private void ChangeLockReport(bool isLocked)
        {
            if (CurrentReport == null || CurrentReport.IsLocked == isLocked || CurrentReport.IsLocked && CurrentReport.LockedBy != CurrentUser)
                return;

            CurrentReport.IsLocked = isLocked;
            CurrentReport.LockedBy = isLocked ? CurrentUser : null;

            ChangeLockDocument(CurrentReport.Document, isLocked);

            var context = ((App)Application.Current).GetDBContext;
            context.SaveChanges();
        }

        private void ChangeLockDocument(Document document, bool isLocked)
        {
            if (document == null || document.IsLocked == isLocked || document.IsLocked && document.LockedBy != CurrentUser)
                return;

            document.IsLocked = isLocked;
            document.LockedBy = isLocked ? CurrentUser : null;
        }

        public bool IsUnlocked => CurrentReport == null || !CurrentReport.IsLocked || CurrentReport.LockedBy == CurrentUser;

        private void LockingForm()
        {
            DoStore.IsEnabled = IsUnlocked;
            RunConditionsCheck.IsEnabled = IsUnlocked;
            DoTest.IsEnabled = IsUnlocked;
            DoExport.IsEnabled = IsUnlocked;

            MainPanel.IsEnabled = IsUnlocked;

            toolBar1.IsEnabled = IsUnlocked;
            DataSourceNameTextBox.IsEnabled = IsUnlocked;
            KeyFieldNameTextBox.IsEnabled = IsUnlocked;
            ForeignKeyFieldNameTextBox.IsEnabled = IsUnlocked;
            OneRow.IsEnabled = IsUnlocked;
            SQLDSQueryEditor.IsReadOnly = !IsUnlocked;

            toolBar2.IsEnabled = IsUnlocked;
            ConditionNameTextBox.IsEnabled = IsUnlocked;
            conditionOperatorComboBox.IsEnabled = IsUnlocked;
            RecordCountTextBox.IsEnabled = IsUnlocked;
            PreconditionBox.IsEnabled = IsUnlocked;
            ConditionOrderTextBox.IsEnabled = IsUnlocked;
            SQLConditionQueryEditor.IsReadOnly = !IsUnlocked;

            toolBarRepParams.IsEnabled = IsUnlocked;
            RepParamNameTextBox.IsEnabled = IsUnlocked;
            RepParamDisplayNameTextBox.IsEnabled = IsUnlocked;
            RepParamDisplayOrderTextBox.IsEnabled = IsUnlocked;
            RepParamDataTypeComboBox.IsEnabled = IsUnlocked;
            RepParamIsNullable.IsEnabled = IsUnlocked;
            RepParamTestValTextBox.IsEnabled = IsUnlocked;
            SQLRepParamQueryEditor.IsReadOnly = !IsUnlocked;

            toolBarFilter.IsEnabled = IsUnlocked;
            FilterValueTextBox.IsEnabled = IsUnlocked;

            if (!IsUnlocked)
                MessageBox.Show($"Данный документ слияния открыт у {CurrentReport.LockedBy} и недоступен для редактирования");
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject rootObject) where T : DependencyObject
        {
            if (rootObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(rootObject); i++)
                {
                    var child = VisualTreeHelper.GetChild(rootObject, i);

                    if (child != null && child is T tempChild)
                        yield return tempChild;

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                        yield return childOfChild;
                }
            }
        }

        private void OpenConditions_Click(object sender, RoutedEventArgs e)
        {
            if (RepParamListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите параметр, чтобы открыть условия", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }
            var connection = CurrentReport != null ? ExtractDataEngine.GetConnection(((App)Application.Current).GetConnection.CreateConnection(CurrentReport.servername, CurrentReport.defaultdatabase)) : null;
            connection.Open();

            using (connection)
            {
                var window = new ParameterConditionEditor(RepParamListBox.SelectedItem as Parameter, connection, CurrentReport?.testid, CurrentReport != null && !string.IsNullOrEmpty(CurrentReport.testuserid) ? Guid.Parse(CurrentReport.testuserid) : Guid.Empty);

                window.ShowDialog();
            }
        }

        private void PinFilter_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalFilter.SelectedItem == null)
            {
                MessageBox.Show("Не выбран глобальный фильтр из списка!");
                return;
            }

            var selectedGlobalFilter = (KeyValuePair<Guid, string>)GlobalFilter.SelectedItem;

            var context = ((App)Application.Current).GetDBContext;

            var existedReportFilter = CurrentReport.ReportFilters.FirstOrDefault(x => x.Report.Reportid.Equals(CurrentReport.Reportid) && x.Filter.Id.Equals(selectedGlobalFilter.Key));

            if (existedReportFilter != null)
            {
                MessageBox.Show("Фильтр уже добавлен!");
                return;
            }

            var filter = context.Filters.Where(x => x.Id.Equals(selectedGlobalFilter.Key)).FirstOrDefault();

            if (filter == null)
            {
                MessageBox.Show("Не найден глобальный фильтр! Обратитесь к админитратору системы");
                return;
            }

            GlobalFilter.SelectedItem = null;

            var reportFilter = new ReportFilter
            {
                Id = Guid.NewGuid(),
                Report = CurrentReport,
                Filter = filter,
            };

            CurrentReport.ReportFilters.Add(reportFilter);
            ReloadReportFilters(reportFilter);
        }

        private void FilterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                FilterSelect(FilterListBox.SelectedItem as Filter);
            }
            catch (ApplicationException ex)
            {
                ShowMessage.Warning(ex.Message);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void FilterSelect(Filter filter)
        {
            if (filter != null)
            {
                FilterReportGrid.DataContext = filter;
                TextSettingModeOn = true;
                SQLFilterQueryEditor.Text = filter.Query;
                TextSettingModeOn = false;
                SQLFilterQueryEditor.Select(0, 0);

                FilterValueTextBox.Text = filter.ReportFilters.FirstOrDefault(x => x.Report.Reportid == CurrentReport.Reportid)?.Value;
            }
        }

        private void ReloadReportFilters(ReportFilter filter = null)
        {
            FilterListBox.ItemsSource = CurrentReport.ReportFilters.Select(x => x.Filter).ToList();
            FilterListBox.DisplayMemberPath = "Name";

            FilterReportGrid.DataContext = null;

            if (filter != null)
                FilterListBox.SelectedItem = filter.Filter;

        }

        private void VisibleFilterField(bool isOpenFilter)
        {
            FilterNameTextBox.IsEnabled = isOpenFilter;
            FilterDisplayNameTextBox.IsEnabled = isOpenFilter;
            FilterOrderTextBox.IsEnabled = isOpenFilter;
            FilterTypeComboBox.IsEnabled = isOpenFilter;
            SQLFilterQueryEditor.IsEnabled = isOpenFilter;

            FilterValueLabel.Visibility = isOpenFilter ? Visibility.Collapsed : Visibility.Visible;
            FilterValueTextBox.Visibility = isOpenFilter ? Visibility.Collapsed : Visibility.Visible;
        }

        private void FilterValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FilterListBox.SelectedItem == null)
                return;

            var filter = FilterListBox.SelectedItem as Filter;

            var reportFilter = filter.ReportFilters.FirstOrDefault(x => x.Report.Reportid == CurrentReport.Reportid);

            if (reportFilter == null)
            {
                MessageBox.Show("Ошибка привязки фильтра. Обратитесь к администратору системы");
            }

            reportFilter.Value = FilterValueTextBox.Text;
        }

        private void OpenVisibleSetting_Click(object sender, RoutedEventArgs e)
        {
            if (FilterListBox.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать фильтр");
                return;
            }

            var window = new FilterVisibleEditor(FilterListBox.SelectedItem as Filter, FilterNameTextBox.IsEnabled);

            window.ShowDialog();
        }

        private void UnpinFilter_Click(object sender, RoutedEventArgs e)
        {
            if (FilterListBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите параметр, чтобы удалить его из отчета", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            if (MessageBox.Show("Удалить параметр?", "Подтверждение операции", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var context = ((App)Application.Current).GetDBContext;
                var filter = (Filter)FilterListBox.SelectedItem;

                var reportFilter = filter.ReportFilters.FirstOrDefault(x => x.Report.Reportid == CurrentReport.Reportid);
                if (reportFilter != null)
                {
                    context.ReportFilters.Remove(reportFilter);
                    CurrentReport.ReportFilters.Remove(reportFilter);
                }

                ReloadReportFilters();
            }
        }

        private void sploadtype_SelectionChanged(object sender, SelectionChangedEventArgs e) => CheckSpFields();

        private void ShareSaveCheckBoxChecked(object sender, RoutedEventArgs e) => CheckSpFields();

        private void CheckSpFields()
        {
            var isSp = ShareSaveCheckBox.IsChecked.GetValueOrDefault();

            SpLoadTypeLabel.Visibility = isSp ? Visibility.Visible : Visibility.Collapsed;
            SpLoadTypeComboBox.Visibility = isSp ? Visibility.Visible : Visibility.Collapsed;

            var isDirectSp = SpLoadTypeComboBox.SelectedValue?.ToString() == Enums.SpLoadEnum.LoadInSp.ToString();

            SpPathFolderNameLabel.Visibility = isSp && isDirectSp ? Visibility.Visible : Visibility.Collapsed;
            SpPathFolderNameTextBox.Visibility = isSp && isDirectSp ? Visibility.Visible : Visibility.Collapsed;
            ChangeNameTypeLabel.Visibility = isSp && isDirectSp ? Visibility.Visible : Visibility.Collapsed;
            ChangeNameTypeComboBox.Visibility = isSp && isDirectSp ? Visibility.Visible : Visibility.Collapsed;

            var isMultiUploader = SpLoadTypeComboBox.SelectedValue?.ToString() == Enums.SpLoadEnum.CreateDocAndLoadSp.ToString();

            GroupLabel.Visibility = isSp && isMultiUploader ? Visibility.Visible : Visibility.Collapsed;
            GroupTextBox.Visibility = isSp && isMultiUploader ? Visibility.Visible : Visibility.Collapsed;
            ShareCodeLabel.Visibility = isSp && isMultiUploader ? Visibility.Visible : Visibility.Collapsed;
            ShareCodeTextBox.Visibility = isSp && isMultiUploader ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SendRequestCheckBoxChecked(object sender, RoutedEventArgs e) => CheckIntegrationFields();

        private void CheckIntegrationFields()
        {
            var isIntegration = SendRequestCheckBox.IsChecked.GetValueOrDefault();

            IntegrationLabel.Visibility = isIntegration ? Visibility.Visible : Visibility.Collapsed;
            IntegrationGrid.Visibility = isIntegration ? Visibility.Visible : Visibility.Collapsed;
            AddIntegration.Visibility = isIntegration ? Visibility.Visible : Visibility.Collapsed;
            RemoveIntegration.Visibility = isIntegration ? Visibility.Visible : Visibility.Collapsed;
        }

        private void InitIntegrationSources()
        {
            if (CurrentType != EditPageType.Report)
                return;

            var context = ((App)Application.Current).GetDBContext;

            IntegrationGrid.ItemsSource = CurrentReport.Integrations.OrderBy(x => x.Order).ToList();

            IntegrationSourceComboBox.ItemsSource = context.DataSources.Where(x => x.Isglobal == true && x.IsIntegrationRequest).ToList();
        }

        private void IntegrationGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (this.IntegrationGrid.SelectedItem != null)
            {
                (sender as DataGrid).RowEditEnding -= IntegrationGrid_RowEditEnding;
                (sender as DataGrid).CommitEdit();
                (sender as DataGrid).Items.Refresh();
                (sender as DataGrid).RowEditEnding += IntegrationGrid_RowEditEnding;
                IntegrationGrid.ItemsSource = CurrentReport.Integrations.OrderBy(x => x.Order).ToList();
            }
        }

        private void AddIntegration_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new Integration
            {
                Id = Guid.NewGuid(),
                Report = CurrentReport
            };

            CurrentReport.Integrations.Add(newItem);
            IntegrationGrid.ItemsSource = CurrentReport.Integrations.OrderBy(x => x.Order).ToList();
        }

        private void RemoveIntegration_Click(object sender, RoutedEventArgs e)
        {
            if (this.IntegrationGrid.SelectedItem == null)
            {
                MessageBox.Show("Необходимо выбрать запись");
                return;
            }

            var context = ((App)Application.Current).GetDBContext;
            var selected = IntegrationGrid.SelectedItem as Integration;
            context.Integrations.Remove(selected);
            CurrentReport.Integrations.Remove(selected);
            IntegrationGrid.ItemsSource = CurrentReport.Integrations.OrderBy(x => x.Order).ToList();
        }

        private void SqlMainReportId_TextChanged(object sender, EventArgs e)
        {
            ReportIsChanged = true;
        }

        private void SqlMainReportCode_TextChanged(object sender, EventArgs e)
        {
            ReportIsChanged = true;
        }

        private void InitDSFields()
        {
            if (!string.IsNullOrEmpty(CurrentReport.SqlMainReportCode))
                SqlMainReportCode.Text = CurrentReport.SqlMainReportCode;

            if (!string.IsNullOrEmpty(CurrentReport.SqlMainRowId))
                SqlMainRowId.Text = CurrentReport.SqlMainRowId;
        }

        private void DSCheckBoxChecked(object sender, RoutedEventArgs e) => CheckDSFields();

        private void CheckDSFields()
        {
            var isDS = DSCheckBox.IsChecked.GetValueOrDefault();

            MainReportCodeLabel.Visibility = isDS ? Visibility.Visible : Visibility.Collapsed;
            SqlMainReportCodeBorder.Visibility = isDS ? Visibility.Visible : Visibility.Collapsed;
            MainReportIdLabel.Visibility = isDS ? Visibility.Visible : Visibility.Collapsed;
            SqlMainRowIdBorder.Visibility = isDS ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetTitle()
        {
            var connectionData = ((App)Application.Current).GetConnection;
            Application.Current.MainWindow.Title = $"[{connectionData.ServerName}.{connectionData.DatabaseName}] {Title}";
        }
    }

}
