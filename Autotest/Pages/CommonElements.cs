using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages
{
    class CommonElements : SeleniumBasePage
    {
        #region Page Title
        [FindsBy(How = How.XPath, Using = ".//h1")]
        [FindsBy(How = How.CssSelector, Using = ".page-title")]
        public IWebElement PageTitle { get; set; }
        #endregion

        #region Footer elelments    
        [FindsBy(How = How.XPath, Using = "//footer//li[1]/a")]
        public IWebElement PrivacyPolicy { get; set; }

        [FindsBy(How = How.XPath, Using = "//footer//li[2]/a")]
        public IWebElement TermsOfUse { get; set; }
        #endregion

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                //There is not page title on some pages
                //SelectorFor(this, p => p.PageTitle)
            };
        }
    }
}
