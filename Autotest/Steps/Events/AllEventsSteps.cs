using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autotest.Extensions;
using Autotest.Pages.Events;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Events
{
    [Binding]
    public class AllEventsSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public AllEventsSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [When(@"User enters '(.*)' to the search string")]
        public void WhenUserEntersToTheSearchString(string searchWord)
        {
            var page = _driver.NowAt<AllEventsPage>();
            page.SerchField.SendKeys(searchWord);
        }

        [Then(@"User sees list of events:")]
        public void ThenUserSeesListOfEvents(Table table)
        {
            IList<string> expEvents = new List<string>(table.Rows.Select(r => r["eventName"]).ToArray());

            var page = _driver.NowAt<AllEventsPage>();

            IList<string> actEvents = new List<string>();
            foreach (IWebElement webElement in page.Events)
            {
                actEvents.Add(webElement.Text);
            }

            foreach (string expEvent in expEvents)
            {
                CollectionAssert.Contains(actEvents, expEvent);
            }
        }

        [Then(@"User sees list of filtered events:")]
        public void ThenUserSeesListOfFilteredEvents(Table table)
        {
            IList<string> expEvents = new List<string>(table.Rows.Select(r => r["eventName"]).ToArray());
            
            var page = _driver.NowAt<AllEventsPage>();
            Thread.Sleep(1000);
            IList<string> actEvents = new List<string>();
            foreach (IWebElement webElement in page.Events)
            {
                actEvents.Add(webElement.Text);
            }

            Assert.AreEqual(expEvents, actEvents);
        }

        [When(@"User clicks on the '(.*)' on the All events")]
        public void WhenUserClicksOnTheOnTheAllEvents(string eventName)
        {
            var page = _driver.NowAt<AllEventsPage>();
            IList<IWebElement> events = page.Events;

            foreach (IWebElement webElement in events)
            {
                if (eventName.Equals(webElement.Text))
                {
                    webElement.Click();
                    return;
                }
            }
            throw new NoSuchElementException($"Event {eventName} is not found");
        }


    }
}
