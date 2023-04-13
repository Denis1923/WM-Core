using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Данные документа
    /// </summary>
    [DataContract]
    public class BinaryData
    {
        /// <summary>
        /// Данные документа
        /// </summary>
        [DataMember]
        public byte[] Data { get; set; }
    }
}