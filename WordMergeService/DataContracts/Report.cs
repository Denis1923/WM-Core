using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Информация о документе
    /// </summary>
    [DataContract]
    public class Report
    {
        /// <summary>
        /// Имя документа
        /// </summary>
        [DataMember]
        public string ReportName { get; set; }

        /// <summary>
        /// Код документа
        /// </summary>
        [DataMember]
        public string ReportCode { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        [DataMember]
        public string ReportType { get; set; }

        /// <summary>
        /// Субъект документа
        /// </summary>
        [DataMember]
        public string ReportSubject { get; set; }

        /// <summary>
        /// Разрешено ли выбирать формат
        /// </summary>
        [DataMember]
        public bool IsChooseFormat { get; set; }

        /// <summary>
        /// Фильтры отображения
        /// </summary>
        [DataMember]
        public List<Filter> Filters { get; set; }
    }
}