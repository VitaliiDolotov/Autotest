using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Base
{
    [Binding]
    public class SpecFlowContextWithDriver : SpecFlowContext
    {
        protected readonly RemoteWebDriver Driver;

        public SpecFlowContextWithDriver(RemoteWebDriver driver)
        {
            Driver = driver;
        }
    }
}
