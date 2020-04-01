using Autotest.Extensions;
using Autotest.Pages.Events;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Events
{
    [Binding]
    public class EventSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public EventSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }


        [Then(@"User sees '(.*)' event title on the Event page")]
        public void ThenUserSeesEventTitleOnTheEventPage(string eventTitle)
        {
            var page = _driver.NowAt<EventPage>();
            Assert.AreEqual(eventTitle, page.EventTitle.Text);
        }


        [Then(@"User sees '(.*)' in the amount and date fields of Event page")]
        public void ThenUserSeesInTheAmountFieldOfEventPage(string eventCounter)
        {
            var page = _driver.NowAt<EventPage>();
            Assert.AreEqual(eventCounter, page.EventDate.Text.Trim());
        }
        
        [Then(@"User sees '(.*)' pictures are displayed on Event page")]
        public void ThenUserSeesPicturesAreDisplayedOnEventPage(int eventAmount)
        {
            var page = _driver.NowAt<EventPage>();
            Assert.AreEqual(eventAmount, page.Photos.Count);
        }

        [When(@"User clicks '(.*)' photo on Event page")]
        public void WhenUserClicksPhotoOnEventPage(int numberOfPhoto)
        {
            var page = _driver.NowAt<EventPage>();
            page.Photos[numberOfPhoto-1].Click();
        }

        [When(@"User clicks on the FIND MY PHOTOS button on the Event page")]
        public void WhenUserClicksOnTheFINDMYPHOTOSButtonOnTheEventPage()
        {
            var page = _driver.NowAt<EventPage>();
            page.FindMyPhotosButton.Click();
        }

        [When(@"User clicks on the ADD PHOTOS button on the Event page")]
        public void WhenUserClicksOnTheADDPHOTOSButtonOnTheEventPage()
        {
            var page = _driver.NowAt<EventPage>();
            page.AddPhotosButton.Click();
        }

    }
}