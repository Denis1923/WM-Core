using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WordMergeService.Config
{
    /// <summary>
    /// Дополнительный параметр для отправки в интеграцию
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "ActionName")]
    public class MapActionName
    {
        /// <summary>
        /// Брать ли название из источника данных
        /// </summary>
        [XmlAttribute("IsFromDS")]
        public bool IsFromDS { get; set; }

        /// <summary>
        /// Название действия, либо названия поля в параметрах, в котором записано действие
        /// </summary>
        [XmlText]
        public string Name { get; set; }
    }
}