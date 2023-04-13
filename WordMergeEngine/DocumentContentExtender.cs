namespace WordMergeEngine.Models
{
    public partial class DocumentContent
    {
        public string VersionName => $"{Version}{(DefaultVersion ? " (текущая)" : string.Empty)}";
    }
}