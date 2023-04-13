using System.ServiceModel;

namespace CRMIntegrationService
{
    [ServiceContract]
    public interface IIntegrationService
    {
        [OperationContract]
        BaseResponse Action(BaseRequest request);
    }
}
