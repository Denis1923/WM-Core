using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WordMergeEngine.Models;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for SelectDocumentDialog.xaml
    /// </summary>
    public partial class SelectDocumentDialog : Window
    {
        public List<Document> SelectedDocuments { get; set; } = new List<Document>();

        public SelectDocumentDialog(DataModel context)
        {
            InitializeComponent();
            DocList.ItemsSource = context.Documents.OrderBy(x => x.Name).ToList();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocuments = DocList.SelectedItems.Cast<Document>().ToList();
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
