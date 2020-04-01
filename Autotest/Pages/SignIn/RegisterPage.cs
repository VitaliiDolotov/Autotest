using System.Collections.Generic;
using AutomationUtils.Extensions;
using Autotest.Extensions;
using CrowdpicAutomation.DTO;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.SignIn
{
    class RegisterPage : SignInBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//input[@id='regUserEmail']")]
        public IWebElement EmailTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//input[@id='regPassword']")]
        public IWebElement PasswordTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//input[@id='regConfirmPassword']")]
        public IWebElement ConfirmPasswordTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//form[@name='registerForm']//button")]
        public IWebElement RegisterButton { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.EmailTextbox),
                SelectorFor(this, p=> p.PasswordTextbox),
                SelectorFor(this, p=> p.ConfirmPasswordTextbox),
                SelectorFor(this, p=> p.RegisterButton)
            };
        }

        public void Register(UserDto user)
        {
            if (!EmailTextbox.Displayed())
            {
                RegisterTab.Click();
                Driver.WaitForElementToBeDisplayed(RegisterButton);
            }

            EmailTextbox.SendKeys(user.Email);
            PasswordTextbox.SendKeys(user.Password);
            ConfirmPasswordTextbox.SendKeys(user.Password);

            Driver.WaitForElementToBeEnabled(RegisterButton);

            RegisterButton.Click();
        }
    }
}
