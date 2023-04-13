using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Доступные документы
    /// </summary>
    [DataContract]
    public class AvailableReport
    {
        /// <summary>
        /// Список документов
        /// </summary>
        [DataMember]
        public List<Report> reports;

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        [DataMember]
        public string Message;
    }
}