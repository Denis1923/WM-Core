using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CRMIntegrationService.Operations
{
    public class CreatePFParameter : IOperation
    {
        public BaseResponse Execute(Dictionary<string, string> parameters)
        {
            try
            {
                LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ()");

                var crmHelper = new CrmHelper();

                crmHelper.CreatePfParameter(parameters);

                return new BaseResponse() { };
            }
            catch (Exception ex)
            {
                var message = $"Во время интеграции c CRM {MethodBase.GetCurrentMethod().DeclaringType.Name} произошла ошибка: {ex.Message}";

                LogHelper.GetLogger().Error(message);

                return new BaseResponse(message);
            }
        }

        public string GetContentType() => "CreatePFParameter";
    }
}