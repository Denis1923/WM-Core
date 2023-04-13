using IntegrationService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace IntegrationService.Operations
{
    public class LeasingContract : IOperation
    {
        public string GetContentType() => "leasingContract";

        public BaseResponse Execute(Dictionary<string, string> parameters)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

                var body = new Dictionary<string, string>
                {
                    { "externalId", "externalId" },
                    { "earchiveId", "GUID1С" },
                    { "organization", "ОрганизацияЭА" },
                    { "counterparty1", "GUID1ССторона1" },
                    { "number", "НомерДоговораЛизинга" },
                    { "date", "ДатаДоговораЭА" },
                    { "status", "СтатусЭА" },
                    { "inCharge", "ОтветственныйЭА" },
                };

                var vin = new KeyValuePair<string, string>("vin", "НомерVIN");

                if (parameters.ContainsKey(vin.Value) && !string.IsNullOrEmpty(parameters[vin.Value]))
                    body.Add(vin.Key, vin.Value);

                var result = IntegrationHelper.GetBarCode(parameters, GetContentType(), body);

                return new BaseResponse() { Parameters = result };
            }
            catch (Exception ex)
            {
                var message = $"Во время интеграции {MethodBase.GetCurrentMethod().DeclaringType.Name} произошла ошибка: {ex.Message}";

                LogHelper.GetLogger().Error(message);

                return new BaseResponse(message);
            }
        }
    }
}