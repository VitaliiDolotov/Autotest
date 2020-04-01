using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationUtils.Utils;
using Autotest.Extensions;
using Autotest.Pages;
using Autotest.Providers;
using Autotest.Utils;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Base
{
    [Binding]
    class OpenWebsite : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        public OpenWebsite(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [Given(@"User is on Crowdpic Homepage")]
        public void GivenUserIsOnCrowdpicHomepage()
        {
            try
            {
                _driver.Navigate().GoToUrl(UrlProvider.Url);
            }
            catch (Exception e)
            {
                Logger.Write($"Unable to open Crowdpic website: {e}");
            }
        }

        [Then(@"User sees '(.*)' page title")]
        public void ThenUserSeesPageTitle(string titleName)
        {
            var page = _driver.NowAt<CommonElements>();
            _driver.WaitForElementToContainsText(page.PageTitle, titleName);
            Assert.AreEqual(titleName, page.PageTitle.Text);
        }
    }
}
