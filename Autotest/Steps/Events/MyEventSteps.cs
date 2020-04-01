using System.Collections.Generic;
using System.Linq;
using Autotest.Extensions;
using Autotest.Pages.Events;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Events
{
    [Binding]
    public class MyEventSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public MyEventSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [Then(@"User sees '(.*)' event title on My event page")]
        public void ThenUserSeesEventTitleOnMyEventPage(string expTitle)
        {
            var page = _driver.NowAt<MyEventPage>();
            Assert.AreEqual(expTitle, page.EventName.Text);
        }
        
        [Then(@"User sees event info on My event page:")]
        public void ThenUserSeesEventInfoOnMyEventPage(Table table)
        {
            IList<string> expInfo = new List<string>(table.Rows.Select(r => r["eventInfo"]).ToArray());

            var page = _driver.NowAt<MyEventPage>();

            IList<IWebElement> eventInfo = page.EventInfoElements;
            IList<string> actualInfo = new List<string>();
            foreach (IWebElement webElement in eventInfo)
            {
                actualInfo.Add(webElement.Text);
            }

            Assert.AreEqual(expInfo, actualInfo);
        }

        [Then(@"User sees '(.*)' in Total uploaded photos field on the My events page")]
        public void ThenUserSeesInTotalUploadedPhotosFieldOnTheMyEventsPage(string amount)
        {
            var page = _driver.NowAt<MyEventPage>();
            Assert.AreEqual(amount, page.TotalUploadedPhotosCounter.Text);
        }

        [Then(@"User sees '(.*)' in Pending for review field on the My events page")]
        public void ThenUserSeesInPendingForReviewFieldOnTheMyEventsPage(string amount)
        {
            var page = _driver.NowAt<MyEventPage>();
            Assert.AreEqual(amount, page.PendingForReviewCounter.Text);
        }

        [Then(@"User sees '(.*)' in Filtered photos field on the My events page")]
        public void ThenUserSeesInFilteredPhotosFieldOnTheMyEventsPage(string amount)
        {
            var page = _driver.NowAt<MyEventPage>();
            Assert.AreEqual(amount, page.FilteredPhotosCounter.Text);
        }

        [Then(@"User sees '(.*)' in Manage photos section on the My events page")]
        public void ThenUserSeesInManagePhotosSectionOnTheMyEventsPage(string amount)
        {
            var page = _driver.NowAt<MyEventPage>();
            Assert.AreEqual(amount, page.LastUpdate.Text);
        }

        [Then(@"User sees that '(.*)' photos are displayed on the My events page")]
        public void ThenUserSeesThatPhotosAreDisplayedOnTheMyEventsPage(int amount)
        {
            var page = _driver.NowAt<MyEventPage>();
            Assert.AreEqual(amount, page.PhotosList.Count);
        }
    }
}
