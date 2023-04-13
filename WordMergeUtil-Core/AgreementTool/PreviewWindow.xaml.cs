using System.Globalization;
using System.IO;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.Model;
using Telerik.Windows.Documents.Proofing;
using WordMergeEngine.Helpers;
using WordMergeUtil.AgreementTool.Localizations;

namespace WordMergeUtil_Core.AgreementTool
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public RadDocument Document { get; set; }

        public PreviewWindow(RadDocument aContent)
        {
            Document = aContent;

            LocalizationManager.Manager = new LocalizationManager()
            {
                ResourceManager = RadRichTextBoxResources.ResourceManager,
                Culture = new CultureInfo("ru-RU")
            };

            InitializeComponent();

            this.radRichTextBox1.Document = this.Document;
            this.radRichTextBoxRibbonUI.DataContext = this.radRichTextBox1.Commands;
            var dictionary = new RadDictionary();

            using (var ms = new MemoryStream(Properties.Resources.ru_RU))
                dictionary.Load(ms);

            ((DocumentSpellChecker)this.radRichTextBox1.SpellChecker).AddDictionary(dictionary, CultureInfo.InvariantCulture);
            LogHelper.Log(GetType().Name);
        }

        void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            this.radRichTextBoxRibbonUI.DataContext = null;
            ((DocumentSpellChecker)this.radRichTextBox1.SpellChecker).RemoveDictionary(CultureInfo.InvariantCulture);
        }
    }
}
