using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeUtil_Core.AgreementTool.DataContracts;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for TagCloudWindow.xaml
    /// </summary>
    public partial class TagCloudWindow : Window
    {
        private DataModel Context { get; set; }

        private List<TagItem> Tags { get; set; }

        private TagParagraphItem SelectedParagraph { get; set; }

        private ParagraphContent SelectedParagraphContent { get; set; }

        public TagCloudWindow()
        {
            Context = ((App)Application.Current).GetDBContext;

            var paragraphs = Context.Paragraphs.Include("DocumentContent").Include("DocumentContent.Document").Include("ParentParagraph").Include("ParentParagraph.ParagraphContent").Include("ParagraphContent").Where(x => !string.IsNullOrEmpty(x.Tag)).ToList();
            var groupParagraphs = paragraphs.GroupBy(x => x.Tag).OrderBy(x => x.Key).ToList();

            Tags = groupParagraphs.Select(x => new TagItem
            {
                Tag = x.Key,
                ParagraphItems = x.Select((y, i) => new TagParagraphItem
                {
                    Number = i + 1,
                    ParagraphName = $"{y.RelativeNo} {y.Name}",
                    DocumentName = y.DocumentContent.Document.Name,
                    Document = y.DocumentContent.Document,
                    Contents = y.ParentParagraph != null ? y.ParentParagraph.ParagraphContents.ToList() : y.ParagraphContents.ToList()
                }).ToList()
            }).ToList();

            InitializeComponent();

            TagList.ItemsSource = Tags;

            LogHelper.Log(GetType().Name);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = (sender as TextBox).Text;

            TagList.ItemsSource = Tags.Where(x => string.IsNullOrEmpty(text) || x.Tag.StartsWith(text)).ToList();
        }

        private void TagList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = (sender as RadListBox).SelectedItem as TagItem;

            TagView.ItemsSource = selected.ParagraphItems;
        }

        private void ContentView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            var grid = sender as RadGridView;
            SelectedParagraph = grid.DataContext as TagParagraphItem;
            var selected = grid.SelectedItem as ParagraphContent;
            SelectedParagraphContent = selected;
            var provider = new XamlFormatProvider();
            var exportProvider = new Telerik.Windows.Documents.FormatProviders.Html.HtmlFormatProvider();

            ContentText.NavigateToString(exportProvider.Export(provider.Import(selected.Content)));
        }

        private void DoOpenTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph == null)
            {
                MessageBox.Show("Выберите запись");
                return;
            }

            var doc = SelectedParagraph.Document;

            var window = new AgreementWindow(doc, null, new List<DataTable>());

            window.ShowDialog();
        }

        private void ChangeVersion_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraphContent == null)
            {
                MessageBox.Show("Выберите версию");
                return;
            }

            if (SelectedParagraphContent.Paragraph.IsGlobal)
            {
                MessageBox.Show("Версию глобального параграфа нельзя изменить");
                return;
            }

            var provider = new XamlFormatProvider();
            var version = provider.Import(SelectedParagraphContent.Content);

            var window = new LightPreviewWindow(version)
            {
                Title = "Редактирование версии"
            };

            window.ShowDialog();

            var newVersion = provider.Export(version);
            if (SelectedParagraphContent.Content != newVersion)
            {
                SelectedParagraphContent.Content = newVersion;
                Context.SaveChanges();
            }
        }
    }
}
