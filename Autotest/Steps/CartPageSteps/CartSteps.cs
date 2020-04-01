using Autotest.Extensions;
using Autotest.Pages.Cart;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.CartPageSteps
{
    [Binding]
    public class CartSteps : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public CartSteps(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [Then(@"User sees '(.*)' on Cart page")]
        public void ThenUserSeesOnCartPage(string message)
        {
            var page = _driver.NowAt<CartPage>();
            Assert.AreEqual(message, page.EmptyCartMessadge.Text, "Message " + message + " is absent");
        }

        [Then(@"User sees '(.*)' cart page title")]
        public void ThenUserSeesCartPageTitle(string expTitle)
        {
            var page = _driver.NowAt<CartPage>();
            Assert.AreEqual(expTitle, page.PageTitle.Text, "Title " + expTitle + " is wrong");
        }
    }
}
