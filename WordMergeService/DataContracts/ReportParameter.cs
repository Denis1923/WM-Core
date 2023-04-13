using System.Runtime.Serialization;

namespace WordMergeService.DataContracts
{
    /// <summary>
    /// Информация о параметрах документа
    /// </summary>
    [DataContract]
    public class ReportParameter
    {
        /// <summary>
        /// Название параметра
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Отображаемое имя параметра
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Тип параметра
        /// </summary>
        [DataMember]
        public string DataType { get; set; }

        /// <summary>
        /// Допускается ли значение null?. По умолчанию false
        /// </summary>
        [DataMember]
        public bool Nulable { get; set; }

        /// <summary>
        /// Список параметров, полученных из запроса
        /// </summary>
        [DataMember]
        public LabelValueEntry[] List { get; set; }

        /// <summary>
        /// Сообщение об ошибке
        /// </summary>
        [DataMember]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Последнее используемое значение
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Доступность параметра
        /// </summary>
        [DataMember]
        public bool IsVisible { get; set; }
    }
}