using System.Configuration;

namespace Autotest.Providers
{
    public static class Database
    {
        public static string ConnectionsString => ConfigurationManager.AppSettings["connectionsString"];
    }
}
