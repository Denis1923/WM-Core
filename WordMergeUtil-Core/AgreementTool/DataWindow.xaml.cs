using System.Data;
using System.Windows;
using WordMergeEngine.Helpers;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataWindow : Window
    {
        public DataTable ReportVariables { get; set; }
        public DataTable DocumentVariables { get; set; }

        public DataWindow(DataTable reportVariables, DataTable documentVariables)
        {
            ReportVariables = reportVariables;
            DocumentVariables = documentVariables;
            InitializeComponent();

            if (ReportVariables != null && ReportVariables.Rows.Count > 0)
                ReportView.ItemsSource = ReportVariables.DefaultView;
            else
                ReportPanel.Visibility = Visibility.Collapsed;

            DocumentView.ItemsSource = DocumentVariables.DefaultView;
            LogHelper.Log(GetType().Name);
        }
    }
}
