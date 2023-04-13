using Aspose.Words;
using AWParagraph = Aspose.Words.Paragraph;
using Color = System.Drawing.Color;

namespace WordMergeEngine.Assets
{
    public class ChangeHighlightColor : DocumentVisitor
    {
        public ChangeHighlightColor(Color fontColor)
        {
            mFontColor = fontColor;
        }

        ///
        /// Called when a FieldEnd node is encountered in the document.
        ///
        public override VisitorAction VisitFieldEnd(Aspose.Words.Fields.FieldEnd fieldEnd)
        {
            //Simply change font name

            ChangeFont(fieldEnd.Font);

            return VisitorAction.Continue;
        }

        ///
        /// Called when a FieldSeparator node is encountered in the document.
        ///
        public override VisitorAction VisitFieldSeparator(Aspose.Words.Fields.FieldSeparator fieldSeparator)
        {
            ChangeFont(fieldSeparator.Font);

            return VisitorAction.Continue;
        }

        ///
        /// Called when a FieldStart node is encountered in the document.
        ///
        public override VisitorAction VisitFieldStart(Aspose.Words.Fields.FieldStart fieldStart)
        {
            ChangeFont(fieldStart.Font);

            return VisitorAction.Continue;
        }

        ///
        /// Called when a Footnote end is encountered in the document.
        ///
        public override VisitorAction VisitFootnoteEnd(Footnote footnote)
        {
            ChangeFont(footnote.Font);

            return VisitorAction.Continue;
        }

        ///
        /// Called when a FormField node is encountered in the document.
        ///
        public override VisitorAction VisitFormField(Aspose.Words.Fields.FormField formField)
        {
            ChangeFont(formField.Font);

            return VisitorAction.Continue;
        }

        ///
        /// Called when a Paragraph end is encountered in the document.
        ///
        public override VisitorAction VisitParagraphEnd(AWParagraph paragraph)
        {
            ChangeFont(paragraph.ParagraphBreakFont);

            return VisitorAction.Continue;
        }

        ///
        /// Called when a Run node is encountered in the document.
        ///
        public override VisitorAction VisitRun(Run run)
        {
            ChangeFont(run.Font);

            return VisitorAction.Continue;
        }

        ///
        /// Called when a SpecialChar is encountered in the document.
        ///
        public override VisitorAction VisitSpecialChar(SpecialChar specialChar)
        {
            ChangeFont(specialChar.Font);

            return VisitorAction.Continue;
        }

        private void ChangeFont(Font font)
        {
            font.HighlightColor = Color.Empty;
            font.Color = mFontColor;
        }

        private Color mFontColor = Color.Transparent;

    }
}
