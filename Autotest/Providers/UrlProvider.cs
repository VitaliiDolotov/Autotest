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
                switch (EnvironmentProvider.Env)
                {
                    case "dev":
                        {
                            return ConfigReader.ByKey("appUrlDev");
                        }
                    default:
                        {
                            throw new Exception($"Unable to generate Base URL for '{EnvironmentProvider.Env}' environment");
                        }
                }
            }
        }
    }
}
