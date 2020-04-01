using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Photos
{
    class UploadPhotosPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id='uploadPage']/div/p")]
        public IWebElement EventName { get; set; }
    }
}
