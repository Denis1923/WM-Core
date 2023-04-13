using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Documents.Model.Revisions;
using Telerik.Windows.Documents.Model;
using WordMergeEngine;
using Telerik.Windows.Documents.FormatProviders.OpenXml.Docx;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for DiffWindow.xaml
    /// </summary>
    public partial class DiffWindow : Window
    {
        public DiffWindow(RadDocument oldDoc, RadDocument newDoc, string name)
        {
            InitializeComponent();

            CaptionLabel.Content = name;

            var provider = new DocxFormatProvider();

            var res = DocBuilder.CompareDocuments(name, provider.Export(oldDoc), provider.Export(newDoc));

            radRichTextBox1.Document = provider.Import(res);

            radRichTextBox1.TrackChangesOptions.Insert.Decoration = RevisionDecoration.Underline;
            radRichTextBox1.TrackChangesOptions.Insert.ColorOptions = new RevisionColor(Colors.Green);

            radRichTextBox1.TrackChangesOptions.Delete.Decoration = RevisionDecoration.Strikethrough;
            radRichTextBox1.TrackChangesOptions.Delete.ColorOptions = new RevisionColor(Colors.Red);

            radRichTextBox1.ChangeAllFieldsDisplayMode(FieldDisplayMode.Code);
        }
    }
}
