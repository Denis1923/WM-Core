using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace WordMergeService.Config
{
    /// <summary>
    /// Настройка интеграции
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "Integration")]
    public class MapIntegration
    {
        /// <summary>
        /// Код интеграции
        /// </summary>
        [XmlAttribute("Code")]
        public string Code { get; set; }

        /// <summary>
        /// Адрес интеграции
        /// </summary>
        [XmlElement("Url")]
        public string Url { get; set; }

        /// <summary>
        /// Названия действия в интеграции
        /// </summary>
        [XmlElement("ActionName")]
        public MapActionName ActionName { get; set; }

        /// <summary>
        /// Параметры, которые нужно добавить в набор перед отправкой
        /// </summary>
        [XmlArray(ElementName = "AddingParameters")]
        [XmlArrayItem("AddingParameter")]
        public List<MapAddingParameter> Parameters { get; set; } = new List<MapAddingParameter>();
    }
}