using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;

namespace WordMergeService.Helpers
{
    public static class IntegrationHelper
    {
        private static IntegrationServiceClient GetClient(string url)
        {
            var client = new IntegrationServiceClient();
            client.Endpoint.Address = new EndpointAddress(url);

            return client;
        }

        public static Dictionary<string, string> Integration(string url, string action, Dictionary<string, string> parameters)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(action)} = {action}, {nameof(parameters)} = {parameters?.Count})");

            using (var client = GetClient(url))
            {
                var request = new BaseRequest
                {
                    Action = action,
                    Parameters = parameters
                };

                var result = client.Action(request);

                if (!result.IsSuccess)
                    throw new ApplicationException($"Во время интеграции произошла ошибка: {result.Message}");

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(action)} = {action}, {nameof(parameters)} = {parameters?.Count}) отработал успешно.");

                return result.Parameters;
            }
        }
    }
}