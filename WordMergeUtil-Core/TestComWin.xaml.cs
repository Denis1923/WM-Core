using DocumentsPackage;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using WordMergeEngine;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core
{
    public partial class TestComWin : Window
    {
        public TestComWin()
        {
            InitializeComponent();

            var ctx = ((App)Application.Current).GetDBContext;

            cbxPackades.ItemsSource = (from p in ctx.ReportPackages orderby p.Name select p).ToList();
            cbxPackades.SelectedValuePath = "Name";
            cbxPackades.DisplayMemberPath = "DisplayName";
        }

        private void cmdOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtResult.Text = string.Empty;
                txtErrors.Text = string.Empty;

                if (string.IsNullOrEmpty(txtGUID.Text))
                {
                    MessageBox.Show("Не указан идентификатор объекта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var etn = (string)cbxPackades.SelectedValue;
                var id = txtGUID.Text;

                var context = new CallingContext() { PackageName = etn, RecordId = id };

                var ctx = ((App)Application.Current).GetDBContext;
                var connection = ((App)Application.Current).GetConnection;

                var pkgBuilder = new DocPackageBuilder(ctx, connection, context);
                var templates = pkgBuilder.GetTemplateList(context);

                var reportList = new StringBuilder();

                foreach (var template in templates)
                    reportList.Append($"{template}\r\n");

                txtResult.Text = reportList.ToString();
                txtErrors.Text = pkgBuilder.MessagesToString(false);

                var message = string.Empty;

                pkgBuilder.CheckDocuments().ForEach(i => message += $"{i}\n");

                if (message != string.Empty)
                    throw new ApplicationException($"Не все документы прошли проверку:\n{message}");
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

        private void cmdBuild_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtResult.Text = string.Empty;
                txtErrors.Text = string.Empty;

                if (string.IsNullOrEmpty(txtGUID.Text))
                {
                    MessageBox.Show("Не указан идентификатор объекта!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var etn = (string)cbxPackades.SelectedValue;
                var id = txtGUID.Text;

                var context = new CallingContext() { PackageName = etn, RecordId = id };

                var ctx = ((App)Application.Current).GetDBContext;
                var connection = ((App)Application.Current).GetConnection;

                DocPackageBuilder pkgBuilder;

                var packagePrintFormat = PackagePrintFormat.zip;
                if (xps.IsChecked.Value)
                    packagePrintFormat = PackagePrintFormat.xps;

                if (doc.IsChecked.Value)
                    packagePrintFormat = PackagePrintFormat.doc;

                if (docx.IsChecked.Value)
                    packagePrintFormat = PackagePrintFormat.docx;

                if (isPdf.IsChecked.Value)
                    packagePrintFormat = PackagePrintFormat.pdf;

                var testParams = ctx.ReportPackages.FirstOrDefault(rp => rp.Name == etn).ReportPackageEntries.SelectMany(entry => entry.ReportReport.ReportParameters).ToDictionary(param => param.Parameter.Name, param => param.Parameter.Testval);
                if (!string.IsNullOrEmpty(txtUserId.Text))
                    testParams.Add("userid", txtUserId.Text);

                if (!testParams.Any())
                    pkgBuilder = new DocPackageBuilder(ctx, connection, context, packagePrintFormat);
                else
                    pkgBuilder = new DocPackageBuilder(ctx, connection, context, testParams, packagePrintFormat);

                pkgBuilder.GetTemplateList(null);
                pkgBuilder.CheckDocuments();

                var errors = pkgBuilder.MessagesToString(true);

                if (!string.IsNullOrEmpty(errors))
                {
                    ShowMessage.Warning(errors);
                    return;
                }

                var result = pkgBuilder.PrintPackage();

                var reportList = new StringBuilder();

                foreach (var template in pkgBuilder.Templates)
                    reportList.Append($"{template}\r\n");

                txtResult.Text = reportList.ToString();
                txtErrors.Text = pkgBuilder.MessagesToString(true);

                var fullName = $"{System.IO.Path.GetTempPath()}{result.FileName}";

                using (var fs = new FileStream(fullName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    fs.Write(result.Data, 0, result.Data.Length);
                    fs.Close();
                }

                Process.Start(fullName);
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
    }
}
