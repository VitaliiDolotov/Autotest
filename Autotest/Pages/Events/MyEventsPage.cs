using System.Collections.Generic;
using Autotest.Base;
using Autotest.Extensions;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Events
{
    class MyEventsPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id=\"user-events\"]/ul/li/a/div[2]/h5")]
        public IList<IWebElement> MyEvents { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id=\"user-events\"]/div/div[1]/input")]
        public IWebElement SerarchField { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.SerarchField)
            };
        }

        public void SelecteventByName(string evenName)
        {
            string selector = $".//h5[text()='{evenName}']";
            Driver.WaitForElement(By.XPath(selector));
            Driver.FindElement(By.XPath(selector)).Click();
        }
    }
}
