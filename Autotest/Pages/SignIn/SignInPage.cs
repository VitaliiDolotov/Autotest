using System.Collections.Generic;
using Autotest.Extensions;
using CrowdpicAutomation.DTO;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.SignIn
{
    class SignInPage : SignInBasePage
    {
        [FindsBy(How = How.XPath, Using = ".//input[@id='userEmail']")]
        public IWebElement EmailTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//input[@id='password']")]
        public IWebElement PasswordTextbox { get; set; }

        [FindsBy(How = How.XPath, Using = ".//form[@name='signInForm']//button")]
        public IWebElement SigninButton { get; set; }

        [FindsBy(How = How.XPath, Using = ".//button[text()='Forgot your password?']")]
        public IWebElement ForgotYourPasswordButton { get; set; }

        [FindsBy(How = How.XPath, Using = ".//p[contains(@class,'validation')]")]
        public IList<IWebElement> ErrorMessages { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.EmailTextbox),
                SelectorFor(this, p=> p.SigninButton),
                SelectorFor(this, p=> p.ForgotYourPasswordButton)
            };
        }

        public void Login(UserDto user)
        {
            EmailTextbox.SendKeys(user.Email);
            PasswordTextbox.SendKeys(user.Password);
            Driver.WaitForElementToBeEnabled(SigninButton);
            SigninButton.Click();
            Driver.WaitForElementToBeNotDisplayed(SigninButton);
        }
    }
}
