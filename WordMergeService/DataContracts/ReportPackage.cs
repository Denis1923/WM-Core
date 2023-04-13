using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Информация о пакете документов
    /// </summary>
    [DataContract]
    public class ReportPackage
    {
        /// <summary>
        /// Имя пакета документов
        /// </summary>
        [DataMember]
        public string PackageName { get; set; }

        /// <summary>
        /// Код интеграции пакета документов
        /// </summary>
        [DataMember]
        public string PackageCode { get; set; }
    }
}