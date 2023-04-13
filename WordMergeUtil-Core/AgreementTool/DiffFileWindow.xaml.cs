using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.Model.Revisions;
using Telerik.Windows.Documents.Model;
using WordMergeEngine.Models;
using WordMergeEngine;
using WordMergeUtil_Core.AgreementTool.DataContracts;
using System.IO;
using Telerik.Windows.Documents.FormatProviders.OpenXml.Docx;
using TelerikParagraph = Telerik.Windows.Documents.Model.Paragraph;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for DiffFileWindow.xaml
    /// </summary>
    public partial class DiffFileWindow : Window
    {
        private List<CompareParagraph> _paragraphs = new List<CompareParagraph>();
        private ObservableCollection<CompareParagraph> _filterParagraphs = new ObservableCollection<CompareParagraph>();
        private Dictionary<Guid, byte[]> _compareDocuments = new Dictionary<Guid, byte[]>();
        private Dictionary<Guid, string> _templates = new Dictionary<Guid, string>();

        public bool IsClosed { get; set; }

        private string _fileName { get; set; }

        private bool Initiate()
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".docx",
                Filter = "Документ Word (*.doc)|*.doc|Документ Word 2007-2010 (*.docx)|*.docx|Документ Word с поддержкой макросов(*.docm)|*.docm"
            };

            if (dlg.ShowDialog() != true)
            {
                Close();
                IsClosed = true;
                return false;
            };
            _fileName = dlg.FileName;
            InitializeComponent();

            radRichTextBox1.TrackChangesOptions.Insert.Decoration = RevisionDecoration.Underline;
            radRichTextBox1.TrackChangesOptions.Insert.ColorOptions = new RevisionColor(Colors.Green);

            radRichTextBox1.TrackChangesOptions.Delete.Decoration = RevisionDecoration.Strikethrough;
            radRichTextBox1.TrackChangesOptions.Delete.ColorOptions = new RevisionColor(Colors.Red);

            radRichTextBox1.ChangeAllFieldsDisplayMode(FieldDisplayMode.Code);

            return true;
        }

        public DiffFileWindow(DataModel context)
        {
            if (!Initiate())
                return;

            var selectDocWindow = new SelectDocumentDialog(context);

            selectDocWindow.ShowDialog();

            if (!selectDocWindow.SelectedDocuments.Any())
            {
                Close();
                IsClosed = true;
                return;
            }

            FillDialog(selectDocWindow.SelectedDocuments);
        }

        public DiffFileWindow(List<Document> documents)
        {
            if (!Initiate())
                return;

            FillDialog(documents);
        }

        private void FillDialog(List<Document> documents)
        {
            var inputDoc = File.ReadAllBytes(_fileName);
            DocName.Content = System.IO.Path.GetFileNameWithoutExtension(_fileName);

            _templates.Add(Guid.Empty, "Все");

            foreach (var document in documents)
            {
                ExtractDataEngine.Loader(document);
                FillCompareParagraphs(inputDoc, document);
                FillCompareDocument(inputDoc, document);
                _templates.Add(document.Id, document.Name);
            }

            TemplateValue.ItemsSource = _templates;

            if (documents.Count == 1)
            {
                var doc = documents.First();
                SetCompareDocument(doc.Id);
                TemplateValue.Visibility = Visibility.Hidden;
                TemplateLabelValue.Content = doc.Name;
                TemplateColumn.IsVisible = false;
            }
            else
            {
                TemplateValue.SelectedItem = _templates.First();
            }
        }

        private void FillCompareParagraphs(byte[] inputDoc, Document document)
        {
            var provider = new XamlFormatProvider();
            var input = new DocxFormatProvider().Import(inputDoc);
            var i = 1;

            foreach (var paragraph in document.CurrentVersion.Paragraphs.Where(x => x.Deleted == false).OrderBy(x => x.OrderNo))
            {
                foreach (var version in paragraph.ParagraphContents.Where(x => x.Deleted == false).OrderByDescending(x => x.DefaultVersion))
                {
                    _paragraphs.Add(new CompareParagraph(document, paragraph, version)
                    {
                        Number = i++,
                        Similarity = !string.IsNullOrEmpty(version.Content) ? CalculateSimilarity(input, provider.Import(version.Content)) : 0
                    });
                }
            }
        }

        private void FillCompareDocument(byte[] inputDoc, Document selectedDoc)
        {
            var selectedDocArray = TelerikBuilder.GetCompareDocumentArray(selectedDoc);
            var res = DocBuilder.CompareDocuments("Compare", selectedDocArray, inputDoc);

            _compareDocuments.Add(selectedDoc.Id, res);
        }

        private void SetCompareDocument(Guid? templateId = null)
        {
            _filterParagraphs = new ObservableCollection<CompareParagraph>(_paragraphs.Where(x => templateId == null || x.Document?.Id == templateId));
            RadGridView.ItemsSource = _filterParagraphs;

            radRichTextBox1.Document = new RadDocument();

            if (templateId != null && _compareDocuments.ContainsKey(templateId.Value))
                radRichTextBox1.Document = new DocxFormatProvider().Import(_compareDocuments[templateId.Value]);
        }

        private void TemplateValue_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            var selected = (KeyValuePair<Guid, string>)e.AddedItems[0];
            SetCompareDocument(selected.Key == Guid.Empty ? default(Guid?) : selected.Key);
        }

        private string[] GetResults(RadDocument doc)
        {
            var section = doc.Sections.First();

            var stringBuilder = new StringBuilder();

            FillStringBuilder(section.Blocks, stringBuilder);

            var str = stringBuilder.ToString();

            var items = str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            return items;
        }

        private void FillStringBuilder(Telerik.Windows.Documents.Model.BlockCollection blocks, StringBuilder stringBuilder)
        {
            foreach (var block in blocks.ToArray())
            {
                if (block is TelerikParagraph p)
                {
                    foreach (var item in p.Inlines)
                    {
                        if (item is Telerik.Windows.Documents.Model.Span s)
                        {
                            stringBuilder.Append(Regex.Replace(s.Text, "\\W", ";").ToUpperInvariant());
                        }
                    }
                }
                else if (block is Telerik.Windows.Documents.Model.Table t)
                {
                    foreach (var row in t.Rows)
                        foreach (var cell in row.Cells)
                            FillStringBuilder(cell.Blocks, stringBuilder);
                }
            }
        }

        private double CalculateSimilarity(RadDocument document, RadDocument compareDocument)
        {
            if (compareDocument == null) return 0;

            var p1 = from r in GetResults(document)
                     group r by r into grp
                     select new { key = grp.Key, count = grp.Count() };

            var p2 = from r in GetResults(compareDocument)
                     group r by r into grp
                     select new { key = grp.Key, count = grp.Count() };

            int totalWords = p2.Sum(x => x.count);

            int wordsFound = totalWords;

            foreach (var item in p2)
            {

                var foundItems = p1.Where(x => x.key == item.key);

                if (foundItems.Count() == 0)
                {
                    wordsFound -= item.count;
                }
                else
                {
                    if (item.count != foundItems.First().count)
                    {
                        wordsFound -= Math.Min(item.count, Math.Abs(item.count - foundItems.First().count));
                    }
                }
            }

            var percentage = totalWords == 0 ? 0 : (double)wordsFound / (double)totalWords * 100;

            return percentage;
        }
    }
}
