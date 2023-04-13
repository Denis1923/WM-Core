

namespace WordMergeEngine.Assets
{
    public class ConditionException : ApplicationException
    {
        public ConditionException() : base() { }
        public ConditionException(string message) : base(message) { }
        public ConditionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
