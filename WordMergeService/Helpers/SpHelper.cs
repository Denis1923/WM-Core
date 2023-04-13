using System;
using System.Configuration;
using System.Reflection;
using System.ServiceModel;

namespace WordMergeService.Helpers
{
    public class SpHelper
    {
        private SpServiceClient GetClient()
        {
            var client = new SpServiceClient();
            client.Endpoint.Address = new EndpointAddress(ConfigurationManager.AppSettings["SPServiceUrl"]);

            return client;
        }

        public void CreateFolders(string siteUrl, string docUrl)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(siteUrl)} = {siteUrl}, {nameof(docUrl)} = {docUrl})");

            using (var client = GetClient())
            {
                var result = client.CreateFolders(siteUrl, docUrl.Replace(siteUrl, string.Empty).Substring(1));

                if (!string.IsNullOrEmpty(result))
                    throw new ApplicationException($"Не удалось создать подпапки: {result}");
            }

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(siteUrl)} = {siteUrl}, {nameof(docUrl)} = {docUrl}) отработал успешно.");
        }

        public Guid UploadToSP(string docRelativeUrl, byte[] data, string spPathUrl, bool overwrite)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(docRelativeUrl)} = {docRelativeUrl}, {nameof(data)} = {data?.Length} байт, {nameof(spPathUrl)} = {spPathUrl}, {nameof(overwrite)} = {overwrite})");

            using (var client = GetClient())
            {
                var docId = default(Guid);
                var result = client.UploadDocumentOverWrite(docRelativeUrl, data, spPathUrl, overwrite, out docId);

                if (!string.IsNullOrEmpty(result))
                    throw new ApplicationException($"Не удалось загрузить файл: {result}");

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(docRelativeUrl)} = {docRelativeUrl}, {nameof(data)} = {data?.Length} байт, {nameof(spPathUrl)} = {spPathUrl}, {nameof(overwrite)} = {overwrite}) отработал успешно. Получено: {docId}");

                return docId;
            }
        }
    }
}