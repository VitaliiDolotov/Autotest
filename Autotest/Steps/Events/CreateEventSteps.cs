using System.Threading;
using Autotest.Extensions;
using Autotest.Pages.Events;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Events
{
    [Binding]
    public class CreateEventSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public CreateEventSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [When(@"User provides the '(.*)' Event name, '(.*)' Event description and clicks on the Get started FREE button")]
        public void ThenUserProvidesTheEventNameEventDescriptionAndClicksOnTheGetStartedFREEButton(string name, string description)
        {
            var page = _driver.NowAt<CreateEvent>();
            page.EventName.SendKeys(name);
            page.EventDescription.SendKeys(description);
            Thread.Sleep(1000);
            page.GetStartedFree.Click();
        }
    }
}
