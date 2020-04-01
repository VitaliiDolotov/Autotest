using System.Collections.Generic;
using Autotest.Base;
using CrowdpicAutomation.DTO;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.ThirdParty
{
    class TwitterLoginPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//input[@id='username_or_email']")]
        public IWebElement LoginTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//input[@id='password']")]
        public IWebElement PasswordTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//input[@value='Sign In']")]
        public IWebElement LoginButton { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.LoginTextbox),
                SelectorFor(this, p=> p.PasswordTextbox),
                SelectorFor(this, p=> p.LoginButton)
            };
        }

        public void Login(UserDto user)
        {
            LoginTextbox.SendKeys(user.Email);
            PasswordTextbox.SendKeys(user.Password);
            LoginButton.Click();
        }
    }
}
