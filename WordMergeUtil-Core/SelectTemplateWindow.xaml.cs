using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WordMergeEngine.Models;
using WordMergeUtil_Core.Assets;

namespace WordMergeUtil_Core
{
    public partial class SelectTemplateWindow : Window
    {
        public List<ReportItem> SelectedReports { get; set; } = new List<ReportItem>();


        public SelectTemplateWindow(List<WordMergeEngine.Models.Report> reports, List<Document> documents)
        {
            foreach (var report in documents.SelectMany(d => d.Reports))
            {
                SelectedReports.Add(new ReportItem(report.Reportid, $"{report.Reportname}{(documents.Count == 1 ? "" : $" (шаблон {report.Document.Name})")}", reports.Contains(report)));
            }

            InitializeComponent();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
