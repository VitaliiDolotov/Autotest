using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps
{
    [Binding]
    class StepExample : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public StepExample(RemoteWebDriver driver)
        {
            _driver = driver;
        }
    }
}
