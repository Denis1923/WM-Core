using System;
using System.Globalization;
using System.Windows.Data;
using Telerik.Windows.Documents.FormatProviders.Xaml;

namespace WordMergeUtil_Core.AgreementTool
{
    public class SecondBlockNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            try
            {
                var textImporter = new XamlFormatProvider();
                var document = textImporter.Import(value.ToString());
                var paragraph = (Telerik.Windows.Documents.Model.Paragraph)document.Sections.First.Blocks.First;
                var text = ((Telerik.Windows.Documents.Model.Span)paragraph.Inlines.First).Text;
                return text;
            }
            catch
            {

            }
            return string.Empty;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
