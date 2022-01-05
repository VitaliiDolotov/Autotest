using AutomationUtils.Extensions;
using AutomationUtils.Utils;

namespace Autotest.Providers
{
    public static class Browser
    {
        public static string Type => Config.Read.ByKey("targetBrowser");
        public static string Version => Config.Read.ByKey("browserVersion");
        public static string Platform => Config.Read.ByKey("platform");
        public static string Resolution => Config.Read.ByKey("browserSize");
        public static string HubUri => Config.Read.ByKey("hubUri");
        public static string RemoteDriver => Config.Read.ByKey("remoteDriver");
    }
}