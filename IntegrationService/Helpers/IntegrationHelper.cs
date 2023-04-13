using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace IntegrationService.Helpers
{
    public static class IntegrationHelper
    {
        public static Dictionary<string, string> GetBarCode(Dictionary<string, string> parameters, string type, Dictionary<string, string> bodyAttributes, bool withPerson = false)
        {
            var body = bodyAttributes.ToDictionary(x => x.Key, y => (object)GetValue(parameters, y.Value));
            body.Add("unit", GetUnit(parameters));

            if (withPerson)
                body.Add("person", GetPerson(parameters));

            var headers = GetHeader(parameters, type);
            var serializer = new JsonSerializer();
            var bodyJson = serializer.Serialize(body);

            FileHelper.SaveMessage(type, serializer.Serialize(new { header = serializer.Serialize(headers), body = bodyJson }), true);

            var result = WebHelper.SendPostRequest(ConfigurationManager.AppSettings["IntegrationUrl"], headers, bodyJson);

            if (!result.IsSuccessful)
            {
                throw new ApplicationException($"Внешняя интеграция вернула неуспешный ответ. Статус: {result.StatusCode}, сообщение: {result.Content}");
            }

            var resultParameters = GetResponseHeader(result.Headers.ToDictionary(x => x.Name, y => y.Value));

            FileHelper.SaveMessage(type, serializer.Serialize(result.Headers.ToDictionary(x => x.Name, y => y.Value)), false);

            return resultParameters;
        }

        public static Dictionary<string, object> GetHeader(Dictionary<string, string> parameters, string type)
        {
            var contentType = $"application/{type}+json";
            var headers = new Dictionary<string, object>
            {
                { "vtbl-related-system", GetValue(parameters, "КодОрганизацииЭА") },
                { "vtbl-request-date", DateTime.Now.ToString(Constants.DateTimeFormat) },
                { "externalId", GetValue(parameters, "externalId") },
                { "Content-Type", contentType }
            };

            return headers;
        }

        public static object GetUnit(Dictionary<string, string> parameters)
        {
            return new
            {
                hrUnitCode = GetValue(parameters, "ПодразделениеЭА"),
            };
        }

        public static object GetPerson(Dictionary<string, string> parameters)
        {
            return new
            {
                fullName = GetValue(parameters, "ФИО"),
                passport = GetValue(parameters, "Паспорт")
            };
        }

        public static Dictionary<string, string> GetResponseHeader(Dictionary<string, object> header)
        {
            return new Dictionary<string, string>
            {
                { "documentId", "vtbl-documentId" },
                { "documentUrl", "vtbl-documentLocation" },
                { "barCode", "vtbl-variantId" }
            }.ToDictionary(x => x.Key, y => GetValue(header, y.Value));
        }

        private static string GetValue<T>(Dictionary<string, T> parameters, string name)
        {
            if (!parameters.ContainsKey(name))
                return string.Empty;

            if (DateTime.TryParse(parameters[name].ToString(), out DateTime date))
                return date.ToString(Constants.DateTimeFormat);

            return parameters[name].ToString();
        }
    }
}