
namespace WordMergeEngine.Helpers
{
    public class SourceCell
    {
        public string SourceName;
        public string CellName;
        public string SheetName;
        public int RowCount;

        public SourceCell(string source, string cell, string sheet, int rowcount)
        {
            SourceName = source;
            CellName = cell;
            SheetName = sheet;
            RowCount = rowcount;
        }
    }
}
