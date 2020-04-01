using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Events
{
    class MyEventPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id='event']//div[2]/h2")]
        public IWebElement EventName { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='event']/div[1]/div/div/div[2]/div/div[1]/p")]
        public IList<IWebElement> EventInfoElements { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"event\"]/div[1]//div[2]/button[1]")]
        public IWebElement EditEventInfoButton { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"event\"]/div[1]//div[2]/button[2]")]
        public IWebElement UploadPhotosButton { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"event\"]/div[3]/div/div[1]/b")]
        public IWebElement TotalUploadedPhotosCounter { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"event\"]/div[3]/div/div[2]/b")]
        public IWebElement PendingForReviewCounter { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"event\"]/div[3]/div/div[3]/b")]
        public IWebElement FilteredPhotosCounter { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"event\"]/div[3]/div/div[4]")]
        public IWebElement LastUpdate { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='event']/div[5]/div/div")]
        public IList<IWebElement> PhotosList { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.EventName),
                SelectorFor(this, p=> p.EditEventInfoButton)
            };
        }
    }
}
