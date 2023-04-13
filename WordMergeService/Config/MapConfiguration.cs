using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace WordMergeService.Config
{
    /// <summary>
    /// Класс конфигурационного файла для интеграции
    /// </summary>
    [Serializable]
    [XmlRoot(ElementName = "Configuration")]
    public class MapConfiguration
    {
        /// <summary>
        /// Настройки интеграций
        /// </summary>
        [XmlElement("Integration")]
        public List<MapIntegration> Integrations { get; set; } = new List<MapIntegration>();

        private static MapConfiguration GetConfiguration()
        {
            using (var file = File.OpenRead(ConfigurationManager.AppSettings["ConfigPath"]))
                return (MapConfiguration)new XmlSerializer(typeof(MapConfiguration)).Deserialize(file);
        }

        /// <summary>
        /// Получение настройки интеграции по коду
        /// </summary>
        /// <param name="key">Код</param>
        /// <returns></returns>
        public static MapIntegration GetIntegration(string key)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} {key}");

            var result = GetConfiguration().Integrations.FirstOrDefault(i => i.Code.ToLower() == key.ToLower());

            return result;
        }
    }
}