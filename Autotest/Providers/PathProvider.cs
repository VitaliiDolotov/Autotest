using AutomationUtils.Utils;

namespace Autotest.Providers
{
    public class PathProvider
    {
        public static string ScreenshotsFolder => ConfigReader.ByKey("screenshotsFolder");
        public static string DownloadsFolder => ConfigReader.ByKey("downloadsFolder");
    }
}
