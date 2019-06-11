using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using SfsExtras.Extensions;
using TechTalk.SpecFlow;

namespace Autotest.Steps.InitialSteps
{
    [Binding]
    class AssertResultsFound : SpecFlowContext
    {
        private readonly RemoteWebDriver _driver;

        //Inject driver - IOC pattern
        public AssertResultsFound(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        [Then(@"results found contains '(.*)' text")]
        public void ThenResultsFoundContainsText(string searchText)
        {
            //Create instance of page - Page object pattern
            var page = _driver.NowAt<Pages.ResultsPage>();

            //Iterate thought app links on the Google search results page
            foreach (IWebElement link in page.SearchResultsLinks)
            {
                //Check content if results link is not empty
                //We need this if for case when link empty
                //in other case unexpected exception can appears
                if (!string.IsNullOrEmpty(link.Text))
                    //Check wether link contains expected text or not
                    //to be sure that correct results returned by Google
                    Assert.IsTrue(link.Text.ToLower().Contains(searchText.ToLower()),
                        $"'{searchText}' doesn't contains '{link.Text}'");
            }
        }
    }
}
