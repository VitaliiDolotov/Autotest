using System.Collections.Generic;
using Autotest.Base;
using Autotest.Extensions;
using CrowdpicAutomation.DTO;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.ThirdParty
{
    class GoogleLoginPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//input[@id='identifierId']")]
        public IWebElement LoginTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//div[@id='password']//input")]
        public IWebElement PasswordTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//span[text()='Next']")]
        public IWebElement NextButton { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.LoginTextbox),
                SelectorFor(this, p=> p.NextButton)
            };
        }

        public void Login(UserDto user)
        {
            LoginTextbox.SendKeys(user.Email);
            NextButton.Click();

            Driver.WaitForElementToBeNotDisplayed(LoginTextbox);

            PasswordTextbox.SendKeys(user.Password);
            NextButton.Click();
        }
    }
}
