using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autotest.Extensions;
using OpenQA.Selenium.Remote;
using SfsExtras.Base;
using TechTalk.SpecFlow;

namespace Autotest.Steps.InitialSteps
{
    [Binding]
    class ExecuteSearch : SpecFlowContextWithDriver
    {
        public ExecuteSearch(RemoteWebDriver driver) : base(driver) { }

        [When(@"I have clicked Search button")]
        public void WhenIHaveClickedSearchButton()
        {
            var page = Driver.NowAt<Pages.SearchPage>();
            Driver.WaitForElementToBeDisplayed(page.SearchButton);
            page.SearchTextbox.SendKeys(OpenQA.Selenium.Keys.Enter);
        }
    }
}
