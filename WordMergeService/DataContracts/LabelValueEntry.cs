using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Параметр
    /// </summary>
    [DataContract]
    public class LabelValueEntry
    {
        /// <summary>
        /// Имя
        /// </summary>
        [DataMember]
        public string Label { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        [DataMember]
        public string Value { get; set; }
    }
}