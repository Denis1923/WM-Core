using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine;
using Paragraph = WordMergeEngine.Models.Paragraph;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for VersionWindow.xaml
    /// </summary>
    public partial class VersionWindow : Window
    {
        private Document SelectedDocument { get; set; }

        private DataModel context { get; set; }

        private ObservableCollection<DocumentContent> DocumentContents
        {
            get
            {
                if (SelectedDocument == null)
                    return new ObservableCollection<DocumentContent>();

                return new ObservableCollection<DocumentContent>(SelectedDocument.DocumentContents.OrderByDescending(x => x.Version));
            }
        }

        private readonly string CurrentUser =
#if DEBUG
            @"HQMCDSOFT\Test_DEBUG";
#else
            System.Security.Principal.WindowsIdentity.GetCurrent().Name;
#endif

        public bool IsUnlocked => SelectedDocument == null || !SelectedDocument.IsLocked || SelectedDocument.LockedBy == CurrentUser;

        public VersionWindow(DataModel context, Document document)
        {
            SelectedDocument = document;
            this.context = context;
            InitializeComponent();

            TableLable.Content = $"{TableLable.Content} {document.Name}";
            VersionView.ItemsSource = DocumentContents;

            LogHelper.Log(GetType().Name);
        }

        private void ApplyVersion_Click(object sender, RoutedEventArgs e)
        {
            if (VersionView.SelectedItems.Count != 1)
            {
                MessageBox.Show("Нужно выбрать одну версию");
                return;
            }

            var selectedContent = VersionView.SelectedItem as DocumentContent;

            if (selectedContent.DefaultVersion)
            {
                MessageBox.Show("Текущая версия уже является текущей");
                return;
            }

            var currentVersion = SelectedDocument.DocumentContents.FirstOrDefault(x => x.DefaultVersion);

            var newVersion = new DocumentContent
            {
                Id = Guid.NewGuid(),
                Data = selectedContent.Data,
                Template = selectedContent.Template,
                Document = selectedContent.Document,
                CreatedOn = DateTime.Now,
                CreatedBy = CurrentUser,
                DefaultVersion = true,
                Version = currentVersion.Version + 1,
            };

            context.DocumentContents.Add(newVersion);
            currentVersion.DefaultVersion = false;

            foreach (var paragraph in selectedContent.Paragraphs)
            {
                var newParagraph = new Paragraph
                {
                    Id = Guid.NewGuid(),
                    ActiveFrom = paragraph.ActiveFrom,
                    ActiveTill = paragraph.ActiveTill,
                    Condition = paragraph.Condition,
                    Deleted = paragraph.Deleted,
                    Errors = paragraph.Errors,
                    Level = paragraph.Level,
                    Name = paragraph.Name,
                    NewPage = paragraph.NewPage,
                    Numerable = paragraph.Numerable,
                    OrderNo = paragraph.OrderNo,
                    PassConditions = paragraph.PassConditions,
                    ReferenceName = paragraph.ReferenceName,
                    Tooltip = paragraph.Tooltip,
                    IsFixNumeration = paragraph.IsFixNumeration,
                    ParentParagraph = paragraph.ParentParagraph,
                    Tag = paragraph.Tag
                };

                newVersion.Paragraphs.Add(newParagraph);

                foreach (var paragraphContent in paragraph.ParagraphContents)
                {
                    var newContent = new ParagraphContent
                    {
                        Id = Guid.NewGuid(),
                        ActiveFrom = paragraphContent.ActiveFrom,
                        ActiveTill = paragraphContent.ActiveTill,
                        Condition = paragraphContent.Condition,
                        Deleted = paragraphContent.Deleted,
                        Name = paragraphContent.Name,
                        PassConditions = paragraphContent.PassConditions,
                        Tooltip = paragraphContent.Tooltip,
                        Approved = paragraphContent.Approved,
                        CreatedOn = paragraphContent.CreatedOn,
                        Content = paragraphContent.Content,
                        DefaultVersion = paragraphContent.DefaultVersion,
                        DiffSource = paragraphContent.DiffSource,
                        DiffTarget = paragraphContent.DiffTarget
                    };

                    newParagraph.ParagraphContents.Add(newContent);
                }
            }

            context.SaveChanges();

            VersionView.ItemsSource = DocumentContents;
        }

        private void CompareVersion_Click(object sender, RoutedEventArgs e)
        {
            if (VersionView.SelectedItems.Count != 2)
            {
                MessageBox.Show("Нужно выбрать 2 версии для сравнения");
                return;
            }

            var firstVersion = VersionView.SelectedItems.First() as DocumentContent;
            var firstDoc = TelerikBuilder.GetCompareDocument(firstVersion);
            var secondVersion = VersionView.SelectedItems.Last() as DocumentContent;
            var secondDoc = TelerikBuilder.GetCompareDocument(secondVersion);

            var diffWindow = new DiffWindow(firstDoc, secondDoc, $"{firstVersion.VersionName} → {secondVersion.VersionName}");
            diffWindow.ShowDialog();
        }
    }
}
