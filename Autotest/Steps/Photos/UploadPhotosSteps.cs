using Autotest.Extensions;
using Autotest.Pages.Photos;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Photos
{
    [Binding]
    public class UploadPhotosSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public UploadPhotosSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }


        [Then(@"User sees '(.*)' event name on the Upload photos page")]
        public void ThenUserSeesEventNameOnTheUploadPhotosPage(string titleName)
        {
            var page = _driver.NowAt<UploadPhotosPage>();
            Assert.AreEqual(titleName, page.EventName.Text);
        }
    }
}
