using Autotest.Utils;
using OpenQA.Selenium.Remote;

namespace Autotest.Base
{
    public class BaseTest : IBaseTest
    {
        public RemoteWebDriver Driver { get; set; }

        public RemoteWebDriver CreateBrowserDriver()
        {
            return BrowserFactory.CreateDriver();
        }
    }
}