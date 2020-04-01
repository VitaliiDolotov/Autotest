using System.Configuration;
using CrowdpicAutomation.DTO;

namespace Autotest.Providers
{
    public class UserProvider
    {
        public static UserDto User()
        {
            return new UserDto()
            {
                Email = ConfigurationManager.AppSettings["user.email"],
                Password = ConfigurationManager.AppSettings["user.password"]
            };
        }

        public static UserDto NewUser()
        {
            return new UserDto()
            {
                Email = ConfigurationManager.AppSettings["new.user.email"],
                Password = ConfigurationManager.AppSettings["new.user.password"]
            };
        }

        public static UserDto FacebookUser()
        {
            return new UserDto()
            {
                Email = ConfigurationManager.AppSettings["user.facebook.email"],
                Password = ConfigurationManager.AppSettings["user.facebook.password"]
            };
        }

        public static UserDto GoogleUser()
        {
            return new UserDto()
            {
                Email = ConfigurationManager.AppSettings["user.twitter.email"],
                Password = ConfigurationManager.AppSettings["user.google.password"]
            };
        }

        public static UserDto TwitterUser()
        {
            return new UserDto()
            {
                Email = ConfigurationManager.AppSettings["user.google.email"],
                Password = ConfigurationManager.AppSettings["user.twitter.password"]
            };
        }
    }
}
