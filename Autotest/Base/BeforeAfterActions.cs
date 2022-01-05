using System;
using System.Collections.Generic;
using System.Linq;
using AutomationUtils.Utils;
using Autotest.DTO;
using Autotest.Extensions;
using Autotest.Utils;
using BoDi;
using NUnit.Framework;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace Autotest.Base
{
    [Binding]
    public class BeforeAfterActions : BaseTest
    {
        private readonly IObjectContainer _objectContainer;
        private readonly ScenarioContext _scenarioContext;
        private readonly TestInfo _testInfo;
        private readonly BrowsersList _browsersList;
        private Exception BeforeScenarioException { get; set; }

        public BeforeAfterActions(IObjectContainer objectContainer, ScenarioContext scenarioContext,
            TestInfo testInfo, BrowsersList browsersList)
        {
            _objectContainer = objectContainer;
            _scenarioContext = scenarioContext;
            _testInfo = testInfo;
            _browsersList = browsersList;
        }

        [BeforeScenario]
        public void OnStartUp()
        {
            try
            {
                _testInfo.StartTime = DateTime.Now;
                _testInfo.Name = GetTestName();
                _testInfo.Tags = GetTags();

                Logger.Write($"TEST STARTED: {_testInfo.Name}", Logger.LogLevel.Info);

                // LockCategory.AwaitTags(_testInfo.Tags);
                // LockCategory.AddTags(_testInfo.Name, _testInfo.Tags);

                // Create browser if not API test
                if (!_testInfo.Tags.Contains("API"))
                {
                    var driverInstance = CreateBrowserDriver();
                    _objectContainer.RegisterInstanceAs(driverInstance);
                    _browsersList.AddDriver(driverInstance);
                }
            }
            catch (Exception e)
            {
                BeforeScenarioException = e;
            }
        }

        [AfterScenario(Order = 0)]
        public void TestResultsAndScreen()
        {
            try
            {
                RemoteWebDriver driver = null;
                if (!_testInfo.Tags.Contains("API"))
                    try
                    {
                        driver = _objectContainer.Resolve<RemoteWebDriver>();
                    }
                    catch (Exception e)
                    {
                        Logger.Write($"UNABLE to get driver from context. It was closed before or doesn't exist: {e}", Logger.LogLevel.Error);
                        driver = null;
                    }

                try
                {
                    var testStatus = GetTestStatus();
                    Logger.Write($"Test status is '{testStatus}'", Logger.LogLevel.Info);
                    _testInfo.Exception = GetTestException();

                    if (GetTestStatus().Equals(TestStatus.Failed))
                    {
                        if (!string.IsNullOrEmpty(_testInfo.Name))
                        {
                            if (driver != null)
                            {
                                driver.CreateScreenshot(_testInfo.Name);
                            }
                            else
                            {
                                Logger.Write("Unable to get screenshot as no Driver in the context", Logger.LogLevel.Warning);
                            }
                        }

                    }

                    if (driver != null)
                    {
                        Logger.Write($"Test finished on: {driver.Url}", Logger.LogLevel.Info);
                    }
                }
                catch (Exception e)
                {
                    Logger.Write(e, Logger.LogLevel.Error);
                }
            }
            catch (Exception e)
            {
                Logger.Write(e, Logger.LogLevel.Error);
            }
        }

        [AfterScenario(Order = 10000)]
        public void QuitDriver()
        {
            try
            {
                if (_testInfo.Tags.Contains("API")) return;
                try
                {
                    foreach (RemoteWebDriver browser in _browsersList.GetAllBrowsers())
                    {
                        try
                        {
                            browser?.QuitDriver();
                        }
                        catch (Exception e)
                        {
                            Logger.Write($"Unable to close driver: {e}", Logger.LogLevel.Warning);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Write($"UNABLE to get driver from context. It was closed before or doesn't exist: {e}", Logger.LogLevel.Warning);
                }

                // LockCategory.RemoveTestTags(_testInfo.Name);
            }
            catch (Exception e)
            {
                Logger.Write(e);
            }
            finally
            {
                try
                {
                    Logger.Write($"TEST FINISHED: {_testInfo.Name}", Logger.LogLevel.Info);
                }
                catch { }
            }
        }

        private TestStatus GetTestStatus()
        {
            if (BeforeScenarioException != null)
            {
                return TestStatus.Failed;
            }
            return _scenarioContext.TestError == null ? TestStatus.Passed : TestStatus.Failed;
        }

        private string GetTestException()
        {
            if (BeforeScenarioException != null)
            {
                return BeforeScenarioException.Message;
            }
            try
            {
                return _scenarioContext.TestError.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetTestName()
        {
            var testName = _scenarioContext.ScenarioInfo.Title;
            var iterator = _scenarioContext.ScenarioInfo.Arguments.GetEnumerator();
            if (iterator.MoveNext())
            {
                var firstArgument = iterator.Value.ToString();
                testName = $"{testName}, {firstArgument}";
            }
            return testName;
        }

        public List<string> GetTags()
        {
            try
            {
                List<string> testTags = TestContext.CurrentContext.Test.Properties["Category"].Select(x => x.ToString()).ToList();

                // If we are not able to get nUnit tags the try to get them from SpecFlow
                if (!testTags.Any())
                {
                    testTags = _scenarioContext.ScenarioInfo.Tags.ToList();
                }

                return testTags;
            }
            catch (Exception e)
            {
                throw new Exception($"Unable to get Tags from context: {e}");
            }
        }

        public enum TestStatus
        {
            Passed,
            Failed,
            Bug
        }
    }
}