using Autotest.Extensions;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.HomePage
{
    [Binding]
    class OpenSignIn : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public OpenSignIn(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [When(@"User click Sign In button on Homepage")]
        public void WhenUserClickSignInButtonOnHomepage()
        {
            var page = _driver.NowAt<Pages.Header>();
            page.SigninButton.Click();
        }
    }
}
