using System.Runtime.Serialization;

namespace WordMergeEngine.IntegrationAssets.DataContracts
{
    [DataContract]
    public class BaseResponse
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public Dictionary<string, string> Parameters { get; set; }

        public BaseResponse()
        {
            IsSuccess = true;
            Message = string.Empty;
        }

        public BaseResponse(string message)
        {
            IsSuccess = false;
            Message = message;
        }
    }
}
