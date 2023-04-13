using IntegrationService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace IntegrationService.Operations
{
    public class CreditHistoryProcessingConcentPEP : IOperation
    {
        public string GetContentType() => "creditHistoryProcessingConcent_PEP";

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
                    { "parent", "GUIDСПЭП" },
                    { "number", "НомерДоговораЛизинга" },
                    { "date", "ДатаДоговораЭА" },
                    { "inCharge", "ОтветственныйЭА" },
                };

                var result = IntegrationHelper.GetBarCode(parameters, GetContentType(), body, true);

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