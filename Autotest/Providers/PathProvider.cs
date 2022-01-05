using AutomationUtils.Extensions;
using AutomationUtils.Utils;

namespace Autotest.Providers
{
    public class PathProvider
    {
        public static string ScreenshotsFolder => Config.Read.ByKey("screenshotsFolder");
        public static string DownloadsFolder => Config.Read.ByKey("downloadsFolder");
    }
}