using Aspose.Words.Fields;
using Aspose.Words;
using System.Collections;
using System.Diagnostics;
using Aspose.Words.Drawing;
using AsposeParagraph = Aspose.Words.Paragraph;
using Aspose.Words.Tables;

namespace WordMergeEngine
{
    public class FieldsHelper : DocumentVisitor
    {
        public static void ConvertFieldsToStaticText(CompositeNode compositeNode, FieldType targetFieldType)
        {
            var originalNodeText = compositeNode.ToString(SaveFormat.Text);

            var helper = new FieldsHelper(targetFieldType);
            compositeNode.Accept(helper);

            Debug.Assert(originalNodeText.Equals(compositeNode.ToString(SaveFormat.Text)), "Error: Text of the node converted differs from the original");

            foreach (Node node in compositeNode.GetChildNodes(NodeType.Any, true))
                Debug.Assert(!(node is FieldChar && ((FieldChar)node).FieldType.Equals(targetFieldType)), "Error: A field node that should be removed still remains.");
        }

        private FieldsHelper(FieldType targetFieldType)
        {
            _targetFieldType = targetFieldType;
        }

        public override VisitorAction VisitFieldStart(FieldStart fieldStart)
        {
            if (fieldStart.FieldType.Equals(_targetFieldType))
            {
                _fieldDepth++;
                fieldStart.Remove();
            }
            else
                CheckDepthAndRemoveNode(fieldStart);

            return VisitorAction.Continue;
        }

        public override VisitorAction VisitFieldSeparator(FieldSeparator fieldSeparator)
        {
            if (fieldSeparator.FieldType.Equals(_targetFieldType))
            {
                _fieldDepth--;
                fieldSeparator.Remove();
            }
            else
                CheckDepthAndRemoveNode(fieldSeparator);

            return VisitorAction.Continue;
        }

        public override VisitorAction VisitFieldEnd(FieldEnd fieldEnd)
        {
            if (fieldEnd.FieldType.Equals(_targetFieldType))
                fieldEnd.Remove();
            else
                CheckDepthAndRemoveNode(fieldEnd);

            return VisitorAction.Continue;
        }

        public override VisitorAction VisitRun(Run run)
        {
            CheckDepthAndRemoveNode(run);

            return VisitorAction.Continue;
        }

        public override VisitorAction VisitShapeEnd(Shape shape)
        {
            CheckDepthAndRemoveNode(shape);

            return VisitorAction.Continue;
        }

        public override VisitorAction VisitParagraphEnd(AsposeParagraph paragraph)
        {
            if (_fieldDepth > 0)
            {
                var nextParagraph = paragraph.NextSibling;

                while (nextParagraph != null && nextParagraph.NodeType != NodeType.Paragraph)
                    nextParagraph = nextParagraph.NextSibling;

                if (nextParagraph != null)
                {
                    while (paragraph.HasChildNodes)
                    {
                        _nodesToSkip.Add(paragraph.LastChild);
                        ((AsposeParagraph)nextParagraph).PrependChild(paragraph.LastChild);
                    }
                }

                paragraph.Remove();
            }

            return VisitorAction.Continue;
        }

        public override VisitorAction VisitTableStart(Table table)
        {
            CheckDepthAndRemoveNode(table);

            return VisitorAction.Continue;
        }

        private void CheckDepthAndRemoveNode(Node node)
        {
            if (_fieldDepth > 0 && !_nodesToSkip.Contains(node))
                node.Remove();
        }

        private int _fieldDepth = 0;
        private ArrayList _nodesToSkip = new ArrayList();
        private FieldType _targetFieldType;
    }
}
