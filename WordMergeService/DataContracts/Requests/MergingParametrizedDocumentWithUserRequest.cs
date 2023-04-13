namespace WordMergeService.DataContracts.Requests
{
    public class MergingParametrizedDocumentWithUserRequest : MergingParametrizedDocumentRequest
    {
        public string UserId { get; set; }
    }
}