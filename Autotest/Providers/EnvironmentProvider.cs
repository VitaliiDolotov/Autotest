using AutomationUtils.Extensions;
using AutomationUtils.Utils;

namespace Autotest.Providers
{
    public class EnvironmentProvider
    {
        public static string Env => Config.Read.ByKey("environment");
    }
}