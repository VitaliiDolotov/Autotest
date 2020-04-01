using AutomationUtils.Extensions;
using Autotest.Extensions;
using Autotest.Pages.ThirdParty;
using Autotest.Providers;
using CrowdpicAutomation.DTO;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.SignInPage
{
    [Binding]
    class SignIn : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;
        private readonly UserDto _user;

        public SignIn(RemoteWebDriver driver, UserDto user)
        {
            _driver = driver;
            _user = user;
        }

        [Then(@"Login Page is displayed to the user")]
        public void ThenLoginPageIsDisplayedToTheUser()
        {
            var page = _driver.NowAt<Autotest.Pages.SignIn.SignInPage>();
            Assert.IsTrue(page.EmailTextbox.Displayed(), "Login Page was not opened");
        }

        [When(@"User provides the Login and Password and clicks on the login button")]
        public void WhenUserProvidesTheLoginAndPasswordAndClicksOnTheLoginButton()
        {
            //Set standard user credential details to the context if it is currently empty
            if (string.IsNullOrEmpty(_user.Email))
                UserProvider.User().CopyPropertiesTo(_user);

            var page = _driver.NowAt<Autotest.Pages.SignIn.SignInPage>();

            page.Login(_user);
        }

        [When(@"User login using facebook account")]
        public void WhenUserLoginUsingFacebookAccount()
        {
            //Set user credential details to the context
            UserProvider.FacebookUser().CopyPropertiesTo(_user);

            var page = _driver.NowAt<Autotest.Pages.SignIn.SignInPage>();

            page.FacebookButton.Click();

            var facebookLoginPage = _driver.NowAt<FacebookLoginPage>();

            facebookLoginPage.Login(_user);
        }

        [When(@"User login using google account")]
        public void WhenUserLoginUsingGoogleAccount()
        {
            //Set user credential details to the context
            UserProvider.GoogleUser().CopyPropertiesTo(_user);

            var page = _driver.NowAt<Autotest.Pages.SignIn.SignInPage>();

            page.GoogleButton.Click();

            var googleLoginPage = _driver.NowAt<GoogleLoginPage>();

            googleLoginPage.Login(_user);
        }

        [When(@"User login using twitter account")]
        public void WhenUserLoginUsingTwitterAccount()
        {
            //Set user credential details to the context
            UserProvider.TwitterUser().CopyPropertiesTo(_user);

            var page = _driver.NowAt<Autotest.Pages.SignIn.SignInPage>();

            page.TwitterButton.Click();

            var twitterLoginPage = _driver.NowAt<TwitterLoginPage>();

            twitterLoginPage.Login(_user);
        }
    }
}
