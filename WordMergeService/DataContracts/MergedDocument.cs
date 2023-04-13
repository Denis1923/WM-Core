using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Сформированный документ
    /// </summary>
    [DataContract]
    public class MergedDocument
    {
        /// <summary>
        /// Имя документа
        /// </summary>
        [DataMember]
        public string DocumentName { get; set; }

        /// <summary>
        /// Сформированный документ
        /// </summary>
        [DataMember]
        public byte[] Data { get; set; }

        /// <summary>
        /// Список ошибок, полученные при формировании
        /// </summary>
        [DataMember]
        public List<string> Errors { get; set; }

        /// <summary>
        /// Дополнительные параметры
        /// </summary>
        [DataMember]
        public Dictionary<string, string> ExtParams { get; set; }

        /// <summary>
        /// Определяет являются ли ошибки невыполнением проверок
        /// </summary>
        [DataMember]
        public bool IsConditionError { get; set; }
    }
}