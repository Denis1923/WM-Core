using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Результат получения шаблона
    /// </summary>
    [DataContract]
    public class GetTemplateResult
    {
        /// <summary>
        /// Индикатор успеха
        /// </summary>
        [DataMember]
        public bool Success { get; set; }

        /// <summary>
        /// Сообщение о результате операции
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Имя шаблона
        /// </summary>
        [DataMember]
        public string DocumentName { get; set; }

        /// <summary>
        /// Шаблон
        /// </summary>
        [DataMember]
        public byte[] DocumentContent { get; set; }
    }
}