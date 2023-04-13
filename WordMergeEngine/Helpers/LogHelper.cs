using log4net;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace WordMergeEngine.Helpers
{
    public static class LogHelper
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().ToString());

        public static ILog GetLogger()
        {
            return _log;
        }

        public static void Log(string className, string message = "", [CallerMemberName] string memberName = "")
        {
            GetLogger().Debug($"Метод {className}.{memberName}. Занято памяти: {GetMemorySize()}Мб. Сообщение: {(string.IsNullOrEmpty(message) ? "отсутствует" : message)}");
        }

        public static string GetMemorySize()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                return Math.Round((decimal)proc.PrivateMemorySize64 / (1024 * 1024), 2).ToString();
            }
        }
    }
}
