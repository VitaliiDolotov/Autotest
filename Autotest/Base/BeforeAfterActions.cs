
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutomationUtils.Utils;
using Autotest.Extensions;
using Autotest.Providers;
using Autotest.Utils;
using BoDi;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace SfsExtras.Base
{
    [Binding]
    public class BeforeAfterActions : BaseTest
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ScenarioContext _scenarioContext;

        public BeforeAfterActions(IObjectContainer objectContainer, ScenarioContext scenarioContext)
        {
            this._objectContainer = objectContainer;
            this._scenarioContext = scenarioContext;
        }

        [BeforeScenario()]
        public void OnStartUp()
        {
            #region Do_Not_Run_With_...

            List<string> testTags = TestContext.CurrentContext.Test.Properties["Category"].Select(x => x.ToString())
                .ToList();
            LockCategory.AwaitTags(testTags);
            LockCategory.AddTags(testTags);

            #endregion

            var driverInstance = CreateBrowserDriver();

            #region Check that alive driver was created

            if (driverInstance == null)
                throw new Exception("Driver was not created");

            try
            {
                var body = driverInstance.FindElement(By.TagName("body"));
            }
            catch (Exception e)
            {
                throw new Exception($"Driver was not created: {e}");
            }

            #endregion

            //Set browser size
            if (BrowserProvider.Resolution.Equals("maximized") && !BrowserProvider.BrowserType.Equals("chrome"))
                driverInstance.Manage().Window.Maximize();

            //Set browser size
            if (BrowserProvider.Resolution.Contains(","))
                driverInstance.Manage().Window.Size =
                    new Size(int.Parse(BrowserProvider.Resolution.Split(',')[0]),
                        int.Parse(BrowserProvider.Resolution.Split(',')[1]));

            _objectContainer.RegisterInstanceAs<RemoteWebDriver>(driverInstance);
        }

        [AfterScenario(Order = 0)]
        public void TestResultsAndScreen()
        {
            try
            {
                RemoteWebDriver driver = null;

                try
                {
                    driver = _objectContainer.Resolve<RemoteWebDriver>();

                    var testStatus = GetTestStatus();
                    if (!string.IsNullOrEmpty(testStatus) && !testStatus.Equals("Passed"))
                    {
                        var testName = GetTestName();
                        if (!string.IsNullOrEmpty(testName))
                            driver.CreateScreenshot(testName);
                    }

                    Logger.Write($"Test finished on URL: {driver.Url}");
                }
                catch (Exception e)
                {
                    Logger.Write(e);
                }
            }
            catch (ObjectContainerException e)
            {
                //There are no driver in the context
                Logger.Write($"There are no driver in the context: {e}");
            }
            catch (Exception e)
            {
                Logger.Write(e);
            }
        }

        [AfterScenario(Order = 10000)]
        public void QuiteDriver()
        {
            try
            {
                List<string> testTags = TestContext.CurrentContext.Test.Properties["Category"].Select(x => x.ToString()).ToList();
                LockCategory.RemoveTags(testTags);

                RemoteWebDriver driver = _objectContainer.Resolve<RemoteWebDriver>();

                driver?.QuitDriver();
            }
            catch (ObjectContainerException e)
            {
                //There are no driver in the context
                Logger.Write($"There are no driver in the context: {e}");
            }
            catch (Exception e)
            {
                Logger.Write(e);
            }
        }

        [BeforeTestRun]
        public static void OnTestsStart() { }

        [AfterTestRun]
        public static void OnTestsComplete() { }

        private string GetTestStatus()
        {
            var testStatus = TestContext.CurrentContext.Result.Outcome.Status;
            return testStatus.ToString();
        }

        private string GetTestName()
        {
            var testName = _scenarioContext.ScenarioInfo.Title;
            return testName;
        }
    }
}
