using Aspose.Words.MailMerging;
using Aspose.Words;

namespace WordMergeEngine.Helpers
{
    public class HandleMergeFieldAlternatingRows : IFieldMergingCallback
    {
        private DocumentBuilder _builder;

        public void FieldMerging(FieldMergingArgs args)
        {
            if (args.FieldName.EndsWith("_url"))
            {
                string url;
                string displayText;

                Helper.SplitHyperlinkValue((string)args.FieldValue, out url, out displayText);

                if (_builder == null)
                    _builder = new DocumentBuilder(args.Document);

                _builder.MoveToMergeField(args.FieldName);
                _builder.InsertHyperlink(displayText, url, false);
            }
            else if (args.FieldValue is string stringValue)
            {
                if (string.IsNullOrEmpty(stringValue))
                    return;

                if (stringValue.IndexOfAny(new[] { '"', '«', '»', '“', '”' }) == -1)
                    return;

                if (_builder == null)
                    _builder = new DocumentBuilder(args.Document);

                var text = string.Empty;
                var chars = new Dictionary<char, int>
                {
                    ['"'] = 34,
                    ['“'] = 34,
                    ['”'] = 34,
                    ['«'] = 171,
                    ['»'] = 187,
                };

                _builder.MoveToMergeField(args.FieldName);

                foreach (var chr in stringValue)
                {
                    if (chars.TryGetValue(chr, out int quote))
                    {
                        _builder.Write(text);
                        text = string.Empty;
                        _builder.InsertField($"QUOTE {quote}");
                        continue;
                    }

                    text += chr;
                }

                if (!string.IsNullOrEmpty(text))
                    _builder.Write(text);
            }
        }

        public void ImageFieldMerging(ImageFieldMergingArgs args)
        {
        }
    }
}
