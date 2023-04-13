using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace IntegrationService
{
    [ServiceContract]
    public interface IIntegrationService
    {
        [OperationContract]
        BaseResponse Action(BaseRequest request);
    }
}
