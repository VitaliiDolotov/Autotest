using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autotest.Providers;
using OpenQA.Selenium.Remote;
using SfsExtras.Base;
using TechTalk.SpecFlow;

namespace Autotest.Steps.Base
{
    [Binding]
    class OpenWebsite : SpecFlowContextWithDriver
    {
        public OpenWebsite(RemoteWebDriver driver) : base(driver) { }

        [Given(@"I have opened google website")]
        public void GivenIHaveOpenedGoogleWebsite()
        {
            Driver.Navigate().GoToUrl(UrlProvider.Url);
        }
    }
}
