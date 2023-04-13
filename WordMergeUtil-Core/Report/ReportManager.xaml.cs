using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using WordMergeEngine.Models.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine;
using WordMergeUtil_Core.AgreementTool;
using WordMergeUtil_Core.Assets;

using Report = WordMergeEngine.Models.Report;

namespace WordMergeUtil_Core.Report
{
    public partial class Reports : Page
    {
        public Reports()
        {
            InitializeComponent();

            var connectionData = new ConnectionData(((App)Application.Current).GetDBContext.Database.Connection);
            Application.Current.MainWindow.Title = $"[{connectionData.ServerName}.{connectionData.DatabaseName}] {Title}";
        }

        private void ReloadReportList(Guid? selectedReportId = null)
        {
            var context = ((App)Application.Current).GetDBContext;

            IQueryable<WordMergeEngine.Models.Report> reports;

            if (string.IsNullOrEmpty(SearchTextBox.Text))
                reports = from p in context.Reports orderby p.Reportname select p;
            else
            {
                var searchCriteria = SearchTextBox.Text.ToUpper();
                reports = from p in context.Reports where p.Reportname.ToUpper().Contains(searchCriteria) || p.Reportcode.ToUpper().Contains(searchCriteria) || p.Entityname.ToUpper().Contains(searchCriteria) orderby p.Reportname select p;
            }

            if (reporttype.SelectedIndex != -1 && reporttype.SelectedValue.ToString() != "null")
                reports = reports.Where(i => i.Reporttype == (string)reporttype.SelectedValue);

            if (reportformat.SelectedIndex != -1 && reportformat.SelectedValue.ToString() != "null")
            {
                if ((string)reportformat.SelectedValue == "")
                    reports = reports.Where(i => i.Reportformat == null);
                else
                    reports = reports.Where(i => i.Reportformat == (string)reportformat.SelectedValue);
            }

            ReportsList.ItemsSource = reports.ToList();
            ReportDataGrid.ItemsSource = reports.ToList();

            if (selectedReportId != null && selectedReportId.HasValue)
            {
                var selectedItem = (from r in reports where r.Reportid == selectedReportId.Value select r).FirstOrDefault();
                if (selectedItem != null)
                    ReportDataGrid.SelectedItem = selectedItem;
            }
        }

        private void EditDoc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditDocHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void EditDocHandler()
        {
            if (ReportDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите отчет для редактирования из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NavigationService.Navigate(new EditReport((Report)ReportDataGrid.SelectedItem));
        }

        private void AddDoc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var context = ((App)Application.Current).GetDBContext;

                var newReport = new Report();
                newReport.reportid = Guid.NewGuid();
                newReport.reportname = "Новый документ слияния";
                newReport.createdon = DateTime.Now;
                newReport.createdby = WindowsIdentity.GetCurrent().Name;
                newReport.setdate = true;
                newReport.DataSource = new DataSource() { Datasourceid = Guid.NewGuid(), Datasourcename = "Новый источник данных", Position = 0 };
                context.Report.Add(newReport);
                context.SaveChanges();

                ReloadReportList(newReport.reportid);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ReportsList_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void ReportsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                EditDocHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void RemoveDoc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ReportDataGrid.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Выберите отчет из списка", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Выбранные документы слияния будут безвозвратно удалены. Продолжить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                var context = ((App)Application.Current).GetDBContext;
                var reports = ReportDataGrid.SelectedItems.Cast<Report>();
                var reportPackageIds = context.ReportPackageEntry.Select(x => x.Report.reportid).ToList();

                var isDeleted = false;

                foreach (var report in reports)
                {
                    if (reportPackageIds.Any(x => x.Equals(report.reportid)))
                    {
                        MessageBox.Show($"Документ слияния \"{report.reportname}\" нельзя удалить, так как он входит в комплект документов.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }

                    isDeleted = true;

                    report.DeleteReportCascade(((App)Application.Current).GetDBContext);
                }

                if (!isDeleted)
                    return;

                ReloadReportList();

                MessageBox.Show("Документ был успешно удален", "Завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReloadReportList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ShowBusinessRolesWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new BusinessRolesPage());
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ShowCRMSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new GlobalSettinsPage());
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DoImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReloadReportList(ImportExport.Import());
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DoExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ReportDataGrid.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Необходимо выбрать документ для экспорта");
                    return;
                }

                ImportExport.Export(ReportDataGrid.SelectedItems.Cast<Report>().ToList());
                ReloadReportList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void DoExportAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connection = ((App)Application.Current).GetConnection.CreateConnection(Settings.Default.ServerName, Settings.Default.DatabaseName);
                var newContext = ((App)Application.Current).GetOtherTempDBContext(connection.ServerName, connection.DatabaseName, connection.UserName, connection.Password);

                ImportExport.Export(newContext.Reports.ToList(), newContext);

                ReloadReportList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ShowDocumentPackages_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var docPackages = new ReportPackageWindow();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void ReportDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.MouseDevice.DirectlyOver.GetType() != typeof(DataGridColumnHeader) && e.MouseDevice.DirectlyOver.GetType() != typeof(ScrollViewer))
                    EditDocHandler();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchButton_Click(sender, null);
        }

        private void reporttype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchButton_Click(sender, null);
        }

        private void reportformat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchButton_Click(sender, null);
        }

        private void CopyDoc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ReportDataGrid.SelectedItems == null || ReportDataGrid.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Необходимо выбрать документ для копирования");
                    return;
                }

                var context = ((App)Application.Current).GetDBContext;
                var oldReportsToCopy = ReportDataGrid.SelectedItems.Cast<Report>().ToList();
                var newReports = new List<Report>();

                foreach (var oldReport in oldReportsToCopy)
                {
                    var newReport = new Report();
                    newReport.createdby = WindowsIdentity.GetCurrent().Name;
                    newReport.createdon = DateTime.Now;
                    newReport.defaultdatabase = oldReport.defaultdatabase;
                    newReport.entityname = oldReport.entityname;
                    newReport.IsShow = oldReport.IsShow;
                    newReport.removeemptyregions = oldReport.removeemptyregions;
                    newReport.replacefieldswithstatictext = oldReport.replacefieldswithstatictext;
                    newReport.reportcode = oldReport.reportcode;
                    newReport.reportformat = oldReport.reportformat;
                    newReport.reportid = Guid.NewGuid();
                    newReport.reportname = $"{oldReport.reportname}_копия";
                    newReport.reportpath = oldReport.reportpath;
                    newReport.reporttype = oldReport.reporttype;
                    newReport.securitymanagement = oldReport.securitymanagement;
                    newReport.sequencenumber = oldReport.sequencenumber;
                    newReport.servername = oldReport.servername;
                    newReport.sharepoint_dosave = oldReport.sharepoint_dosave;
                    newReport.sharepoint_groupkey = oldReport.sharepoint_groupkey;
                    newReport.sharepoint_intcode = oldReport.sharepoint_intcode;
                    newReport.sqlqueryfilename = oldReport.sqlqueryfilename;
                    newReport.subjectemail = oldReport.subjectemail;
                    newReport.testid = oldReport.testid;
                    newReport.testuserid = oldReport.testuserid;
                    newReport.DataSource = CopyDataSources(newReport, oldReport);
                    newReport.BusinessRoleReport = CopyBusinessRoles(oldReport.BusinessRoleReport);
                    newReport.ReportParameter = CopyParameters(newReport, oldReport);
                    newReport.ReportPackageEntry = CopyPackageEntry(oldReport.ReportPackageEntry);
                    newReport.SqlQueryUseCondition = oldReport.SqlQueryUseCondition;
                    newReport.isCustomTemplate = oldReport.isCustomTemplate;
                    newReport.Document = oldReport.Document;

                    CopyConditions(newReport, oldReport);

                    foreach (var item in newReport.ReportCondition)
                    {
                        context.ReportConditions.Add(item);

                        if (item.Condition.isglobal != true)
                            context.Conditions.Add(item.Condition);
                    }

                    foreach (var item in newReport.ReportParameter)
                    {
                        context.ReportParameters.Add(item);
                        context.Parameters.Add(item.Parameter);
                    }

                    context.DataSources.Add(newReport.DataSource);
                    context.Reports.Add(newReport);
                    context.SaveChanges();
                    newReports.Add(newReport);
                }

                MessageBox.Show("Копирование завершено успешно", "Операция завершена", MessageBoxButton.OK, MessageBoxImage.Information);

                if (newReports.Count > 0)
                    ReloadReportList(newReports[0].reportid);
                else
                    ReloadReportList();
            }
            catch (Exception ex)
            {
                ShowMessage.ShowErrorMessage(ex);
            }
        }

        private DataSource CopyDataSources(Report newReport, Report oldReport)
        {
            ExtractDataEngine.Loader(oldReport);

            if (oldReport == null || oldReport.DataSource == null || newReport == null)
                return new DataSource() { Datasourceid = Guid.NewGuid(), Datasourcename = "Новый источник данных", Position = 0 };

            var oldToNewMap = new Dictionary<Guid, DataSource>();
            var start = true;
            var oldSources = new List<DataSource>() { oldReport.DataSource };

            DataSource root = null;

            while (oldSources.Count > 0)
            {
                var nextOldRoot = oldSources.FirstOrDefault();

                DataSource nextNewRoot;

                if (start)
                {
                    root = ReportExtension.CreateNewSource(nextOldRoot, oldToNewMap, newReport);
                    oldToNewMap.Add(nextOldRoot.Datasourceid, root);
                    start = false;
                }
                else
                {
                    nextNewRoot = ReportExtension.CreateNewSource(nextOldRoot, oldToNewMap);
                    oldToNewMap.Add(nextOldRoot.Datasourceid, nextNewRoot);
                }

                oldSources.AddRange(nextOldRoot.NestedDataSources);
                oldSources.RemoveAt(0);
            }

            return root;
        }

        private ObservableCollection<ReportPackageEntry> CopyPackageEntry(ICollection<ReportPackageEntry> entityCollection)
        {
            return new ObservableCollection<ReportPackageEntry>();
        }

        private ObservableCollection<BusinessRoleReport> CopyBusinessRoles(ICollection<BusinessRoleReport> entityCollection)
        {
            return new ObservableCollection<BusinessRoleReport>();
        }

        private ObservableCollection<ReportParameter> CopyParameters(Report newReport, Report oldReport)
        {
            var res = new ObservableCollection<ReportParameter>();

            if (newReport == null || oldReport == null || oldReport.ReportParameter == null || oldReport.ReportParameter.Count <= 0)
                return res;

            foreach (var reportParameter in oldReport.ReportParameter)
            {
                var oldParam = reportParameter.Parameter;

                var newParam = new Parameter();
                newParam.Datatype = oldParam.datatype;
                newParam.Displayname = oldParam.displayname;
                newParam.Displayorder = oldParam.displayorder;
                newParam.Errormessage = oldParam.errormessage;
                newParam.Id = Guid.NewGuid();
                newParam.Name = oldParam.name;
                newParam.Nullable = oldParam.nullable;
                newParam.Query = oldParam.query;

                var newReportParameter = new ReportParameter();
                newReportParameter.Id = Guid.NewGuid();
                newReportParameter.Report = newReport;
                newReportParameter.Parameter = newParam;

                newParam.ReportParameters.Add(newReportParameter);
                newParam.Isglobal = false;
                newParam.Createdon = DateTime.Now;
                newParam.Modifiedon = DateTime.Now;
                newParam.Createdby = WindowsIdentity.GetCurrent().Name;
                newParam.Modifiedby = WindowsIdentity.GetCurrent().Name;
                newParam.Testval = oldParam.testval;
                res.Add(newReportParameter);
            }

            return res;
        }

        private void CopyConditions(Report newReport, Report oldReport)
        {
            if (oldReport == null || oldReport.ReportCondition == null || oldReport.ReportCondition.Count <= 0 || newReport == null)
                return;

            var context = ((App)Application.Current).GetDBContext;

            foreach (var item in oldReport.ReportCondition)
            {
                var oldCondition = item.Condition;

                var newrp = new ReportCondition
                {
                    Id = Guid.NewGuid(),
                    Report = newReport
                };

                if (oldCondition.isglobal == true)
                {
                    newrp.Condition = oldCondition;
                    oldCondition.ReportCondition.Add(newrp);
                }
                else
                {
                    var newCondition = new WordMergeEngine.Models.Condition();
                    newCondition.Conditionid = Guid.NewGuid();
                    newCondition.Conditionname = oldCondition.conditionname;
                    newCondition.Conditionoperator = oldCondition.conditionoperator;
                    newCondition.Conditiontype = oldCondition.conditiontype;
                    newCondition.Dataquery = oldCondition.dataquery;
                    newCondition.Errormessage = oldCondition.errormessage;
                    newCondition.Precondition = oldCondition.precondition;
                    newCondition.Recordcount = oldCondition.recordcount;

                    newrp.Condition = newCondition;
                    newCondition.ReportConditions.Add(newrp);
                }

                newReport.ReportCondition.Add(newrp);
            }
        }

        private static void GetDataSources(List<DataSource> sources, ICollection<DataSource> dataSources)
        {
            foreach (var dataSource in dataSources)
            {
                sources.Add(dataSource);

                if (dataSource.NestedDataSources.Count > 0)
                    GetDataSources(sources, dataSource.NestedDataSources);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ReloadReportList();
        }

        private void DoGlobalParagraph_Click(object sender, RoutedEventArgs e)
        {
            var window = new GlobalParagraphWindow();

            window.ShowDialog();
        }

        private void DoTagCloud_Click(object sender, RoutedEventArgs e)
        {
            var window = new TagCloudWindow();

            window.ShowDialog();
        }

        private void DoCompareDocument_Click(object sender, RoutedEventArgs e)
        {
            var context = ((App)Application.Current).GetDBContext;
            var window = new DiffFileWindow(context);

            if (!window.IsClosed)
                window.ShowDialog();
        }
    }
}
