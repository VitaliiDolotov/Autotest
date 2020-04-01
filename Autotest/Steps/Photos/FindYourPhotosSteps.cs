using Autotest.Extensions;
using Autotest.Pages.Photos;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Photos
{
    [Binding]
    public class FindYourPhotosSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public FindYourPhotosSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [Then(@"User sees '(.*)' event name on the Find your photos page")]
        public void ThenUserSeesEventNameOnTheFindYourPhotosPage(string titleName)
        {
            var page = _driver.NowAt<FindYourPhotosPage>();
            Assert.AreEqual(titleName, page.EventName.Text);
        }
    }
}
