using Autotest.Utils;

namespace Autotest.Providers
{
    public static class BrowserProvider
    {
        /// <summary>
        /// Stored in the 'browser.target_browser' property. Possible values: chrome, firefox, internetExplorer, edge
        /// </summary>
        public static string TargetBrowser => ConfigurationReader.AppSettings("browser.target_browser");
        /// <summary>
        /// Stored in the 'browser.type' property. Possible values: local, remote
        /// </summary>
        public static string BrowserType => ConfigurationReader.AppSettings("browser.type");
        /// <summary>
        /// Stored in the 'browser.hubUri' property.
        /// </summary>
        public static string HubUri => ConfigurationReader.AppSettings("browser.hubUri");
        /// <summary>
        /// Stored in the 'browser.size' property. Possible values: default, maximized or 1936,1217
        /// </summary>
        public static string Resolution => ConfigurationReader.AppSettings("browser.size");
    }
}
