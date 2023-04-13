using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IntegrationService.Helpers
{
    public class FileHelper
    {
        public static void SaveMessage(string name, string body, bool isRequest)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name}");

            File.WriteAllText($"{ConfigurationManager.AppSettings[isRequest ? "PathToSaveRequest" : "PathToSaveResponse"]}\\{AggregateFileName(name)}.txt", body, Encoding.GetEncoding(1251));

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно.");
        }

        public static string AggregateFileName(string requestName)
        {
            return $"{Path.GetInvalidFileNameChars().Aggregate($"{requestName}_{DateTime.Now.ToFileTime()}", (current, c) => current.Replace(c.ToString(), string.Empty))}";
        }
    }
}