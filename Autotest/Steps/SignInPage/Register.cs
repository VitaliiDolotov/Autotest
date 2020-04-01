using AutomationUtils.Extensions;
using Autotest.Extensions;
using Autotest.Helpers;
using Autotest.Pages.SignIn;
using Autotest.Providers;
using CrowdpicAutomation.DTO;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.SignInPage
{
    [Binding]
    class Register : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;
        private readonly UserDto _user;

        public Register(RemoteWebDriver driver, UserDto user)
        {
            _driver = driver;
            _user = user;
        }

        [When(@"User has registered new account")]
        public void WhenUserHasRegisteredNewAccount()
        {
            //Set user credential details to the context
            UserProvider.NewUser().CopyPropertiesTo(_user);

            var signInBasePage = _driver.NowAt<SignInBasePage>();

            signInBasePage.RegisterTab.Click();

            var page = _driver.NowAt<RegisterPage>();

            Assert.IsTrue(page.FacebookButton.Displayed(), "Facebook button is not displayed on Register tab");
            Assert.IsTrue(page.GoogleButton.Displayed(), "Google button is not displayed on Register tab");

            page.Register(_user);

            var completeregistrationUrl = EmailHelper.GetCompleteRegistrationUrl(_user);
            _driver.Navigate().GoToUrl(completeregistrationUrl);
        }

        [AfterScenario("Delete_Newly_Created_User")]
        public void Delete_Newly_Created_User()
        {
            DatabaseHelper.ExecuteQuery($"delete from [dbo].[Users] where [UserEmail] = '{_user.Email}'");
        }
    }
}