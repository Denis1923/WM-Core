using IntegrationService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace IntegrationService.Operations
{
    public class SalesContractAddAgreement : IOperation
    {
        public string GetContentType() => "salesContract.additionalAgreement";

        public BaseResponse Execute(Dictionary<string, string> parameters)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

                var body = new Dictionary<string, string>
                {
                    { "externalId", "externalId" },
                    { "earchiveId", "GUID1С" },
                    { "parent", "GUIDContract" },
                    { "number", "НомерДС" },
                    { "date", "ДатаДС" },
                    { "inCharge", "ОтветственныйЭА" },
                };

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