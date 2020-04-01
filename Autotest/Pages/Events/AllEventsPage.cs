using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Events
{
    class AllEventsPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id='eventList']//input")]
        public IWebElement SerchField { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='eventList']/ul/li//h5")]
        public IList<IWebElement> Events { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.SerchField),
                SelectorFor(this, p=> p.Events)
            };
        }
    }
}
