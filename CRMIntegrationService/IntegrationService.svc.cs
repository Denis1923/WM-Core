using System;

namespace CRMIntegrationService
{
    public class IntegrationService : IIntegrationService
    {
        public BaseResponse Action(BaseRequest request)
        {
            try
            {
                LogHelper.GetLogger().Info($"Начало вызова сервиса. action: {request.Action}");
                var result = new ActionFactory().GetInstance(request.Action).Execute(request.Parameters);
                LogHelper.GetLogger().Info($"Результат выполнения: {result.Message}");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger().Error("Ошибка при выполнении", ex);
                return new BaseResponse($"Ошибка при выполнении: {ex.Message}");
            }
        }
    }
}
