using System;
using AutomationUtils.Utils;

namespace Autotest.Providers
{
    public class UrlProvider
    {
        public static string RestClientBaseUrl => ConfigReader.ByKey("appUrlDev");

        public static string BaseUrl
        {
            get
            {
                return EnvironmentProvider.Env switch
                {
                    "dev" => ConfigReader.ByKey("appUrlDev"),
                    _ => throw new Exception($"Unable to generate Base URL for '{EnvironmentProvider.Env}' environment")
                };
            }
        }
    }
}