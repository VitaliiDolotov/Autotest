using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Events
{
    class EventPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id='event']//div[2]/h2")]
        public IWebElement EventTitle{ get; set; }


        [FindsBy(How = How.XPath, Using = "//*[@id='event']//div[2]/p[2]/span")]
        public IWebElement EventCounter { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='event']/div[1]/div/div/div[2]/p[2]")]
        public IWebElement EventDate { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='event']/div[3]/div/div[2]/div/div")]
        public IList<IWebElement> Photos { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='event']//button[2]")]
        public IWebElement FindMyPhotosButton { get; set; }


        [FindsBy(How = How.XPath, Using = "//*[@id='event']//button[3]")]
        public IWebElement AddPhotosButton { get; set; }


        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
               SelectorFor(this, p=> p.EventTitle)
            };
        }
    }
}
