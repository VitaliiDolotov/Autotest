using System.Collections.Generic;
using AutomationUtils.Extensions;
using Autotest.Base;
using Autotest.Extensions;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages
{
    class Header : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//a/img[@alt='logo']")]
        public IWebElement Logog { get; set; }

        [FindsBy(How = How.XPath, Using = ".//div[@id='navbar-collapse']//a[text()='Events']")]
        public IWebElement EventsButton { get; set; }

        #region Before Sign In

        [FindsBy(How = How.XPath, Using = ".//div[@id='navbar-collapse']//a[text()='Sign in']")]
        public IWebElement SigninButton { get; set; }

        #endregion

        #region When user is logged in

        [FindsBy(How = How.XPath, Using = ".//div[@id='navbar-collapse']//a[text()='Photos']")]
        public IWebElement PhotosButton { get; set; }

        [FindsBy(How = How.XPath, Using = ".//img[@alt='User profile']")]
        public IWebElement UserProfileDropdown { get; set; }

        #region Profile Menu

        [FindsBy(How = How.XPath, Using = ".//ul[contains(@class,'dropdown-menu')]//a[text()='Profile']")]
        public IWebElement ProfileMenuItem { get; set; }

        [FindsBy(How = How.XPath, Using = ".//ul[contains(@class,'dropdown-menu')]//a[text()='Notifications']")]
        public IWebElement NotificationsMenuItem { get; set; }

        [FindsBy(How = How.XPath, Using = ".//ul[contains(@class,'dropdown-menu')]//a[text()='Settings']")]
        public IWebElement SettingsMenuItem { get; set; }

        [FindsBy(How = How.XPath, Using = ".//ul[contains(@class,'dropdown-menu')]//a[text()='Log out']")]
        public IWebElement LogoutMenuItem { get; set; }

        #endregion

        [FindsBy(How = How.XPath, Using = ".//li//img[@alt='Shopping cart']/..")]
        public IWebElement ShoppingCartButton { get; set; }

        #endregion

        [FindsBy(How = How.XPath, Using = "//*[@id='navbar-collapse']/ul[1]/li[4]/a/img")]
        public IWebElement CartIcon { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='navbar-collapse']/ul[1]/li/a")]
        public  IList<IWebElement> HeaderButtons { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='navbar-collapse']/ul[1]/li[1]/ul/li/a")]
        public IList<IWebElement> EventsItemButtons { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.Logog),
                SelectorFor(this, p=> p.EventsButton)
            };
        }

        public void OpenProfileMenu()
        {
            if (LogoutMenuItem.Displayed())
                return;

            UserProfileDropdown.Click();

            //Wait for dropdown to be displayed
            Driver.WaitForElementToBeEnabled(LogoutMenuItem);
        }
    }
}
