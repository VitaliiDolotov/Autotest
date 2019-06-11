using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;
using SfsExtras.Base;

namespace Autotest.Pages
{
    class SearchPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//div/input[@role='combobox']")]
        public IWebElement SearchTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//*[@type='submit']")]
        public IWebElement SearchButton { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.SearchTextbox)
            };
        }
    }
}
