using System;
using AutomationUtils.Extensions;
using AutomationUtils.Utils;

namespace Autotest.Providers
{
    public class UrlProvider
    {
        public static string RestClientBaseUrl => Config.Read.ByKey("appUrlDev");

        public static string BaseUrl
        {
            get
            {
                return EnvironmentProvider.Env switch
                {
                    "dev" => Config.Read.ByKey("appUrlDev"),
                    _ => throw new Exception($"Unable to generate Base URL for '{EnvironmentProvider.Env}' environment")
                };
            }
        }
    }
}