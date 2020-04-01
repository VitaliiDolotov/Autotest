using System.Threading;
using AutomationUtils.Extensions;
using Autotest.Extensions;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Header
{
    [Binding]
    class SuccessfulSignIn : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public SuccessfulSignIn(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [Then(@"User has successfully logged in")]
        public void ThenUserHasSuccessfullyLoggedIn()
        {
            Thread.Sleep(4000);

            var page = _driver.NowAt<Autotest.Pages.Header>();

            Assert.IsTrue(page.PhotosButton.Displayed(), "Photos Button is not displayed for Signed In user");
            Assert.IsTrue(page.UserProfileDropdown.Displayed(), "User Profile Dropdown is not displayed for Signed In user");
        }

        [When(@"User has logged out")]
        public void WhenUserHasLoggedOut()
        {
            var page = _driver.NowAt<Autotest.Pages.Header>();

            page.OpenProfileMenu();

            page.LogoutMenuItem.Click();

            _driver.WaitForElementToBeNotDisplayed(page.LogoutMenuItem);
            _driver.WaitForElementToBeNotEnabled(page.SigninButton);
        }

        [Then(@"User has successfully logged out")]
        public void ThenUserHasSuccessfullyLoggedOut()
        {
            var page = _driver.NowAt<Autotest.Pages.Header>();

            Assert.IsTrue(page.SigninButton.Displayed(), "Sign in button is not displayed");

            Assert.IsFalse(page.PhotosButton.Displayed(), "Photos Button is still displayed for Signed In user");
            Assert.IsFalse(page.UserProfileDropdown.Displayed(), "User Profile Dropdown is still displayed for Signed In user");
            Assert.IsFalse(page.ShoppingCartButton.Displayed(), "Shopping Cart Button is still displayed for Signed In user");
        }

        [Then(@"Correct menu items are displayed in the User Profile Menu")]
        public void ThenCorrectMenuItemsAreDisplayedInTheUserProfileMenu()
        {
            var page = _driver.NowAt<Autotest.Pages.Header>();

            page.OpenProfileMenu();

            Assert.IsTrue(page.ProfileMenuItem.Displayed(), "Profile menu item is not displayed");
            Assert.IsTrue(page.NotificationsMenuItem.Displayed(), "Notifications menu item is not displayed");
            Assert.IsTrue(page.SettingsMenuItem.Displayed(), "Settings menu item is not displayed");
            Assert.IsTrue(page.LogoutMenuItem.Displayed(), "Logout menu item is not displayed");
        }
    }
}
