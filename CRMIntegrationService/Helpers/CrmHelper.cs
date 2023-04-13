using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CRMIntegrationService.Helpers
{
    public class CrmHelper
    {
        private OrganizationServiceProxy Context { get; }

        public CrmHelper()
        {
            Context = new CrmServiceClient(ConfigurationManager.AppSettings["CrmConnectionString"]).OrganizationServiceProxy;
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public void CreatePfParameter(Dictionary<string, string> parameters)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(parameters)} = {parameters?.Count} параметров)");

            var toCreate = new Entity("vtbl_pfparameters")
            {
                ["vtbl_pfid"] = parameters["rowid"],
                ["vtbl_entity_name"] = parameters["entityname"],
                ["vtbl_pf_reportcode"] = parameters["reportcode"],
                ["vtbl_pf_datetime"] = !string.IsNullOrEmpty(parameters["ДатаФормирования"]) ? DateTime.Parse(parameters["ДатаФормирования"]) : default(DateTime?),
                ["vtbl_pf_author"] = parameters["Ответственный"],
                ["vtbl_link"] = parameters["СсылкаSP"],
                ["vtbl_name"] = parameters["auditid"]
            };

            toCreate.Id = Context.Create(toCreate);

            LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} отработал успешно ({nameof(parameters)} = {parameters?.Count} параметров)");
        }
    }
}