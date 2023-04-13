using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Base;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.Proofing;
using Telerik.Windows.Documents.RichTextBoxCommands;
using WordMergeEngine.Assets.Enums;
using WordMergeEngine.Helpers;
using WordMergeEngine.Models;
using WordMergeEngine;
using TelerikModel = Telerik.Windows.Documents.Model;
using Paragraph = WordMergeEngine.Models.Paragraph;
using WordMergeEngine.Assets;
using Microsoft.EntityFrameworkCore;

namespace WordMergeUtil_Core.AgreementTool
{
    public partial class AgreementWindow : Window, INotifyPropertyChanged
    {
        public bool DocumentIsOpen { get; set; }

        public bool DocumentIsOpenAndNotFiltered { get; set; }

        public Document SelectedDocument { get; set; }

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
                    NotifyPropertyChanged(nameof(ParagraphIsFixNumeration));
                    NotifyPropertyChanged(nameof(RefName));
                    NotifyPropertyChanged(nameof(ParagraphTag));
                    NotifyPropertyChanged(nameof(ParagraphIsAttachment));
                    NotifyPropertyChanged(nameof(IsCustomNumbering));
                    NotifyPropertyChanged(nameof(StartNumberingFrom));
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
                    UpdateDataFields();
                }
            }
        }

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

        public bool ParagraphIsFixNumeration
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.IsFixNumeration;

                return false;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.IsFixNumeration))
                {
                    _selectedParagraph.IsFixNumeration = value;
                    NotifyPropertyChanged(nameof(Paragraphs));
                    ParagraphListBox.Items.Refresh();
                }
            }
        }

        public string RefName
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.ReferenceName;
                return string.Empty;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.ReferenceName))
                {
                    _selectedParagraph.ReferenceName = value;
                    NotifyPropertyChanged(nameof(Paragraphs));
                    ParagraphListBox.Items.Refresh();
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

        public string ParagraphTag
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.Tag;
                return string.Empty;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.Name))
                {
                    _selectedParagraph.Tag = value;
                    NotifyPropertyChanged(nameof(Paragraphs));
                    ParagraphListBox.Items.Refresh();
                }
            }
        }

        public bool ParagraphIsAttachment
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.IsAttachment;
                return false;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.IsAttachment))
                {
                    _selectedParagraph.IsAttachment = value;
                    NotifyPropertyChanged(nameof(Paragraphs));
                    ParagraphListBox.Items.Refresh();
                }
            }
        }

        public bool IsCustomNumbering
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.IsCustomNumbering;
                return false;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.IsCustomNumbering))
                {
                    _selectedParagraph.IsCustomNumbering = value;
                    NotifyPropertyChanged(nameof(IsCustomNumbering));
                    ParagraphListBox.Items.Refresh();
                }
            }
        }

        public string StartNumberingFrom
        {
            get
            {
                if (_selectedParagraph != null)
                    return _selectedParagraph.StartNumberingFrom;
                return string.Empty;
            }
            set
            {
                if (!value.Equals(_selectedParagraph.StartNumberingFrom))
                {
                    _selectedParagraph.StartNumberingFrom = value;
                    NotifyPropertyChanged(nameof(StartNumberingFrom));
                    ParagraphListBox.Items.Refresh();
                }
            }
        }

        private readonly DataModel context = ((App)Application.Current).GetDBContext;

        private DataTable _documentVariables;
        public DataTable DocumentVariables
        {
            get
            {
                if (this._documentVariables == null)
                {
                    this._documentVariables = this.CreateDocumentVariablesDataTable();
                }

                return this._documentVariables;
            }
        }

        public DataTable ReportVariables { get; set; }

        public List<DataTable> ChildTables { get; set; }

        public List<string> TableNames => ChildTables?.ConvertAll(x => x.TableName) ?? new List<string>();

        public DataTable Variables
        {
            get
            {
                if (ReportVariables == null)
                    return DocumentVariables;

                var result = ReportVariables.Copy();

                foreach (DataRow row in DocumentVariables.Rows)
                    result.Rows.Add(row.ItemArray);

                return result;
            }
        }

        private DataTable CreateDocumentVariablesDataTable()
        {
            var dataTable = new DataTable
            {
                TableName = "DocVariables"
            };
            dataTable.Columns.Add(new DataColumn("Variable", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Value", typeof(string)));

            return dataTable;
        }

        public AgreementWindow()
        {
            this.DataContext = this;

            DocumentIsOpen = false;
            DocumentIsOpenAndNotFiltered = false;

            StyleManager.ApplicationTheme = new VisualStudio2013Theme();

            LocalizationManager.Manager = new LocalizationManager()
            {
                ResourceManager = RadRichTextBoxResources.ResourceManager,
                Culture = new CultureInfo("ru-RU")
            };

            InitializeComponent();

            this.radRichTextBoxRibbonUI.DataContext = this.radRichTextBox1.Commands;
            var dictionary = new RadDictionary();

            using (var ms = new MemoryStream(Resources.ru_RU))
                dictionary.Load(ms);

            ((DocumentSpellChecker)this.radRichTextBox1.SpellChecker).AddDictionary(dictionary, CultureInfo.InvariantCulture);

            SizeChanged += ChangeDropDownHeight;

            radRichTextBox1.ContextMenu = new Telerik.Windows.Controls.RichTextBoxUI.ContextMenu();
        }

        private void ChangeDropDownHeight(object sender, SizeChangedEventArgs e) => AddMailMergeButton.DropDownMaxHeight = Math.Max(ActualHeight - 210, 200);

        public AgreementWindow(Document document, DataTable variables, List<DataTable> childTables) : this()
        {
            ReportVariables = variables;
            ChildTables = childTables;

            FillChildTablesButton();
            FillChildRowsButton();
            FillGlobalTableButton();

            if (document != null)
            {
                LoadAction = () => OpenDocument(document);
            }

            LogHelper.Log(GetType().Name);
        }

        private Action LoadAction;

        private void OnLoad(object sender, RoutedEventArgs e) => LoadAction?.Invoke();

        void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= ChangeDropDownHeight;
            this.radRichTextBoxRibbonUI.DataContext = null;

            ((DocumentSpellChecker)this.radRichTextBox1.SpellChecker).RemoveDictionary(CultureInfo.InvariantCulture);
            LoadAction = null;

            ClearChildTablesButton();
            ClearChildFieldButton();

            _documentVariables?.Dispose();
            ReportVariables?.Dispose();
        }

        void RadRichTextBox_CommandExecuting(object sender, CommandExecutingEventArgs e)
        {
            if (e.Command is PasteCommand)
            {
                if (ClipboardEx.ContainsDocument())
                {
                    var document = ClipboardEx.GetDocument();
                    var xamlDoc = new XamlFormatProvider().Export(document.ToDocument());

                    e.Cancel = true;
                    this.radRichTextBox1.InsertFragment(document);

                    LogHelper.GetLogger().Debug($"Вставка документа из буфера обмена: {xamlDoc}");
                }
                else
                {
                    var text = ClipboardEx.GetText();

                    LogHelper.GetLogger().Debug($"Вставка текста из буфера обмена: {text}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadParagraphContents()
        {
            if (SelectedDocument == null)
                return;

            RefreshParagraphContents();
            RecalculateParagraphContentFilters();
        }

        private void RecalculateParagraphContentFilters()
        {
            foreach (var pc in ParagraphContents)
            {
                if (pc.ActiveFrom.HasValue & pc.ActiveFrom > VersionDatePicker.SelectedValue)
                {
                    pc.PassConditions = false;
                    continue;
                }

                if (pc.ActiveTill.HasValue & pc.ActiveTill < VersionDatePicker.SelectedValue)
                {
                    pc.PassConditions = false;
                    continue;
                }

                if (string.IsNullOrEmpty(pc.Condition) || (pc.DefaultVersion == true))
                {
                    pc.PassConditions = true;
                    continue;
                }

                pc.PassConditions = TelerikBuilder.CheckConditionGroup(Variables, pc.Condition);

            }
            NotifyPropertyChanged(nameof(ParagraphContents));
            if (ParagraphContents.Any()) SelectedParagraphContent = ParagraphContents.First();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName.Equals(nameof(SelectedParagraph)))
            {
                LoadParagraphContents();
            }

            if (propertyName.Equals(nameof(IsContentUnlocked)))
            {
                radRichTextBoxRibbonUI.IsEnabled = IsContentUnlocked;
            }
        }

        private void PrepareParagraphs()
        {
            if (SelectedDocument.Paragraph.Count == 0)
            {
                var header = new Paragraph { Id = Guid.NewGuid(), Name = "Вступление" };
                SelectedDocument.Paragraph.Add(header);

                var hpc = new ParagraphContent { Id = Guid.NewGuid(), Name = "Версия по умолчанию", DefaultVersion = true };
                header.ParagraphContents.Add(hpc);

                var p = new Paragraph { Id = Guid.NewGuid(), Name = "Первый параграф" };
                SelectedDocument.Paragraph.Add(p);

                var pc = new ParagraphContent { Id = Guid.NewGuid(), Name = "Версия по умолчанию", DefaultVersion = true };
                p.ParagraphContents.Add(pc);

                var footer = new Paragraph { Id = Guid.NewGuid(), Name = "Заключение" };
                SelectedDocument.Paragraph.Add(footer);

                var fpc = new ParagraphContent { Id = Guid.NewGuid(), Name = "Версия по умолчанию", DefaultVersion = true };
                footer.ParagraphContents.Add(fpc);
            }

            foreach (var pp in SelectedDocument.Paragraph)
            {
                if (pp.Level is null) pp.Level = 1;
                if (pp.Numerable is null) pp.Numerable = false;
            }

            context.SaveChanges();
        }

        private void PrepareDocument()
        {
            VersionDatePicker.SelectedDate = DateTime.Today;
            DocumentIsOpen = true;
            DocumentIsOpenAndNotFiltered = true;

            PrepareParagraphs();
            FillParagraphList();


            foreach (var p in Paragraphs)
            {
                p.OrderNo = Paragraphs.IndexOf(p);
                p.Errors = "Ok";
                p.PassConditions = true;
            }

            RecalculateParagraphsFilters();

            SelectedParagraph = Paragraphs.First();
            NotifyPropertyChanged(nameof(IsBlocked));
            NotifyPropertyChanged(nameof(IsUnlocked));
            NotifyPropertyChanged(nameof(ContentLockedByGlobal));
            NotifyPropertyChanged(nameof(IsContentUnlocked));
            NotifyPropertyChanged(nameof(IsEnable));
            NotifyPropertyChanged(nameof(IsContentEnabled));
            NotifyPropertyChanged(nameof(IsFilterEnabled));
            NotifyPropertyChanged(nameof(SelectedDocument));
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();

            DocumentVariables.Clear();

            if (!string.IsNullOrEmpty(SelectedDocument.Data))
            {
                using (var sr = new StringReader(SelectedDocument.Data))
                    DocumentVariables.ReadXml(sr);
            }

            UpdateDataFields();
        }

        private void CreateDocument_Click(object sender, RoutedEventArgs e)
        {
            SelectedDocument = new Document
            {
                Id = Guid.NewGuid(),
                Name = "Новый документ"
            };

            context.Documents.Add(SelectedDocument);

            var docContent = new DocumentContent
            {
                Id = Guid.NewGuid(),
                Document = SelectedDocument,
                DefaultVersion = true,
                Version = 1,
                CreatedOn = DateTime.Now
            };

            context.DocumentContents.Add(docContent);
            context.SaveChanges();

            PrepareDocument();
        }

        private void OpenDocument_Click(object sender, RoutedEventArgs e)
        {
            var window = new OpenDocumentWindow(context)
            {
                Owner = this
            };

            window.ShowDialog();

            if (window.DeletedDocuments.Any() && SelectedDocument != null && window.DeletedDocuments.Contains(SelectedDocument.Id))
            {
                CloseDocument();
            }
            else if (SelectedDocument != null && window.ImportIds.Contains(SelectedDocument.Id))
            {
                OpenDocument(context.Documents.FirstOrDefault(x => x.Id == SelectedDocument.Id));
            }

            if (window.DialogResult != true)
            {
                return;
            }
            else if (window.SelectedDocument != null)
            {
                if (CheckSavingBeforeAction())
                    return;

                OpenDocument(context.Documents.FirstOrDefault(x => x.Id == window.SelectedDocument.Id));
            }
            else
            {
                MessageBox.Show("Документ не выбран");
            }
        }

        private void OpenDocument(Document document)
        {
            SelectedDocument = document;

            PrepareDocument();

            if (!IsUnlocked)
                MessageBox.Show($"Данный шаблон открыт у {SelectedDocument.LockedBy} и недоступен для редактирования");

            TrackChangeChecker.IsEnabled = !SelectedDocument.TrackChanges;
        }

        private void SaveDocument_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void SaveData(bool checkIsModified = true)
        {
            if (checkIsModified && !CheckIsModified())
                return;

            foreach (var p in Paragraphs)
            {
                p.OrderNo = Paragraphs.IndexOf(p);
            }

            var sw = new StringWriter();
            DocumentVariables.WriteXml(sw);
            SelectedDocument.CurrentVersion.Data = sw.ToString();

            var (parId, parContentId) = DocBuilder.SaveChanges(context, SelectedParagraph?.Id, SelectedParagraphContent?.Id);
            OpenDocument(SelectedDocument);

            if (parId != null)
                SelectedParagraph = SelectedDocument.Paragraph.FirstOrDefault(x => x.Id == parId);

            if (parContentId != null)
                SelectedParagraphContent = SelectedParagraph.ParagraphContents.FirstOrDefault(x => x.Id == parContentId);
        }

        private void Template_Click(object sender, RoutedEventArgs e)
        {
            //var targetDocument = new Telerik.Windows.Documents.Model.RadDocument();

            var provider = new XamlFormatProvider();

            var document = new TelerikModel.RadDocument();

            if (!string.IsNullOrEmpty(SelectedDocument.Template))
            {
                document = provider.Import(SelectedDocument.Template);
            }

            var previewWindow = new PreviewWindow(document)
            {
                Title = "Редактирование шаблона"
            };
            previewWindow.ShowDialog();

            SelectedDocument.CurrentVersion.Template = provider.Export(document);
        }

        private void CompareParagraphs_Click(object sender, RoutedEventArgs e)
        {
            var provider = new XamlFormatProvider();

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
        private void Test_Click(object sender, RoutedEventArgs e)
        {
            var (targetDocument, dt) = TelerikBuilder.PrintDocument(SelectedDocument, ReportVariables);
            TelerikBuilder.SetMergeSource(targetDocument, dt);

            var previewWindow = new PreviewWindow(targetDocument)
            {
                ShowInTaskbar = true
            };
            previewWindow.Show();

        }

        private void DataWindow_Click(object sender, RoutedEventArgs e)
        {
            DataWindow dw = new DataWindow(ReportVariables, DocumentVariables);
            dw.ShowDialog();

            UpdateDataFields();

        }

        private void UpdateDataFields()
        {
            DataTable dt = GetDataForDocument();

            radRichTextBox1.Document.MailMergeDataSource.ItemsSource = dt.AsEnumerable();
            radRichTextBox1.Document.MailMergeDataSource.MoveToLast();
            radRichTextBox1.Document.MailMergeDataSource.MoveToPrevious();
            radRichTextBox1.Document.MailMergeDataSource.MoveToFirst();
        }

        private DataTable GetDataForDocument()
        {
            DataTable dt = new DataTable("Data");
            dt.Rows.Add();
            foreach (DataRow r in Variables.Rows)
            {
                dt.Columns.Add(new DataColumn(r["Variable"].ToString(), typeof(String)));
                dt.Rows[0][r["Variable"].ToString()] = r["Value"];
            }

            int col = 0;
            foreach (Paragraph p in Paragraphs.Where(p => !string.IsNullOrEmpty(p.ReferenceName)))
            {
                var colName = "Ref_" + Regex.Replace(p.ReferenceName.ToString(), "\\W", ""); ;
                if (dt.Columns.Contains(colName))
                {
                    col++;
                    colName += col.ToString();
                }
                dt.Columns.Add(new DataColumn(colName, typeof(String)));
                dt.Rows[0][colName] = p.RelativeNo;
            }

            return dt;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Paragraph newParagraph = new Paragraph
            {
                Id = Guid.NewGuid(),
                Name = "Новый параграф",
                Level = SelectedParagraph?.Level,
                DocumentContent = SelectedDocument.CurrentVersion,
                OrderNo = (SelectedParagraph?.OrderNo ?? 0) + 1,
                Deleted = false,
                Numerable = true
            };

            SelectedDocument.Paragraph.Add(newParagraph);
            context.Paragraphs.Add(newParagraph);

            ParagraphContent hpc = new ParagraphContent { Id = Guid.NewGuid(), Name = "Версия по умолчанию", Paragraph = newParagraph, DefaultVersion = true, Deleted = false, PassConditions = true };
            newParagraph.ParagraphContents.Add(hpc);
            context.ParagraphContents.Add(hpc);

            Paragraphs.Insert((SelectedParagraph?.OrderNo ?? 0) + 1, newParagraph);

            foreach (Paragraph p in Paragraphs)
            {
                p.OrderNo = Paragraphs.IndexOf(p);
            }

            newParagraph.PassConditions = true;
            SelectedParagraph = newParagraph;

            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph.Deleted)
            {
                SelectedParagraph.Deleted = false;

                foreach (var (paragraph, index) in SelectedDocument.Paragraph.Where(x => !x.Deleted).OrderBy(x => x.OrderNo).Select((x, i) => (x, i)))
                    paragraph.OrderNo = index;
            }
            else
            {
                var oldOrder = SelectedParagraph.OrderNo;
                SelectedParagraph.Deleted = true;
                SelectedParagraph.OrderNo = null;

                foreach (var (paragraph, index) in SelectedDocument.Paragraph.Where(x => !x.Deleted).OrderBy(x => x.OrderNo).Select((x, i) => (x, i)))
                    paragraph.OrderNo = index;

                SelectedParagraph = SelectedDocument.Paragraph.OrderBy(x => x.OrderNo).FirstOrDefault(x => !x.Deleted && (x.OrderNo == oldOrder || x.OrderNo == oldOrder - 1));
            }

            FillParagraphList();
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedParagraph.Level = SelectedParagraph.Level + 1;

            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph.Level != 0) SelectedParagraph.Level = SelectedParagraph.Level - 1;

            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeOrderNInParagraph(false);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeOrderNInParagraph(true);

        }

        private void ChangeOrderNInParagraph(bool isUp)
        {
            var orderNo = Paragraphs.IndexOf(SelectedParagraph);
            if (isUp)
            {

                if (orderNo > 0)
                {
                    var tmp = Paragraphs[orderNo - 1];
                    Paragraphs[orderNo - 1] = SelectedParagraph;
                    Paragraphs[orderNo] = tmp;
                    Paragraphs[orderNo - 1].OrderNo = orderNo - 1;
                    Paragraphs[orderNo].OrderNo = orderNo;
                    SelectedParagraph = Paragraphs[orderNo - 1];
                }
            }
            else
            {
                if (orderNo < Paragraphs.Count - 1)
                {
                    var tmp = Paragraphs[orderNo + 1];
                    Paragraphs[orderNo + 1] = SelectedParagraph;
                    Paragraphs[orderNo] = tmp;

                    Paragraphs[orderNo + 1].OrderNo = orderNo + 1;
                    Paragraphs[orderNo].OrderNo = orderNo;
                    SelectedParagraph = Paragraphs[orderNo + 1];
                }
            }
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void DeleteVersionButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedParagraphContent.Deleted = true;
            ParagraphContents.Remove(SelectedParagraphContent);
        }

        private void AddVersionButton_Click(object sender, RoutedEventArgs e)
        {
            ParagraphContent hpc = new ParagraphContent { Id = Guid.NewGuid(), Name = "Новая версия", Paragraph = SelectedParagraph, Deleted = false, DefaultVersion = false };
            SelectedParagraph.ParagraphContents.Add(hpc);
            ParagraphContents.Add(hpc);

            SelectedParagraphContent = hpc;
        }

        private void IsNumeric_Click(object sender, RoutedEventArgs e)
        {
            //Paragraphs.Where(x => x.Id == SelectedParagraph.Id).FirstOrDefault().Numerable = SelectedParagraph.Numerable == true? false : true;
            SelectedParagraph.Numerable = !SelectedParagraph.Numerable;

            ShowOrHideStartNumbering(false);

            var nextParagraph = Paragraphs.SkipWhile(x => x.Id != SelectedParagraph.Id).Skip(1).FirstOrDefault();

            if (nextParagraph != null && nextParagraph.IsCustomNumbering)
                nextParagraph.StartNumberingFrom = nextParagraph.CustomNo;

            IsCustomNumbering = false;
            StartNumberingFrom = string.Empty;
            SelectedParagraph.CustomNo = string.Empty;
            SelectedParagraph.CustomLevel = 0;

            NotifyPropertyChanged(nameof(SelectedParagraph.Numerable));

            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void ParagraphListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(SelectedParagraph is null))
            {
                var conditions = TelerikBuilder.GetConditions(SelectedParagraph.Condition);

                var dataWindow = new ConditionWindow(conditions, SelectedParagraph.ActiveFrom, SelectedParagraph.ActiveTill);
                dataWindow.ShowDialog();

                SelectedParagraph.Condition = conditions.Any() ? Extensions.Serialize(conditions) : string.Empty;

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
                RecalculateParagraphsFilters();
                NotifyPropertyChanged(nameof(Paragraphs));
                ParagraphListBox.Items.Refresh();
            }
        }

        private void FilterButton_Checked(object sender, RoutedEventArgs e)
        {
            RecalculateParagraphsFilters();

            FillParagraphList();

            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();

            RecalculateParagraphContentFilters();

            DocumentIsOpenAndNotFiltered = false;
            NotifyPropertyChanged(nameof(IsFilterEnabled));

        }

        private void RecalculateParagraphsFilters()
        {
            foreach (Paragraph p in SelectedDocument.Paragraph.Where(y => y.Deleted == false).OrderBy(x => x.OrderNo).ToList())
            {
                if (string.IsNullOrEmpty(p.Condition))
                {
                    p.PassConditions = true;
                    continue;
                }

                p.PassConditions = TelerikBuilder.CheckConditionGroup(Variables, p.Condition);
            }
        }

        private void FilterButton_Unchecked(object sender, RoutedEventArgs e)
        {
            /*foreach (Paragraph p in SelectedDocument.Paragraph.Where(y => y.Deleted == false).OrderBy(x => x.OrderNo).ToList())
            {
                p.PassConditions = true;
            }*/
            FillParagraphList();
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
            DocumentIsOpenAndNotFiltered = true;
            NotifyPropertyChanged(nameof(IsFilterEnabled));
        }

        private string copiedConditions = string.Empty;

        public Boolean ConditionBufferNotEmpty { get; set; }

        private void CopyConditions_Click(object sender, RoutedEventArgs e)
        {
            ConditionBufferNotEmpty = true;
            copiedConditions = SelectedParagraph.Condition;
            NotifyPropertyChanged(nameof(IsConditionEnabled));
        }

        private void PasteConditions_Click(object sender, RoutedEventArgs e)
        {
            SelectedParagraph.Condition = copiedConditions;
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void RadListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(SelectedParagraphContent is null) && IsContentEnabled)
            {
                var conditions = TelerikBuilder.GetConditions(SelectedParagraphContent.Condition);

                var dataWindow = new ConditionWindow(conditions, SelectedParagraphContent.ActiveFrom, SelectedParagraphContent.ActiveTill);
                dataWindow.ShowDialog();

                SelectedParagraphContent.Condition = conditions.Any() ? Extensions.Serialize(conditions) : string.Empty;
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

        private void ParagraphWizard_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddWizardWindow(SelectedDocument, Paragraphs, context)
            {
                ShowInTaskbar = true
            };
            window.ShowDialog();

            if (window.IsParagraphChanged)
            {
                FillParagraphList();
                NotifyPropertyChanged(nameof(Paragraphs));
                ParagraphListBox.Items.Refresh();
            }
            else if (window.IsVersionChanged)
            {
                RefreshParagraphContents();
            }
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

        private void RefreshParagraphContents()
        {
            if (SelectedParagraph == null)
            {
                ParagraphContents = new ObservableCollection<ParagraphContent>();
            }
            else
            {
                var parentContent = SelectedParagraph.ParentParagraph == null ? null : context.ParagraphContents.Where(pc => pc.Paragraph.Id == SelectedParagraph.ParentParagraph.Id && pc.Deleted == false).ToList().OrderBy(p => p.DefaultVersion).Reverse().ToList();
                var content = SelectedParagraph.ParagraphContents.Where(pc => pc.Deleted == false).OrderBy(p => p.DefaultVersion).Reverse().ToList();

                ParagraphContents = new ObservableCollection<ParagraphContent>(parentContent ?? content);
            }

            ParagraphContentsListBox.Items.Refresh();
            NotifyPropertyChanged(nameof(ParagraphContents));
            NotifyPropertyChanged(nameof(IsContentEnabled));
            NotifyPropertyChanged(nameof(IsContentUnlocked));
            NotifyPropertyChanged(nameof(ContentLockedByGlobal));
            NotifyPropertyChanged(nameof(IsBlocked));
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DateInputDialog
            {
                Owner = this
            };
            if (dialog.ShowDialog() != true) return;

            var filterDate = dialog.SelectedData.Value;

            var targetDocument = TelerikBuilder.GetApproveDocument(SelectedDocument, Variables, filterDate);

            var previewWindow = new PreviewWindow(targetDocument)
            {
                ShowInTaskbar = true
            };
            previewWindow.Show();
        }

        public void Input_Unbreakable_Click(object sender, RoutedEventArgs e)
        {
            if (IsContentUnlocked)
                radRichTextBox1.Insert(Constants.Unbreakable);
        }

        public void Input_Unbreakable_Row_Click(object sender, RoutedEventArgs e)
        {
            if (IsContentUnlocked)
                radRichTextBox1.Insert(Constants.UnbreakableRow);
        }

        public void Input_Isolated_Numbered_List_Click(object sender, RoutedEventArgs e)
        {
            if (IsContentUnlocked)
                radRichTextBox1.Insert(Constants.IsolatedNumberedList);
        }

        private void CloseDocument()
        {
            VersionDatePicker.SelectedDate = null;

            DocumentIsOpen = false;
            NotifyPropertyChanged(nameof(IsEnable));

            DocumentIsOpenAndNotFiltered = false;
            NotifyPropertyChanged(nameof(IsFilterEnabled));

            SelectedDocument = null;
            NotifyPropertyChanged(nameof(SelectedDocument));
            NotifyPropertyChanged(nameof(IsBlocked));
            NotifyPropertyChanged(nameof(IsUnlocked));
            NotifyPropertyChanged(nameof(ContentLockedByGlobal));

            Paragraphs = new ObservableCollection<Paragraph>();
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphContents = new ObservableCollection<ParagraphContent>();
            NotifyPropertyChanged(nameof(ParagraphContents));
            NotifyPropertyChanged(nameof(IsContentEnabled));
            NotifyPropertyChanged(nameof(IsContentUnlocked));

            SelectedParagraph = null;
            SelectedParagraphContent = null;

            ParagraphListBox.Items.Refresh();
            ParagraphContentsListBox.Items.Refresh();

            DocumentVariables.Clear();

            radRichTextBox1.Document.Selection.SelectAll();
            radRichTextBox1.Delete(true);

            UpdateDataFields();

            ///MainGrid.InvalidateVisual();
        }

        private readonly string CurrentUser =
#if DEBUG
            @"HQMCDSOFT\Test_DEBUG";
#else
            System.Security.Principal.WindowsIdentity.GetCurrent().Name;
#endif

        public bool IsBlocked => !IsContentUnlocked;

        public bool IsUnlocked => SelectedDocument == null || !SelectedDocument.IsLocked || SelectedDocument.LockedBy == CurrentUser;

        public bool ContentLockedByGlobal => SelectedParagraph?.ParentParagraph != null;

        public bool IsContentUnlocked => IsUnlocked && !ContentLockedByGlobal;

        public bool IsEnable => DocumentIsOpen && IsUnlocked;

        public bool IsContentEnabled => DocumentIsOpen && IsContentUnlocked;

        public bool IsFilterEnabled => DocumentIsOpenAndNotFiltered && IsUnlocked;

        public bool IsConditionEnabled => ConditionBufferNotEmpty && IsUnlocked;

        private void ClearChildTablesButton()
        {
            foreach (var item in ChildTableContent.Children.OfType<TextBlock>())
            {
                item.PreviewMouseDown -= MergeTable_PreviewMouseDown;
                item.MouseEnter -= TextBlock_MouseEnter;
                item.MouseLeave -= TextBlock_MouseLeave;
            }

            ChildTableContent.Children.Clear();
        }

        private void FillChildTablesButton()
        {
            ClearChildTablesButton();

            foreach (var table in ChildTables)
            {
                var textBlock = GenerateTextBlock(table.TableName);

                textBlock.PreviewMouseDown += MergeTable_PreviewMouseDown;

                ChildTableContent.Children.Add(textBlock);
            }
        }

        private void FillGlobalTableButton()
        {
            var globalParagraphs = context.Paragraphs.Where(x => x.IsGlobal && !x.Deleted && (x.ActiveTill == null || x.ActiveTill >= DateTime.Today)).ToList();

            foreach (var paragraph in globalParagraphs)
            {
                var textBlock = GenerateTextBlock(paragraph.Name);

                textBlock.PreviewMouseDown += GlobalParagraph_PreviewMouseDown;

                GlobalContent.Children.Add(textBlock);
            }
        }

        private void GlobalParagraph_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedParagraphName = (sender as TextBlock).Text;
            var globalParagraph = context.Paragraphs.FirstOrDefault(x => x.IsGlobal && !x.Deleted && x.Name == selectedParagraphName);

            if (globalParagraph == null)
                return;

            var newParagraph = new Paragraph
            {
                Id = Guid.NewGuid(),
                Name = globalParagraph.Name,
                Level = SelectedParagraph.Level,
                DocumentContent = SelectedDocument.CurrentVersion,
                OrderNo = SelectedParagraph.OrderNo + 1,
                Deleted = false,
                Numerable = true,
                ParentParagraph = globalParagraph
            };

            SelectedDocument.Paragraph.Add(newParagraph);
            context.Paragraphs.Add(newParagraph);

            Paragraphs.Insert(SelectedParagraph.OrderNo.Value + 1, newParagraph);
            //SelectedDocument.Paragraph.Add(SelectedParagraph);
            foreach (Paragraph p in Paragraphs)
            {
                p.OrderNo = Paragraphs.IndexOf(p);
            }

            newParagraph.PassConditions = true;
            ParagraphListBox.Items.Refresh();
            SelectedParagraph = newParagraph;
        }

        private TextBlock GenerateTextBlock(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Padding = new Thickness(5)
            };

            textBlock.MouseEnter += TextBlock_MouseEnter;
            textBlock.MouseLeave += TextBlock_MouseLeave;

            return textBlock;
        }

        private void TextBlock_MouseLeave(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBlock;

            if (tb == null)
                return;

            tb.Background = null;
        }

        private void TextBlock_MouseEnter(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBlock;

            if (tb == null)
                return;

            tb.Background = Brushes.LightBlue;
        }

        private void MergeTable_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedTable = (sender as TextBlock).Text;
            var childTable = ChildTables.FirstOrDefault(x => x.TableName == selectedTable);

            var table = new TelerikModel.Table();
            table.StyleName = TelerikModel.RadDocumentDefaultStyles.DefaultTableGridStyleName;
            var row = table.AddRow();
            var columns = childTable.Columns.OfType<DataColumn>().Where(x => !x.ColumnName.Contains(Constants.IgnoreText)).ToList();

            for (var i = 0; i < columns.Count; i++)
            {
                var cell = new TelerikModel.TableCell();
                var paragraph = new TelerikModel.Paragraph();

                if (i == 0)
                    AddMergeField(paragraph, string.Format(Constants.TableStartMergeFieldName, childTable.TableName));

                AddMergeField(paragraph, columns[i].ColumnName);

                if (i == columns.Count - 1)
                    AddMergeField(paragraph, string.Format(Constants.TableEndMergeFieldName, childTable.TableName));

                cell.Blocks.Add(paragraph);
                row.Cells.Add(cell);
            }

            radRichTextBox1.InsertTable(table);
        }

        private void AddMergeField(TelerikModel.Paragraph paragraph, string propertyName)
        {
            var start = new TelerikModel.FieldRangeStart();
            var end = new TelerikModel.FieldRangeEnd
            {
                Start = start
            };

            start.Field = new TelerikModel.MergeField { PropertyPath = propertyName };
            var span = new TelerikModel.Span(string.Format(Constants.MergeField, propertyName));

            paragraph.Inlines.Add(start);
            paragraph.Inlines.Add(span);
            paragraph.Inlines.Add(end);
        }

        private void ClearChildFieldButton()
        {
            foreach (var item in ChildTableContent.Children.OfType<TextBlock>())
            {
                item.PreviewMouseDown -= MergeField_PreviewMouseDown;
                item.MouseEnter -= TextBlock_MouseEnter;
                item.MouseLeave -= TextBlock_MouseLeave;
            }

            ChildFieldContent.Children.Clear();
        }

        private void FillChildRowsButton()
        {
            ClearChildFieldButton();

            foreach (var table in ChildTables)
            {
                var startBlock = GenerateTextBlock(string.Format(Constants.TableStartMergeFieldName, table.TableName));

                startBlock.PreviewMouseDown += MergeField_PreviewMouseDown;

                ChildFieldContent.Children.Add(startBlock);

                foreach (var column in table.Columns.OfType<DataColumn>().Where(x => !x.ColumnName.Contains(Constants.IgnoreText)))
                {
                    var textBlock = GenerateTextBlock(column.ColumnName);

                    textBlock.PreviewMouseDown += MergeField_PreviewMouseDown;

                    ChildFieldContent.Children.Add(textBlock);
                }

                var endBlock = GenerateTextBlock(string.Format(Constants.TableEndMergeFieldName, table.TableName));

                endBlock.PreviewMouseDown += MergeField_PreviewMouseDown;

                ChildFieldContent.Children.Add(endBlock);
            }
        }

        private void MergeField_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedField = (sender as TextBlock).Text;

            radRichTextBox1.InsertField(new TelerikModel.MergeField { PropertyPath = selectedField });
        }

        private void Input_Keep_With_Next_Click(object sender, RoutedEventArgs e)
        {
            if (IsContentUnlocked)
                radRichTextBox1.Insert(Constants.KeepWithNext);
        }

        //private bool IsLoadChecked(EntityObject entity) => new[] { EntityState.Unchanged, EntityState.Modified }.Contains(entity.EntityState);

        private void IsGlobalButton_Checked(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph == null)
                return;

            if (SelectedParagraph.ParentParagraph != null)
                return;

            var globalParagraph = new Paragraph
            {
                Id = Guid.NewGuid(),
                Name = SelectedParagraph.Name,
                IsGlobal = true
            };

            context.Paragraphs.Add(globalParagraph);

            var contents = SelectedParagraph.ParagraphContents.ToList();

            foreach (var content in contents)
            {
                content.Paragraph = globalParagraph;
            }

            SelectedParagraph.ParentParagraph = globalParagraph;

            context.SaveChanges();

            RefreshParagraphContents();

            ParagraphListBox.Items.Refresh();
        }

        private void IsGlobalButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SelectedParagraph == null)
                return;

            if (SelectedParagraph.ParentParagraph == null)
                return;

            var parentParagraph = SelectedParagraph.ParentParagraph;

            foreach (var content in parentParagraph.ParagraphContents)
            {
                var newContent = new ParagraphContent
                {
                    Id = Guid.NewGuid(),
                    ActiveFrom = content.ActiveFrom,
                    ActiveTill = content.ActiveTill,
                    Approved = content.Approved,
                    Comment = content.Comment,
                    Condition = content.Condition,
                    Content = content.Content,
                    CreatedOn = content.CreatedOn,
                    DefaultVersion = content.DefaultVersion,
                    Deleted = content.Deleted,
                    Name = content.Name,
                    Paragraph = SelectedParagraph
                };

                context.ParagraphContents.Add(newContent);
            }

            SelectedParagraph.ParentParagraph = null;

            context.SaveChanges();

            RefreshParagraphContents();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!IsUnlocked)
                return;

            if (CheckSavingBeforeAction())
                e.Cancel = true;
        }

        private bool CheckSavingBeforeAction()
        {
            if (!CheckIsModified())
                return false;

            var res = MessageBox.Show("Настраиваемый шаблон был изменен, вы хотите сохранить изменения?", "Подтвердите действие", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (res == MessageBoxResult.Cancel)
            {
                return true;
            }
            else if (res == MessageBoxResult.Yes)
                SaveData(false);
            else if (res == MessageBoxResult.No)
                RollbackChanges();

            return false;
        }

        private bool CheckIsModified()
        {
            var entries = context.ChangeTracker.Entries().Where(x => new[] { EntityState.Modified | EntityState.Deleted | EntityState.Added }.Contains(x.State));

            if (entries.Any(x => x.State == EntityState.Deleted || x.State == EntityState.Added))
                return true;

            foreach (var entry in entries.Where(x => x.State == EntityState.Modified))
            {
                foreach (var propName in entry.GetDatabaseValues().Properties)
                {
                    if (entry.OriginalValues[propName].ToString() != entry.CurrentValues[propName].ToString())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void RollbackChanges()
        {
            var collection = context.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged);

            foreach (var e in collection)
            {
                switch (e.State)
                {
                    case EntityState.Modified:
                        e.CurrentValues.SetValues(e.OriginalValues);
                        e.State = EntityState.Unchanged;
                        break;

                    case EntityState.Added:
                        e.State = EntityState.Detached;
                        break;

                    case EntityState.Deleted:
                        e.State = EntityState.Unchanged;
                        break;
                }
            }
        }

        private void Version_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSavingBeforeAction())
                return;

            var window = new VersionWindow(context, SelectedDocument);

            window.ShowDialog();

            OpenDocument(SelectedDocument);
        }

        private void TrackChangeChecker_Checked(object sender, RoutedEventArgs e)
        {
            TrackChangeChecker.IsEnabled = false;
            VersionButton.IsEnabled = true;
        }

        private void FillParagraphList()
        {
            var withDeleted = DeletedParagraphVisible.IsChecked.GetValueOrDefault();
            var passCondition = FilterButton.IsChecked.GetValueOrDefault();

            Paragraphs = new ObservableCollection<Paragraph>(SelectedDocument.Paragraph.Where(y => (withDeleted || y.Deleted == false) && (!passCondition || y.PassConditions == true)).OrderBy(x => x.OrderNo).ToList());
        }

        private void DeletedParagraphVisible_Checked(object sender, RoutedEventArgs e)
        {
            FillParagraphList();
            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedDocument == null)
            {
                MessageBox.Show("Документ не выбран");
            }

            var window = new DiffFileWindow(new List<Document> { SelectedDocument });

            if (!window.IsClosed)
                window.ShowDialog();
        }

        private void Footnote_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedParagraphContent == null)
                return;

            var comment = new TelerikModel.Comment("ApplyFootnote");
            radRichTextBox1.InsertComment(comment);
            var commentRange = radRichTextBox1.Document.GetCommentRangeStartByComment(comment);
            radRichTextBox1.DeleteComment(commentRange);
        }

        private void IsCustomNumbering_Click(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;

            if (cb.IsChecked == true)
            {
                if (Paragraphs.First().Id == SelectedParagraph.Id)
                {
                    ShowOrHideStartNumbering(true);

                    UpdateNextParagraph(Array.FindIndex(Paragraphs.ToArray(), x => x.Id == SelectedParagraph.Id) + 1);

                    return;
                }

                var prevParagraph = Paragraphs.TakeWhile(x => x.Id != SelectedParagraph.Id).Last();

                ShowOrHideStartNumbering(!prevParagraph.IsCustomNumbering);

                SelectedParagraph.CustomNo = GetCustomNoByPrev(prevParagraph.CustomNo, prevParagraph.CustomLevel);
                SelectedParagraph.CustomLevel = prevParagraph.CustomLevel;

                UpdateNextParagraph(Array.FindIndex(Paragraphs.ToArray(), x => x.Id == SelectedParagraph.Id) + 1);

                NotifyPropertyChanged(nameof(Paragraphs));
                ParagraphListBox.Items.Refresh();
            }
            else
            {
                ShowOrHideStartNumbering(false);

                var nextParagraph = Paragraphs.SkipWhile(x => x.Id != SelectedParagraph.Id).Skip(1).FirstOrDefault();

                if (nextParagraph != null && nextParagraph.IsCustomNumbering)
                    nextParagraph.StartNumberingFrom = nextParagraph.CustomNo;

                SelectedParagraph.CustomNo = string.Empty;
                StartNumberingFrom = string.Empty;

                NotifyPropertyChanged(nameof(Paragraphs));
                ParagraphListBox.Items.Refresh();
            }
        }

        private void StartNumberingFrom_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StartNumberingFromTextBox.Text))

                if (!Regex.IsMatch(StartNumberingFromTextBox.Text, @"^([0-9]+\.?)*[0-9]+$"))
                    return;

            var maxLevel = 9;
            var dotCount = Regex.Matches(StartNumberingFromTextBox.Text, @"\.").Count;

            if (dotCount > maxLevel - 1)
                return;

            SelectedParagraph.CustomNo = StartNumberingFromTextBox.Text;
            SelectedParagraph.CustomLevel = dotCount;

            UpdateNextParagraph(Array.FindIndex(Paragraphs.ToArray(), x => x.Id == SelectedParagraph.Id) + 1);

            NotifyPropertyChanged(nameof(Paragraphs));
            ParagraphListBox.Items.Refresh();
        }

        private void StartNumberingFrom_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(IsGood);
        }

        private void SelectedParagraph_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var isFirstParagraphs = Paragraphs.First().Id == SelectedParagraph?.Id;
            var relativeParagraphs = isFirstParagraphs ? IsCustomNumbering : IsCustomNumbering && !Paragraphs.TakeWhile(x => x.Id != SelectedParagraph.Id).Last().IsCustomNumbering;

            ShowOrHideStartNumbering(relativeParagraphs);

            if (!string.IsNullOrEmpty(SelectedParagraph?.CustomNo))
                StartNumberingFrom = SelectedParagraph.CustomNo;
        }

        private void ShowOrHideStartNumbering(bool show)
        {
            if (show)
            {
                StartNumberingFromLabel.Visibility = Visibility.Visible;
                StartNumberingFromTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                StartNumberingFromLabel.Visibility = Visibility.Hidden;
                StartNumberingFromTextBox.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateNextParagraph(int ind)
        {
            if (ind == Paragraphs.Count())
                return;

            if (Paragraphs.ToArray()[ind].IsCustomNumbering)
            {
                Paragraphs.ToArray()[ind].CustomNo = GetCustomNoByPrev(Paragraphs.ToArray()[ind - 1].CustomNo, Paragraphs.ToArray()[ind - 1].CustomLevel);
                Paragraphs.ToArray()[ind].CustomLevel = Paragraphs.ToArray()[ind - 1].CustomLevel;
                Paragraphs.ToArray()[ind].StartNumberingFrom = string.Empty;

                UpdateNextParagraph(ind + 1);
            }
        }

        private string GetCustomNoByPrev(string prevCustomNo, int level)
        {
            if (string.IsNullOrEmpty(prevCustomNo))
                return string.Empty;

            var dots = Regex.Matches(prevCustomNo, @"\.").Count;
            for (var i = 0; i < level - dots; i++)
                prevCustomNo += ".0";

            var reverseCustomNo = prevCustomNo.ReverseString();

            var indexes = new List<int>();
            int index = reverseCustomNo.IndexOf('.', 0);
            while (index > -1)
            {
                indexes.Add(index);
                index = reverseCustomNo.IndexOf('.', index + 1);
            }

            var newCustomNo = (decimal.Parse(string.Join(string.Empty, prevCustomNo.Split('.'))) + 1).ToString();

            var reverseNewCustomNo = newCustomNo.ReverseString();

            indexes.ForEach(x => reverseNewCustomNo = reverseNewCustomNo.Insert(x, "."));

            var result = reverseNewCustomNo.ReverseString();

            while (result.Length > 1 && result.Substring(result.Length - 2) == ".0")
                result = result.Substring(0, result.Length - 2);

            return result;
        }

        private bool IsGood(char c)
        {
            if (c == ' ')
                return false;
            if (c >= '0' && c <= '9')
                return true;
            if (c == '.')
                return true;

            return false;
        }
    }
}
