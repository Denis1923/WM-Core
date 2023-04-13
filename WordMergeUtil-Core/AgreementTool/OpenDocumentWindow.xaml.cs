using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine;
using WordMergeUtil_Core.Assets;
using Paragraph = WordMergeEngine.Models.Paragraph;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for OpenDocumentWindow.xaml
    /// </summary>
    public partial class OpenDocumentWindow : Window
    {
        DataModel dbContext;

        public List<Document> Documents { get; set; }
        public Document SelectedDocument { get; set; }

        public List<Guid> DeletedDocuments { get; set; } = new List<Guid>();

        public List<Guid> ImportIds { get; set; } = new List<Guid>();

        public OpenDocumentWindow(DataModel aContext)
        {
            dbContext = aContext;

            this.DataContext = this;
            InitializeComponent();

            ReloadDocuments();
            LogHelper.Log(GetType().Name);
        }

        private void ReloadDocuments()
        {
            Documents = dbContext.Documents.OrderBy(x => x.Name).ToList();
            DocList.ItemsSource = Documents;
            BusyIndicator.IsBusy = false;
            ButtonPanel.IsEnabled = true;
        }

        private void SelectDocument_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private async void CopyDocument_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null)
            {
                MessageBox.Show("Не выбран документ для копирования");
                return;
            }

            StartIndicator("Идет копирование шаблона...");

            await Task.Factory.StartNew(() =>
            {
                ExtractDataEngine.Loader(SelectedDocument);

                var newDocument = new Document
                {
                    Id = Guid.NewGuid(),
                    Name = $"{SelectedDocument.Name}_копия",
                };

                dbContext.Documents.Add(newDocument);
                dbContext.SaveChanges();

                var newDocumentContent = new DocumentContent
                {
                    Id = Guid.NewGuid(),
                    Data = SelectedDocument.Data,
                    Template = SelectedDocument.Template,
                    Document = newDocument,
                    CreatedBy = WindowsIdentity.GetCurrent().Name,
                    CreatedOn = DateTime.Now,
                    DefaultVersion = true,
                    Version = 1
                };

                dbContext.DocumentContents.Add(newDocumentContent);
                dbContext.SaveChanges();

                foreach (var paragraph in SelectedDocument.Paragraph.Where(x => !x.Deleted))
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

                    newDocument.Paragraph.Add(newParagraph);

                    foreach (var content in paragraph.ParagraphContents.Where(x => !x.Deleted))
                    {
                        var newContent = new ParagraphContent
                        {
                            Id = Guid.NewGuid(),
                            ActiveFrom = content.ActiveFrom,
                            ActiveTill = content.ActiveTill,
                            Condition = content.Condition,
                            Deleted = content.Deleted,
                            Name = content.Name,
                            PassConditions = content.PassConditions,
                            Tooltip = content.Tooltip,
                            Approved = content.Approved,
                            CreatedOn = content.CreatedOn,
                            Content = content.Content,
                            DefaultVersion = content.DefaultVersion,
                            DiffSource = content.DiffSource,
                            DiffTarget = content.DiffTarget
                        };

                        newParagraph.ParagraphContents.Add(newContent);
                    }
                }

                dbContext.SaveChanges();
            });

            ReloadDocuments();
        }

        private async void ImportDocuments_Click(object sender, RoutedEventArgs e)
        {
            StartIndicator("Идет импорт шаблонов...");

            ImportIds.AddRange(await Task.Factory.StartNew(() => ImportExport.ImportDocuments()));

            ReloadDocuments();
        }

        private async void ExportDocuments_Click(object sender, RoutedEventArgs e)
        {
            StartIndicator("Идет экспорт шаблонов...");

            await Task.Factory.StartNew(() => ImportExport.Export(Documents));

            ReloadDocuments();
        }

        private async void ExportDocument_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument != null)
            {
                StartIndicator("Идет экспорт шаблона...");

                await Task.Factory.StartNew(() => ImportExport.Export(SelectedDocument));

                ReloadDocuments();
            }
            else
                MessageBox.Show("Документ не выбран");
        }

        private async void DeleteDocument_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument != null)
            {
                StartIndicator("Идет удаление шаблона...");

                var res = MessageBox.Show($"{(SelectedDocument.Reports.Any() ? $"С этим шаблоном связаны следующие документы слияния: {string.Join(", ", SelectedDocument.Reports.Select(x => x.Reportname))}." : string.Empty)}Вы уверены, что хотите удалить шаблон {SelectedDocument.Name}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (res == MessageBoxResult.Yes)
                {
                    DeletedDocuments.Add(SelectedDocument.Id);
                    await Task.Factory.StartNew(() => SelectedDocument.DeleteDocumentCascade(dbContext));
                }

                ReloadDocuments();
            }
            else
                MessageBox.Show("Документ не выбран");
        }

        private void StartIndicator(string message)
        {
            BusyIndicator.BusyContent = message;
            BusyIndicator.IsBusy = true;
            ButtonPanel.IsEnabled = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
