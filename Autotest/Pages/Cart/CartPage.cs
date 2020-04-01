using System.Collections.Generic;
using Autotest.Base;
using OpenQA.Selenium;
using SeleniumExtras.PageObjects;

namespace Autotest.Pages.Cart
{
    class CartPage : SeleniumBasePage
    {
        [FindsBy(How = How.XPath, Using = "//*[@id='cart']/div[1]/div[1]/h3")]
        public IWebElement EmptyCartMessadge { get; set; }

        [FindsBy(How = How.XPath, Using = "//*[@id='cart']//h1")]
        public IWebElement PageTitle { get; set; }

        public override List<By> GetPageIdentitySelectors()
        {
            return new List<By>
            {
                SelectorFor(this, p=> p.PageTitle)
            };
        }

    }
}
