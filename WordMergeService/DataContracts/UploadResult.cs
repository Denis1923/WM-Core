using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Результат загрузки
    /// </summary>
    [DataContract]
    public class UploadResult
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
    }
}