using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Photos
{
    class FindYourPhotosPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id='searchPage']/div/p")]
        public IWebElement EventName { get; set; }
    }
}
