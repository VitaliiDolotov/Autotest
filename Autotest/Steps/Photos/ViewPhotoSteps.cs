using Autotest.Extensions;
using Autotest.Pages.Photos;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Photos
{
    [Binding]
    public class EventSteps : SpecFlowContext
    {

        private readonly RemoteWebDriver _driver;

        public EventSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        } 


        [Then(@"User sees '(.*)' photo on View photo page")]
        public void ThenUserSeesPhotoOnViewPhotoPage(string photo)
        {
            var page = _driver.NowAt<ViewPhoto>();
            Assert.AreEqual(photo, page.Image.GetAttribute("src"));
        }

        [Then(@"User sees '(.*)' photographer's name in view photo menu")]
        public void ThenUserSeesPhotographerSNameInViewPhotoMenu(string photographer)
        {
            var page = _driver.NowAt<ViewPhoto>();
            Assert.AreEqual(photographer, page.Photographer.Text);
        }

        [Then(@"User sees '(.*)' date in view photo menu")]
        public void ThenUserSeesDateInViewPhotoMenu(string date)
        {
            var page = _driver.NowAt<ViewPhoto>();
            Assert.AreEqual(date, page.Date.Text);
        }
    }
}
