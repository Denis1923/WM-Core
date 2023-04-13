using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Proofing;
using WordMergeEngine.Assets.Enums;
using WordMergeEngine.Models;
using WordMergeEngine;
using Paragraph = WordMergeEngine.Models.Paragraph;
using WordMergeUtil.AgreementTool.Localizations;
using WordMergeEngine.Helpers;
using WordMergeEngine.Assets;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for GlobalParagraphWindow.xaml
    /// </summary>
    public partial class GlobalParagraphWindow : Window, INotifyPropertyChanged
    {

        public ObservableCollection<Paragraph> Paragraphs { get; set; } = new ObservableCollection<Paragraph>();

        private Paragraph _selectedParagraph;
        public Paragraph SelectedParagraph
        {
            get { return _selectedParagraph; }
            set
            {
                if (value != _selectedParagraph)
                {
                    _selectedParagraph = value;
                    NotifyPropertyChanged(nameof(SelectedParagraph));
                    NotifyPropertyChanged(nameof(ParagraphName));
                    NotifyPropertyChanged(nameof(ParagraphActiveTill));
                }
            }
        }

        public ObservableCollection<ParagraphContent> ParagraphContents { get; set; }

        private ParagraphContent _selectedParagraphContent;

        public ParagraphContent SelectedParagraphContent
        {
            get { return _selectedParagraphContent; }
            set
            {
                if (value != _selectedParagraphContent)
                {
                    _selectedParagraphContent = value;
                    NotifyPropertyChanged(nameof(SelectedParagraphContent));
                    NotifyPropertyChanged(nameof(ParagraphContentName));
                }
            }
        }

        public ObservableCollection<Document> Documents { get; set; } = new ObservableCollection<Document>();

        public Document SelectedDocument { get; set; }

        public string ParagraphName
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.Name;
                return string.Empty;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.Name))
                {
                    _selectedParagraph.Name = value;
                    NotifyPropertyChanged(nameof(Paragraphs));
                    ParagraphListBox.Items.Refresh();
                }
            }
        }

        public DateTime? ParagraphActiveTill
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.ActiveTill;

                return null;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.ActiveTill))
                {
                    _selectedParagraph.ActiveTill = value;
                    NotifyPropertyChanged(nameof(Paragraphs));
                }
            }
        }

        public string ParagraphContentName
        {
            get
            {
                if (_selectedParagraphContent != null)
                    return _selectedParagraphContent.Name;
                return string.Empty;
            }
            set
            {
                if (!value.Equals(_selectedParagraphContent.Name))
                {
                    _selectedParagraphContent.Name = value;
                    NotifyPropertyChanged(nameof(ParagraphContents));
                    RefreshParagraphContents();
                }
            }
        }

        private readonly DataModel context = ((App)Application.Current).GetDBContext;

        public GlobalParagraphWindow()
        {
            this.DataContext = this;

            StyleManager.ApplicationTheme = new VisualStudio2013Theme();

            LocalizationManager.Manager = new LocalizationManager()
            {
                ResourceManager = RadRichTextBoxResources.ResourceManager,
                Culture = new CultureInfo("ru-RU")
            };

            InitializeComponent();

            this.radRichTextBoxRibbonUI.DataContext = this.radRichTextBox1.Commands;
            var dictionary = new RadDictionary();

            using (var ms = new MemoryStream(Properties.Resources.ru_RU))
                dictionary.Load(ms);

            ((DocumentSpellChecker)this.radRichTextBox1.SpellChecker).AddDictionary(dictionary, CultureInfo.InvariantCulture);

            radRichTextBox1.ContextMenu = new Telerik.Windows.Controls.RichTextBoxUI.ContextMenu();

            Paragraphs = new ObservableCollection<Paragraph>(context.Paragraphs.Where(y => y.IsGlobal && y.Deleted == false).OrderBy(x => x.OrderNo).ToList());

            SelectedParagraph = Paragraphs.FirstOrDefault();
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName.Equals(nameof(SelectedParagraph)))
            {
                RefreshParagraphContents();
                RefreshDocumentList();

                NotifyPropertyChanged(nameof(ParagraphContents));
                if (ParagraphContents.Any()) SelectedParagraphContent = ParagraphContents.First();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RefreshParagraphContents()
        {
            if (SelectedParagraph == null)
            {
                ParagraphContents = new ObservableCollection<ParagraphContent>();
            }
            else
            {
                var content = SelectedParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList();

                ParagraphContents = new ObservableCollection<ParagraphContent>(content);
            }

            ParagraphContentsListBox.Items.Refresh();
            NotifyPropertyChanged(nameof(ParagraphContents));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            context.SaveChanges();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newParagraph = new Paragraph
            {
                Id = Guid.NewGuid(),
                Name = "Новый параграф",
                Deleted = false,
                IsGlobal = true
            };

            context.Paragraphs.Add(newParagraph);

            var hpc = new ParagraphContent { Id = Guid.NewGuid(), Name = "Версия по умолчанию", Paragraph = newParagraph, DefaultVersion = true, Deleted = false, PassConditions = true };
            newParagraph.ParagraphContents.Add(hpc);
            context.ParagraphContents.Add(hpc);

            Paragraphs.Add(newParagraph);

            foreach (Paragraph p in Paragraphs)
            {
                p.OrderNo = Paragraphs.IndexOf(p);
            }

            newParagraph.PassConditions = true;

            ParagraphListBox.Items.Refresh();

            context.SaveChanges();

            SelectedParagraph = newParagraph;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph == null)
                return;

            if (SelectedParagraph.InverseParentParagraph != null && SelectedParagraph.InverseParentParagraph.Any(x => !x.Deleted))
            {
                MessageBox.Show("Параграф нельзя удалить - с ним связаны документы");
                return;
            }

            var childParagraphs = SelectedParagraph.InverseParentParagraph.Where(x => x.Deleted).ToList();

            childParagraphs.Select(context.Paragraphs.Remove);

            SelectedParagraph.Deleted = true;
            Paragraphs.Remove(SelectedParagraph);

            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();

            context.SaveChanges();
        }

        private void ParagraphWizard_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddWizardWindow(SelectedDocument, Paragraphs, context)
            {
                ShowInTaskbar = true
            };
            window.Show();
        }

        private void AddVersionButton_Click(object sender, RoutedEventArgs e)
        {
            var hpc = new ParagraphContent { Id = Guid.NewGuid(), Name = "Новая версия", Paragraph = SelectedParagraph, Deleted = false, DefaultVersion = false };
            SelectedParagraph.ParagraphContents.Add(hpc);
            ParagraphContents.Add(hpc);

            SelectedParagraphContent = hpc;
        }

        private void DeleteVersionButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedParagraphContent.Deleted = true;
            ParagraphContents.Remove(SelectedParagraphContent);
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedParagraphContent is null))
            {
                foreach (ParagraphContent pc in ParagraphContents)
                {
                    pc.DefaultVersion = false;
                }

                SelectedParagraphContent.DefaultVersion = true;
                RefreshParagraphContents();
            }
        }

        private void CompareParagraphs_Click(object sender, RoutedEventArgs e)
        {
            var provider = new Telerik.Windows.Documents.FormatProviders.Xaml.XamlFormatProvider();

            if (ParagraphContents.Where(x => x.DiffSource == true).Count() == 0)
            {
                MessageBox.Show("Не выбран источник для сравнения");
                return;
            }

            if (ParagraphContents.Where(x => x.DiffTarget == true).Count() == 0)
            {
                MessageBox.Show("Не выбран целевой параграф");
                return;
            }

            var firstDoc = ParagraphContents.Where(x => x.DiffSource == true).First();
            var secondDoc = ParagraphContents.Where(x => x.DiffTarget == true).Last();

            var oldText = provider.Import(firstDoc.Content);
            var newText = provider.Import(secondDoc.Content);

            var diffWindow = new DiffWindow(oldText, newText, $"{firstDoc.Name} → {secondDoc.Name}");
            diffWindow.ShowDialog();
        }

        private void SetAsCompareSource_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedParagraphContent is null))
            {
                foreach (ParagraphContent pc in ParagraphContents)
                {
                    pc.DiffSource = false;
                }

                SelectedParagraphContent.DiffSource = true;
                RefreshParagraphContents();
            }
        }

        private void SetAsCompareTarget_Click(object sender, RoutedEventArgs e)
        {
            if (!(SelectedParagraphContent is null))
            {
                foreach (ParagraphContent pc in ParagraphContents)
                {
                    pc.DiffTarget = false;
                }

                SelectedParagraphContent.DiffTarget = true;
                RefreshParagraphContents();
            }
        }

        private void ParagraphListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(SelectedParagraph is null))
            {
                var conditions = TelerikBuilder.GetConditions(SelectedParagraph.Condition);

                var dataWindow = new ConditionWindow(conditions, SelectedParagraph.ActiveFrom, SelectedParagraph.ActiveTill);
                dataWindow.ShowDialog();

                SelectedParagraph.Condition = Extensions.Serialize(conditions);

                SelectedParagraph.ActiveFrom = dataWindow.ActiveFrom;
                SelectedParagraph.ActiveTill = dataWindow.ActiveTill;

                string s = string.Empty;
                if (SelectedParagraph.ActiveFrom != null)
                {
                    s += "Действует с " + SelectedParagraph.ActiveFrom.Value.ToShortDateString();
                    if (SelectedParagraph.ActiveTill != null)
                    {
                        s += " по " + SelectedParagraph.ActiveTill.Value.ToShortDateString();
                    }
                }
                else if (SelectedParagraph.ActiveTill != null)
                {
                    s += "Действует по " + SelectedParagraph.ActiveTill.Value.ToShortDateString();
                }

                if (conditions.Any())
                {
                    if (string.IsNullOrEmpty(s))
                        s = "Если ";
                    else
                        s += ", если ";

                    s += string.Join(" и ", conditions.Where(x => x.Conditions.Any()).Select(group =>
                    {
                        return $"({string.Join(group.GroupType == GroupTypeEnum.And ? " и " : " или ", group.Conditions.Select(x => $"{x.Variable} {x.Operator.GetDisplayName()} {x.Value}"))})";
                    }));
                }

                SelectedParagraph.Tooltip = s;
                NotifyPropertyChanged(nameof(Paragraphs));
                ParagraphListBox.Items.Refresh();
            }
        }

        private void DocumentListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedDocument == null)
                return;

            //var context = ((App)Application.Current).GetDBContext;
            //context.Refresh(RefreshMode.StoreWins, SelectedDocument);

            var agreementTool = new AgreementWindow(SelectedDocument, null, new List<DataTable>());

            agreementTool.ShowDialog();
        }

        private void RadListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(SelectedParagraphContent is null))
            {
                var conditions = TelerikBuilder.GetConditions(SelectedParagraphContent.Condition);

                var dataWindow = new ConditionWindow(conditions, SelectedParagraphContent.ActiveFrom, SelectedParagraphContent.ActiveTill);
                dataWindow.ShowDialog();

                SelectedParagraphContent.Condition = Extensions.Serialize(conditions);
                SelectedParagraphContent.ActiveFrom = dataWindow.ActiveFrom;
                SelectedParagraphContent.ActiveTill = dataWindow.ActiveTill;

                string s = string.Empty;
                if (SelectedParagraphContent.ActiveFrom != null)
                {
                    s += "Действует с " + SelectedParagraphContent.ActiveFrom.Value.ToShortDateString();
                    if (SelectedParagraphContent.ActiveTill != null)
                    {
                        s += " по " + SelectedParagraphContent.ActiveTill.Value.ToShortDateString();
                    }
                }
                else if (SelectedParagraphContent.ActiveTill != null)
                {
                    s += "Действует по " + SelectedParagraphContent.ActiveTill.Value.ToShortDateString();
                }

                if (conditions.Any())
                {
                    if (string.IsNullOrEmpty(s))
                        s = "Если ";
                    else
                        s += ", если ";

                    s += string.Join(" и ", conditions.Where(x => x.Conditions.Any()).Select(group =>
                    {
                        return $"({string.Join(group.GroupType == GroupTypeEnum.And ? " и " : " или ", group.Conditions.Select(x => $"{x.Variable} {x.Operator.GetDisplayName()} {x.Value}"))})";
                    }));
                }
                SelectedParagraphContent.Tooltip = s;

                ParagraphContentsListBox.Items.Refresh();
                NotifyPropertyChanged(nameof(ParagraphContents));
            }
        }

        private void RefreshDocumentList()
        {
            if (SelectedParagraph == null)
            {
                Documents = new ObservableCollection<Document>();
            }
            else
            {
                var content = SelectedParagraph.InverseParentParagraph.Where(x => !x.Deleted).Select(x => x.DocumentContent?.Document).Distinct().ToList();

                Documents = new ObservableCollection<Document>(content);
            }

            DocumentListBox.Items.Refresh();
            NotifyPropertyChanged(nameof(Documents));
        }

        public void Input_Unbreakable_Click(object sender, RoutedEventArgs e)
        {
            radRichTextBox1.Insert(Constants.Unbreakable);
        }

        public void Input_Unbreakable_Row_Click(object sender, RoutedEventArgs e)
        {
            radRichTextBox1.Insert(Constants.UnbreakableRow);
        }

        public void Input_Isolated_Numbered_List_Click(object sender, RoutedEventArgs e)
        {
            radRichTextBox1.Insert(Constants.IsolatedNumberedList);
        }

        private void Input_Keep_With_Next_Click(object sender, RoutedEventArgs e)
        {
            radRichTextBox1.Insert(Constants.KeepWithNext);
        }
    }
}
