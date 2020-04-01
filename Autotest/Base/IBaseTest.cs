using OpenQA.Selenium.Remote;

namespace Autotest.Base
{
    public interface IBaseTest
    {
        RemoteWebDriver Driver { get; set; }

        RemoteWebDriver CreateBrowserDriver();
    }
}
