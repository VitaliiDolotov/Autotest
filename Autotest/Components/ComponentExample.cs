using AutomationUtils.Component;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Components
{
    public class ComponentExample : BaseWebComponent, IWebComponent
    {
        public new By Container => By.XPath(".//div[@id='some_id']");

        [FindsBy(How = How.XPath, Using = ".//div[contains(@class,'some_class')]//input")]
        public IWebElement Element { get; set; }

        protected override By Construct()
        {
            return Container;
        }
    }
}