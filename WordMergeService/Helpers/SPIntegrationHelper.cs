using System;
using System.Configuration;
using System.ServiceModel;

namespace WordMergeService.Helpers
{
    public class SPIntegrationHelper
    {
        private WCFServiceClient GetClient()
        {
            var client = new WCFServiceClient();
            client.Endpoint.Address = new EndpointAddress(ConfigurationManager.AppSettings["SPIntegrationUrl"]);

            return client;
        }

        public void UploadToSP(string entityName, string entityId, byte[] fileContent, string fileName, string userId)
        {
            using (var client = GetClient())
            {
                client.CreateFolderTreeWithUserContext(entityName, entityId, userId);

                var uploadDocumentResult = client.UploadDocumentOverWriteWithUserContext(fileContent, fileName, entityName, entityId, true, userId);

                if (uploadDocumentResult != ConfigurationManager.AppSettings["UploadDocumentResultSuccess"])
                    throw new ApplicationException(uploadDocumentResult);
            }
        }
    }
}