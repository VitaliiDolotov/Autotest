using AutomationUtils.Utils;

namespace Autotest.Providers
{
    public class EnvironmentProvider
    {
        public static string Env => ConfigReader.ByKey("environment");
    }
}