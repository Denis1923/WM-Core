using WordMergeEngine.IntegrationAssets.DataContracts;

namespace WordMergeEngine.IntegrationAssets
{
    public interface IOperation
    {
        string GetContentType();

        BaseResponse Execute(Dictionary<string, string> parameters);
    }
}
