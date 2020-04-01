using Autotest.Extensions;
using Autotest.Pages;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Footer
{
    [Binding]
    public class FooterSteps : SpecFlowContext
    {

        private readonly RemoteWebDriver _driver;

        public FooterSteps (RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [When(@"User clicks Privacy Policy button in the footer on the page")]
        public void WhenUserClicksPrivacyPolicyButtonInTheFooterOnThePage()
        {
            var page = _driver.NowAt<CommonElements>();
            page.PrivacyPolicy.Click();
        }

        [When(@"User clicks Terms of Use button in the footer on the page")]
        public void WhenUserClicksTermsOfUseButtonInTheFooterOnThePage()
        {
            var page = _driver.NowAt<CommonElements>();
            page.TermsOfUse.Click();
        }
    }
}
