using System.Runtime.Serialization;

namespace WordMergeEngine.IntegrationAssets.DataContracts
{
    [DataContract]
    public class BaseRequest
    {
        [DataMember]
        public string Action { get; set; }

        [DataMember]
        public Dictionary<string, string> Parameters { get; set; }
    }
}
