using System;
using System.Diagnostics;
using System.Security.Permissions;
using System.Text;
using System.Web.Services.Protocols;

namespace WordMergeService.Helpers
{
    public class ProtocolWriter
    {
        public static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string PrepareExceptionInfo(Exception ex, bool innerException, bool onlyMessage)
        {
            var msg = new StringBuilder();

            if (onlyMessage)
            {
                if (ex.Message != null)
                    msg.Append(ex.Message.Trim());
            }
            else
                msg.Append($"Ошибка типа {ex.GetType().ToString()}: {ex.Message} \r\n{ex.StackTrace}".Trim());

            if (ex is SoapException)
                msg.Append($"\r\n{(ex as SoapException).Detail.InnerText}".Trim());

            if (innerException && ex.InnerException != null)
                msg.Append($"\r\n\r\n{PrepareExceptionInfo(ex.InnerException, innerException, onlyMessage)}");

            return msg.ToString().Trim();
        }

        public static string PrepareExceptionInfoUserFriendly(Exception ex, bool innerException)
        {
            return PrepareExceptionInfo(ex, innerException, true);
        }

        public static string PrepareExceptionInfoFull(Exception ex, bool innerException)
        {
            return PrepareExceptionInfo(ex, innerException, false);
        }

        [EventLogPermission(SecurityAction.Demand)]
        public static void WriteToEventlog(string eventlogSource, EventLogEntryType entryType, Exception ex)
        {
            WriteToEventlog(eventlogSource, entryType, PrepareExceptionInfo(ex, true, false));
        }

        [EventLogPermission(SecurityAction.Demand)]
        public static void WriteToEventlog(string eventlogSource, EventLogEntryType entryType, string message)
        {
            if (!EventLog.SourceExists(eventlogSource))
            {
                EventLog.CreateEventSource(eventlogSource, "Application");
                return;
            }

            EventLog.WriteEntry(eventlogSource, message, entryType);
        }

        public static void LogError(string message)
        {
            Log.Error(message);
        }

        public static void LogError(Exception exception)
        {
            Log.Error(PrepareExceptionInfoFull(exception, true));
        }

        public static void LogError(string message, Exception exception)
        {
            Log.Error(message, exception);
        }
    }
}