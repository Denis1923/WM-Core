using System;
using System.Windows;

namespace WordMergeUtil_Core.Assets
{
    public static class ShowMessage
    {
        public static void ShowErrorMessage(Exception ex)
        {
            ShowErrorMessage(ex, "Выполнение операции завершилось с ошибкой");
        }

        public static void ShowErrorMessage(Exception ex, string addiditionalMessage)
        {
            var msg = addiditionalMessage + ":\n" + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);

            if (Settings.Default.ShowStackTrace)
                msg += ("\n\nСтек вызовов:\n" + ex.StackTrace);

            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowErrorMessage(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void Warning(string message)
        {
            MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
