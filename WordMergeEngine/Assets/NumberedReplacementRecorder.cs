using Aspose.Words.Replacing;

namespace WordMergeEngine.Assets
{
    class NumberedReplacementRecorder : IReplacingCallback
    {
        public int Index { get; set; } = 1;
        public ReplaceAction Replacing(ReplacingArgs args)
        {
            args.Replacement = string.Format(args.Replacement, Index++);

            return ReplaceAction.Replace;
        }
    }
}
