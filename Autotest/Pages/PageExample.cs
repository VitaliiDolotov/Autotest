using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages
{
    public class PageExample : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//div")]
        public IWebElement Element { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>()
            {
                SelectorFor(this, p => p.Element)
            };
        }
    }
}