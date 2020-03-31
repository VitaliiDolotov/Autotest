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
    class EnterSearchText : SpecFlowContextWithDriver
    {
        public EnterSearchText(RemoteWebDriver driver) : base(driver) { }

        [Given(@"I have entered '(.*)' text in the search field")]
        public void GivenIHaveEnteredTextInTheSearchField(string searchText)
        {
            var page = Driver.NowAt<Pages.SearchPage>();

            page.SearchTextbox.SendKeys(searchText);
        }
    }
}
