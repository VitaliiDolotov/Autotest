using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Photos
{
    class ViewPhoto : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id='chosenPhoto']/div[2]/div[1]/div/div/img")]
        public IWebElement Image { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='chosenPhoto']/div[1]/div/div[2]/div/div/div/div/p[1]")]
        public IWebElement Photographer { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='chosenPhoto']/div[1]/div/div[2]/div/div/div/div/p[2]")]
        public IWebElement Date { get; set; }


        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.Date)
               };
        }
    }
}
