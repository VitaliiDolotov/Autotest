using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Events
{
    class CreateEvent : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id=\"eventName\"]")]
        public IWebElement EventName { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"eventInfo\"]/form/div[2]/div/input")]
        public IWebElement EventDate { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"description\"]")]
        public IWebElement EventDescription { get; set; }

        [FindsBy(How = How.XPath, Using = "//li[1]/div[3]/button/span[1]")]
        public IWebElement GetStartedFree { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.EventName)
            };
        }
    }
}
