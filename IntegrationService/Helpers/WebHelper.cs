using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace IntegrationService.Helpers
{
    public class WebHelper
    {
        public static string SendGetRequest(string url, Dictionary<string, string> parameters)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(url)}={url})");

            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);

            foreach (var parameter in parameters)
                request.AddQueryParameter(parameter.Key, parameter.Value);

            try
            {
                var response = client.Execute(request);
                if (response.ErrorException != null)
                    throw (WebException)response.ErrorException;

                var result = response.Content;

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(url)}={url}) отработал успешно");

                return result;
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    LogHelper.GetLogger().Error($"Произошла ошибка при отправке запроса: ", ex);

                    throw new ApplicationException($"Произошла ошибка при отправке запроса: {ex.Message}");
                }

                using (var responseStream = ex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        LogHelper.GetLogger().Error($"Произошла ошибка при отправке запроса: ", ex);

                        throw new ApplicationException($"Произошла ошибка при отправке запроса: {reader.ReadToEnd()}");
                    }
                }
            }
        }

        public static IRestResponse SendPostRequest(string url, Dictionary<string, object> headers, string body)
        {
            LogHelper.GetLogger().Debug($"Запущен метод {MethodBase.GetCurrentMethod().Name} ({nameof(url)}={url})");

            var client = new RestClient(url)
            {
                Authenticator = new NtlmAuthenticator()
            };

            var request = new RestRequest(Method.POST)
            {
                RequestFormat = DataFormat.Json
            };

            foreach (var header in headers)
                request.AddHeader(header.Key, header.Value.ToString());

            request.AddParameter(headers["Content-Type"].ToString(), body, ParameterType.RequestBody);

            try
            {
                var response = client.Execute(request);
                if (response.ErrorException != null)
                    throw (WebException)response.ErrorException;

                LogHelper.GetLogger().Debug($"Метод {MethodBase.GetCurrentMethod().Name} ({nameof(url)}={url}) отработал успешно");

                return response;
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    LogHelper.GetLogger().Error($"Произошла ошибка при отправке запроса: ", ex);

                    throw new ApplicationException($"Произошла ошибка при отправке запроса: {ex.Message}");
                }

                using (var responseStream = ex.Response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        LogHelper.GetLogger().Error($"Произошла ошибка при отправке запроса: ", ex);

                        throw new ApplicationException($"Произошла ошибка при отправке запроса: {reader.ReadToEnd()}");
                    }
                }
            }
        }
    }
}