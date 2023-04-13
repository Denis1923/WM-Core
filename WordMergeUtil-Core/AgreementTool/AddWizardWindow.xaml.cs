using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Flow.Model;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.Proofing;
using WordMergeEngine.Models;
using WordMergeUtil_Core.AgreementTool.DataContracts;
using WordMergeUtil_Core.Assets;
using Paragraph = WordMergeEngine.Models.Paragraph;
using TelerikParagraph = Telerik.Windows.Documents.Model.Paragraph;

namespace WordMergeUtil_Core.AgreementTool
{
    public partial class AddWizardWindow : Window
    {
        private ObservableCollection<CompareParagraph> _paragraphs;

        DataModel _context;

        private Document _document { get; set; }

        private CompareParagraph SelectedParagraph { get; set; }

        public bool IsParagraphChanged { get; set; }

        public bool IsVersionChanged { get; set; }

        private SaveHelper saveHelper { get; set; }

        public AddWizardWindow(Document document, ObservableCollection<Paragraph> aList, DataModel context)
        {
            InitializeComponent();

            var dictionary = new RadDictionary();

            using (var ms = new MemoryStream(Properties.Resources.ru_RU))
                dictionary.Load(ms);

            ((DocumentSpellChecker)RadRichTextBox.SpellChecker).AddDictionary(dictionary, CultureInfo.InvariantCulture);

            _context = context;
            _document = document;

            _paragraphs = new ObservableCollection<CompareParagraph>();
            var i = 1;
            aList.Where(x => x.Deleted == false).OrderBy(x => x.OrderNo).SelectMany(p => p.ParagraphContents.Where(x => x.Deleted == false).Select(c => new CompareParagraph(p, c)
            {
                Number = i++

            })).ToList().ForEach(_paragraphs.Add);

            RadGridView.ItemsSource = _paragraphs;

            saveHelper = new SaveHelper(context);
        }

        public void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ((DocumentSpellChecker)RadRichTextBox.SpellChecker).RemoveDictionary(CultureInfo.InvariantCulture);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if ((IsParagraphChanged || IsVersionChanged) && saveHelper.CheckSavingBeforeAction())
                e.Cancel = true;
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

        private void CalculateSimilarity(CompareParagraph compareParagraph, RadDocument document)
        {
            if (string.IsNullOrEmpty(compareParagraph.Version.Content)) return;

            var provider = new XamlFormatProvider();

            var compareDocument = provider.Import(compareParagraph.Version.Content);

            var p1 = from r in GetResults(document)
                     group r by r into grp
                     select new { key = grp.Key, count = grp.Count() };

            var p2 = from r in GetResults(compareDocument)
                     group r by r into grp
                     select new { key = grp.Key, count = grp.Count() };

            int totalWords = p1.Sum(x => x.count);

            int wordsFound = totalWords;

            foreach (var item in p1)
            {

                var foundItems = p2.Where(x => x.key == item.key);

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

            var percentage1 = totalWords == 0 ? 0 : (double)wordsFound / (double)totalWords;
            totalWords = p2.Sum(x => x.count);
            wordsFound = totalWords;

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

            var percentage2 = totalWords == 0 ? 0 : (double)wordsFound / (double)totalWords;

            var percentage = (percentage1 + percentage2) / 2 * 100;

            compareParagraph.Similarity = percentage;
        }

        private void FindComparingButton_Click(object sender, RoutedEventArgs e)
        {
            StartIndicator("Идёт подсчёт коэффициэнта сравнения...");

            var doc = RadRichTextBox.Document;

            foreach (var dbParagraph in _paragraphs)
            {
                CalculateSimilarity(dbParagraph, doc);
            }

            RadGridView.Items.Refresh();

            FinishIndicator();
        }

        private void AddNewParagraphButton_Click(object sender, RoutedEventArgs e)
        {
            if (_document == null)
            {
                MessageBox.Show("Нет документа для добавления");
                return;
            }

            StartIndicator("Добавляем новый параграф...");

            var provider = new XamlFormatProvider();

            var newParagraph = new Paragraph
            {
                Id = Guid.NewGuid(),
                Name = "Новый параграф",
                DocumentContent = _document.CurrentVersion,
                OrderNo = (SelectedParagraph?.Paragraph.OrderNo ?? 0) + 1,
                Deleted = false,
                Numerable = true
            };

            _document.Paragraph.Add(newParagraph);
            _context.Paragraphs.Add(newParagraph);

            var hpc = new ParagraphContent
            {
                Id = Guid.NewGuid(),
                Name = "Версия по умолчанию",
                Paragraph = newParagraph,
                DefaultVersion = true,
                Deleted = false,
                PassConditions = true,
                Content = provider.Export(RadRichTextBox.Document)
            };

            newParagraph.ParagraphContents.Add(hpc);
            _context.ParagraphContents.Add(hpc);

            _paragraphs.Insert((SelectedParagraph?.Paragraph.OrderNo ?? 0) + 1, new CompareParagraph(newParagraph, hpc));

            foreach (var p in _paragraphs)
            {
                p.Number = _paragraphs.IndexOf(p) + 1;
                p.Paragraph.OrderNo = _paragraphs.IndexOf(p) + 1;
            }

            IsParagraphChanged = true;
            FinishIndicator("Параграф успешно добавлен");
        }

        private void AddNewVersionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            StartIndicator("Добавляем новую версию...");

            var provider = new XamlFormatProvider();
            var hpc = new ParagraphContent
            {
                Id = Guid.NewGuid(),
                Name = "Новая версия",
                Paragraph = SelectedParagraph.Paragraph,
                DefaultVersion = false,
                Deleted = false,
                PassConditions = true,
                Content = provider.Export(RadRichTextBox.Document)
            };

            SelectedParagraph.Paragraph.ParagraphContents.Add(hpc);
            _context.ParagraphContents.Add(hpc);

            _paragraphs.Insert(1, new CompareParagraph(SelectedParagraph.Paragraph, hpc));

            foreach (var p in _paragraphs)
            {
                p.Number = _paragraphs.IndexOf(p) + 1;
            }

            IsVersionChanged = true;
            FinishIndicator("Версия успешно добавлена");
        }

        private void ChangeVersionButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            StartIndicator("Изменяем выбранную версию...");

            var provider = new XamlFormatProvider();
            SelectedParagraph.Version.Content = provider.Export(RadRichTextBox.Document);

            IsVersionChanged = true;
            FinishIndicator("Версия успешно изменена");
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            StartIndicator("Идёт сравнение...");

            var provider = new XamlFormatProvider();

            var inputText = RadRichTextBox.Document;
            var contentText = !string.IsNullOrEmpty(SelectedParagraph.Version.Content) ? provider.Import(SelectedParagraph.Version.Content) : new RadDocument();

            var diffWindow = new DiffWindow(inputText, contentText, $"Введенное → {SelectedParagraph.Paragraph.Name} - {SelectedParagraph.Version.Name}");
            diffWindow.ShowDialog();

            FinishIndicator();
        }

        private void ContentView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var grid = sender as RadGridView;
            SelectedParagraph = grid.SelectedItem as CompareParagraph;
        }

        private void StartIndicator(string message)
        {
            BusyIndicator.BusyContent = message;
            BusyIndicator.IsBusy = true;
            ButtonPanel.IsEnabled = false;
        }

        private void FinishIndicator(string message = null)
        {
            BusyIndicator.IsBusy = false;
            ButtonPanel.IsEnabled = true;

            if (!string.IsNullOrEmpty(message))
                MessageBox.Show(message);
        }
    }
}
