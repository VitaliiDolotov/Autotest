using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.SignIn
{
    class SignInBasePage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//button[@aria-label='Close']")]
        public IWebElement CloseOverlay { get; set; }

        [FindsBy(How = How.XPath, Using = ".//div[@class='tabs-container']/div[text()='Sign-in']")]
        public IWebElement SigninTab { get; set; }

        [FindsBy(How = How.XPath, Using = ".//div[@class='tabs-container']/div[text()='Register']")]
        public IWebElement RegisterTab { get; set; }

        #region Social networks

        [FindsBy(How = How.XPath, Using = ".//div[@id='login-modal']//button[contains(@ng-click,'facebook')]")]
        public IWebElement FacebookButton { get; set; }

        [FindsBy(How = How.XPath, Using = ".//div[@id='login-modal']//button[contains(@ng-click,'google')]")]
        public IWebElement GoogleButton { get; set; }

        [FindsBy(How = How.XPath, Using = ".//div[@id='login-modal']//button[contains(@ng-click,'twitter')]")]
        public IWebElement TwitterButton { get; set; }

        #endregion

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.CloseOverlay),
                SelectorFor(this, p=> p.SigninTab),
                SelectorFor(this, p=> p.RegisterTab)
            };
        }
    }
}
