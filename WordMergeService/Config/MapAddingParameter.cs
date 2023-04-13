using System;
using System.Xml.Serialization;

namespace WordMergeService.Config
{
    /// <summary>
    /// Дополнительный параметр для отправки в интеграцию
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "AddingParameter")]
    public class MapAddingParameter
    {
        /// <summary>
        /// Название в параметрах
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Поле из Отчета
        /// </summary>
        [XmlAttribute("FromReport")]
        public string FromReport { get; set; }

        /// <summary>
        /// Поле из Аудита
        /// </summary>
        [XmlAttribute("FromAudit")]
        public string FromAudit { get; set; }

        /// <summary>
        /// Индикатор добавления rowId в запрос
        /// </summary>
        [XmlAttribute("IsRowId")]
        public bool IsRowId { get; set; }
    }
}