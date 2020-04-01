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
    public class MyEventsSteps : SpecFlowContext

    {
        private readonly RemoteWebDriver _driver;

        public MyEventsSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [Then(@"User sees list of events on My events page:")]
        public void ThenUserSeesListOfEventsOnMyEventsPage(Table table)
        {
            IList<string> expEvents = new List<string>(table.Rows.Select(r => r["eventName"]).ToArray());

            var page = _driver.NowAt<MyEventsPage>();
            Thread.Sleep(1000);
            IList<string> actEvents = new List<string>();
            foreach (IWebElement webElement in page.MyEvents)
            {
                actEvents.Add(webElement.Text);
            }

            foreach (string expEvent in expEvents)
            {
                CollectionAssert.Contains(actEvents, expEvent);
            }
        }

        [When(@"User types '(.*)' on search field on the My events page")]
        public void WhenUserTypesOnSearchFieldOnTheMyEventsPage(string eventName)
        {
            var page = _driver.NowAt<MyEventsPage>();
            page.SerarchField.SendKeys(eventName);
        }

        [Then(@"User sees list of filtered events on My events page:")]
        public void ThenUserSeesListOfFilteredEventsOnMyEventsPage(Table table)
        {
            IList<string> expEvents = new List<string>(table.Rows.Select(r => r["eventName"]).ToArray());

            var page = _driver.NowAt<MyEventsPage>();
            Thread.Sleep(1000);
            IList<string> actEvents = new List<string>();
            foreach (IWebElement webElement in page.MyEvents)
            {
                actEvents.Add(webElement.Text);
            }

            Assert.AreEqual(expEvents, actEvents);
        }

        [When(@"User clicks on first event in event list on the My events page")]
        public void WhenUserClicksOnFirstEventInEventListOnTheMyEventsPage()
        {
            var page = _driver.NowAt<MyEventsPage>();
            page.MyEvents[0].Click();
        }

        [When(@"User open '(.*)' event on the My events page")]
        public void WhenUserOpenEventOnTheMyEventsPage(string eventName)
        {
            var page = _driver.NowAt<MyEventsPage>();
            page.SelecteventByName(eventName);
        }
    }
}
