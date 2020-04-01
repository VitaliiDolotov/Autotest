using Autotest.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Header
{
    [Binding]
    public class HeaderSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public HeaderSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [When(@"User presses cart icon")]
        public void WhenUserPressesCartIcon()
        {
            var page = _driver.NowAt<Autotest.Pages.Header>();
            page.CartIcon.Click();
        }

        [When(@"User clicks on the '(.*)' button on the header")]
        public void WhenUserClicksOnTheButtonOnTheHeader(string buttonName)
        {
            var page = _driver.NowAt<Autotest.Pages.Header>();
            foreach (IWebElement pageHeaderButton in page.HeaderButtons)
            {
                if (pageHeaderButton.Text.Equals(buttonName))
                {
                    pageHeaderButton.Click();
                    return;
                }
            }
            throw new NotFoundException($"Button {buttonName} is not found in the header");
        }

        [When(@"User clicks on the '(.*)' item in Events menu")]
        public void WhenUserClicksOnTheItemInEventsMenu(string itemName)
        {
            var page = _driver.NowAt<Autotest.Pages.Header>();
            foreach (IWebElement pageHeaderButton in page.EventsItemButtons)
            {
                if (pageHeaderButton.Text.Equals(itemName))
                {
                    pageHeaderButton.Click();
                    return;
                }
            }
            throw new NotFoundException($"Item {itemName} is not found in the Events");

        }

    }
}
