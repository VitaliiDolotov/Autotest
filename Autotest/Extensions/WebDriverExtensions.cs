using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using AutomationUtils.Extensions;
using AutomationUtils.Utils;
using Autotest.Base;
using Autotest.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.PageObjects;

namespace Autotest.Extensions
{
    public static class WebDriverExtensions
    {
        private const int NumberOfTimesToWait = 2;
        private const int WaitTimeoutSeconds = 30;
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(WaitTimeoutSeconds);
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
        private static readonly By matOptionsSelector = By.XPath(".//mat-option//*[@class='mat-option-text']");

        public static void NavigateToUrl(this RemoteWebDriver driver, string url)
        {
            Logger.Write($"Navigating to the {url}");
            driver.Navigate().GoToUrl(url);
        }

        public static By GetByFor<T>(string element)
        {
            var propertyName = element;
            var type = typeof(T);
            var property = type.GetProperty(propertyName);
            var findsByAttributes =
                property.GetCustomAttributes(typeof(FindsByAttribute), false).Single() as FindsByAttribute;
            return ByFactory.From(findsByAttributes);
        }

        public static T NowAt<T>(this RemoteWebDriver driver) where T : SeleniumBasePage, new()
        {
            var page = new T { Driver = driver, Actions = new Actions(driver) };
            driver.WaitForLoadingElements(page, null);
            page.InitElements();
            return page;
        }

        public static T NowAtWithoutWait<T>(this RemoteWebDriver driver) where T : SeleniumBasePage, new()
        {
            var page = new T { Driver = driver, Actions = new Actions(driver) };
            page.InitElements();
            return page;
        }

        //For Datepicker tooltip is already on the page. No need to hover
        public static IList<IWebElement> GetDatepickerTooltipElements(this RemoteWebDriver driver, int dayNumber)
        {
            var selector = By.XPath($".//td[@role='button']//div[text()='{dayNumber}']/span/span");
            return driver.FindElements(selector);
        }

        public static string GetTooltipBubbleText(this RemoteWebDriver driver)
        {
            var by = By.XPath(_toolTipBubbleSelector);
            if (!driver.IsElementDisplayed(by, WaitTime.Short))
                return string.Empty;

            return driver.FindElement(by).Text;
        }

        //Pay attention that there are two different method that work with two different type of tooltips
        public static string GetTooltipText(this RemoteWebDriver driver)
        {
            driver.WhatForElementToBeExists(By.XPath(_toolTipSelector));
            var toolTips = driver.FindElements(By.XPath(_toolTipSelector));
            if (!toolTips.Any())
                throw new Exception("Tool tip was not displayed");
            var toolTipText = toolTips.First().FindElement(By.XPath("./div")).Text;
            if (String.IsNullOrEmpty(toolTipText))
            {
                driver.WaitForElementToBeDisplayed(By.XPath(_toolTipSelector + "/div"), 6);
                toolTipText = toolTips.First().FindElement(By.XPath("./div")).Text;
            }
            return toolTipText;
        }

        public static string CreateScreenshot(this RemoteWebDriver driver, string fileName)
        {
            try
            {
                FileSystemHelper.EnsureScreensotsFolderExists();
                var formatedFileName =
                    fileName.Replace("\\", string.Empty).Replace("/", string.Empty).Replace("\"", "'");
                var filePath = FileSystemHelper.GetPathForScreenshot(formatedFileName);
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();

                screenshot.SaveAsFile(filePath, ScreenshotImageFormat.Png);
                Logger.Write($"Check screenshot by folklowing path: {filePath}");
                return filePath;
            }
            catch (Exception e)
            {
                Logger.Write($"Unable to get screenshot: {e.Message}");
                return string.Empty;
            }
        }

        public static void QuitDriver(this RemoteWebDriver driver)
        {
            //Suggested workaround to solve issue with not closed browsers after tests
            //TODO Try to remove and test without it
            Thread.Sleep(3000);

            try
            {
                driver.Manage().Cookies.DeleteAllCookies();
            }
            catch (Exception e)
            {
                Logger.Write($"Error during cookie deleting: {e}");
            }

            try
            {
                driver.Close();
            }
            catch (Exception e)
            {
                Logger.Write($"Error during driver closing: {e}");
            }

            try
            {
                driver.Quit();
            }
            catch (Exception e)
            {
                Logger.Write($"Error on driver quite: {e}");
                try
                {
                    Thread.Sleep(3000);
                    Logger.Write("Retrying to quite chromedriver");
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    Logger.Write($"Driver was not quite on retry: {ex}");
                }
            }

            try
            {
                driver.Dispose();
            }
            catch (Exception e)
            {
                Logger.Write($"Error disposing webdriver: {e}");
            }
        }

        public static void OpenInNewTab(this RemoteWebDriver driver, string url)
        {
            driver.ExecuteScript($"window.open('{url}','_blank');");
            driver.SwitchTo().Window(driver.WindowHandles.Last());
        }

        public static void WaitForLoadingElements(this RemoteWebDriver driver, SeleniumBasePage page, By bySelector)
        {
            var bys = bySelector != null ? new List<By> { bySelector } : page.GetPageIdentitySelectors();

            foreach (var by in bys)
                driver.WaitForElement(by);

            page.InitElements();
        }

        public static void WaitForElement(this RemoteWebDriver driver, By by)
        {
            var attempts = NumberOfTimesToWait;
            while (attempts > 0)
                try
                {
                    attempts--;
                    ExecuteWithLogging(() => FluentWait.Create(driver)
                        .WithTimeout(WaitTimeout)
                        .WithPollingInterval(PollingInterval)
                        .Until(ExpectedConditions.ElementIsVisible(by)), by);

                    return;
                }

                // System.InvalidOperationException : Error determining if element is displayed (UnexpectedJavaScriptError)
                catch (InvalidOperationException e)
                {
                    Logger.Write("Following Exception is occured in the WaitForElement method: {0}", e.Message);
                    Thread.Sleep(200);
                }

                // System.InvalidOperationException :The xpath expression './/option[contains(text(),'xxx')]' cannot be evaluated or does notresult in a WebElement
                catch (InvalidSelectorException e)
                {
                    Logger.Write("Following Exception is occured in the WaitForElement method: {0}", e.Message);
                    Thread.Sleep(200);
                }
                catch (Exception e)
                {
                    throw new Exception($"Error waiting element by '{by}' : {e.Message}");
                }
        }

        private static void ExecuteWithLogging(Action actionToExecute, By by)
        {
            try
            {
                actionToExecute();
            }
            catch (Exception)
            {
                Logger.Write($"Error while wating for {by}");

                throw;
            }
        }

        public static void WaitForDataLoading(this RemoteWebDriver driver)
        {
            WaitForDataToBeLoaded(driver, ".//div[contains(@class,'spinner') and not(contains(@class,'small'))]", WaitTimeout);
        }

        public static void WaitForDataLoading(this RemoteWebDriver driver, int timeoutInSeconds)
        {
            if (timeoutInSeconds <= 0)
                throw new Exception("timeoutInSeconds should be positive value");
            WaitForDataToBeLoaded(driver, ".//div[contains(@class,'spinner') and not(contains(@class,'small'))]", TimeSpan.FromSeconds(timeoutInSeconds));
        }

        public static void WaitForDataLoadingInActionsPanel(this RemoteWebDriver driver)
        {
            WaitForDataToBeLoaded(driver, ".//div[contains(@class,'action-progress')]", WaitTimeout);
        }

        public static void WaitForDataLoadingInTopBarOnItemDetailsPage(this RemoteWebDriver driver)
        {
            WaitForDataToBeLoaded(driver, ".//div[contains(@class,'topbar-loader')]", WaitTimeout);
        }

        public static void WaitForDataLoadingOnProjects(this RemoteWebDriver driver)
        {
            WaitForDataToBeLoaded(driver, ".//div[@id='ajaxProgressMessage']/img", WaitTimeout);
        }

        private static void WaitForDataToBeLoaded(RemoteWebDriver driver, string loadingSpinnerSelector, TimeSpan timeout)
        {
            //Small sleep for Spinner waiting
            Thread.Sleep(400);

            var by = By.XPath(loadingSpinnerSelector);

            DataLoadingWaiter(driver, by, timeout);

            //Wait for second spinner
            try
            {
                WebDriverWait waitForDisplay = new WebDriverWait(driver, TimeSpan.FromMilliseconds(550));
                waitForDisplay.Until(ElementIsInDisplayedCondition(by, true));
            }
            catch
            {
                //Loading spinner is not displayed for second time
                return;
            }

            DataLoadingWaiter(driver, by, timeout);
        }

        private static void DataLoadingWaiter(RemoteWebDriver driver, By by, TimeSpan timeout)
        {
            var wasLoadingSpinnerDisplayed = driver.IsElementDisplayed(by);
            var totalSeconds = Convert.ToInt32(timeout.TotalSeconds);

            if (wasLoadingSpinnerDisplayed)
            {
                try
                {
                    WaitForElementsToBeNotDisplayed(driver, by, totalSeconds);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Loading spinner is displayed longer that {totalSeconds} seconds: {driver.Url}. {e}");
                }
            }
        }

        public static void CheckConsoleErrors(this RemoteWebDriver driver)
        {
            //Bug in chromedriver. Need to wait for fix
            //var errorsList = new List<LogEntry>();
            //foreach (var entry in driver.Manage().Logs.GetLog(LogType.Browser).ToList())
            //    if (entry.Level == LogLevel.Severe)
            //        errorsList.Add(entry);
            //Verify.IsEmpty(errorsList, "Error message is displayed in the console");
        }

        public static void CheckConsoleErrors(this RemoteWebDriver driver, string expectedConsoleError)
        {
            //Bug in chromedriver. Need to wait for fix
            //var errorsList = new List<LogEntry>();
            //foreach (var entry in driver.Manage().Logs.GetLog(LogType.Browser).ToList())
            //    if (entry.Level == LogLevel.Severe)
            //        errorsList.Add(entry);

            //Verify.IsTrue(errorsList.All(x => x.Message.Contains(expectedConsoleError)),
            //    $"Unexpected errors are displayed in the console: {string.Concat(", ", errorsList.Select(x => x.Message))}");
        }

        #region Web element extensions

        public static void SelectCustomSelectbox(this RemoteWebDriver driver, IWebElement selectbox, string option)
        {
            selectbox.Click();
            //Small wait for dropdown display
            Thread.Sleep(500);

            //TODO: [Yurii Timchenko] commented code below doesn't work on 6 Dec 2018. Temporary fixed below, will be rewritten when new filters functionality is ready (per K. Kim's answer)
            //var options = driver.FindElements(By.XPath(
            //".//div[contains(@class,'mat-autocomplete-panel mat-autocomplete-visible ng-star-inserted')]/mat-option"));
            var options = driver.FindElements(By.XPath(
                "//div[contains(@class,'mat-select-panel mat-primary')]/mat-option"));

            if (!options.Any())
            {
                options = driver.FindElements(By.XPath(
                    "//mat-option[@class='mat-option ng-star-inserted']"));
                if (!options.Any())
                    throw new Exception($"Filter options were not loaded, unable to select '{option}'");
            }

            driver.MouseHover(options.Last());
            //options = driver.FindElements(By.XPath(
            //".//div[contains(@class,'mat-select-content ng-trigger ng-trigger-fadeInContent')]"));
            driver.ClickByJavascript(options.First(x => x.Text.ContainsText(option)));
        }

        #endregion Web element extensions

        /// <summary>
        ///     Execute this method after some actions on page to get sent requests
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public static List<string> GetAllRequests(this RemoteWebDriver driver)
        {
            var allRequests = new List<string>();
            var scriptToExecute =
                "var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntries() || {}; return network;";
            var netData = driver.ExecuteScript(scriptToExecute);
            var collection = (IList)netData;
            foreach (object o in collection)
            {
                var innerCollection = (Dictionary<string, object>)o;
                foreach (KeyValuePair<string, object> keyValuePair in innerCollection)
                    if (keyValuePair.Key.Equals("name") && !string.IsNullOrEmpty(keyValuePair.Value.ToString()) &&
                        keyValuePair.Value.ToString().Contains("http"))
                        allRequests.Add(keyValuePair.Value.ToString());
            }

            return allRequests;
        }

        private static string _toolTipSelector = ".//mat-tooltip-component";
        private static string _toolTipBubbleSelector = ".//div[contains(@class,'ag-tooltip')]";

        public static bool IsTooltipDisplayed(this RemoteWebDriver driver)
        {
            var toolTips = driver.FindElements(By.XPath(_toolTipSelector));
            var toolTipBubbles = driver.FindElements(By.XPath(_toolTipBubbleSelector));

            return toolTips.Count > 0 || toolTipBubbles.Count > 0;
        }

        public static void WhatForElementToBeSelected(this RemoteWebDriver driver, IWebElement element, bool selectorState)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, WaitTimeout);
                wait.Until(ExpectedConditions.ElementToBeSelected(element, selectorState));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception(
                    $"Element '{element}' not switched to '{selectorState}' selected state in {WaitTimeout.TotalSeconds} seconds",
                    e);
            }
        }

        #region Actions

        public static void MouseHover(this RemoteWebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(element).Perform();
        }

        public static void MouseHover(this RemoteWebDriver driver, By by)
        {
            var element = driver.FindElement(by);
            Actions action = new Actions(driver);
            action.MoveToElement(element).Perform();
        }

        public static void ClickByActions(this RemoteWebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.Click(element).Perform();
        }

        public static void ClickElementLeftCenter(this RemoteWebDriver driver, IWebElement element)
        {
            var width = element.Size.Width;
            var height = element.Size.Height;
            Actions action = new Actions(driver);
            action.MoveToElement(element, width / 4, height / 2).Click().Build().Perform();
        }

        public static void DoubleClick(this RemoteWebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.DoubleClick(element).Build().Perform();
        }

        public static void ContextClick(this RemoteWebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.ContextClick(element).Build().Perform();
        }

        public static void HoverAndClick(this RemoteWebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(element).Click(element).Perform();
        }

        public static void MoveToElement(this RemoteWebDriver driver, IWebElement element)
        {
            Actions action = new Actions(driver);
            action.MoveToElement(element).Perform();
        }

        public static void DragAndDrop(this RemoteWebDriver driver, IWebElement elementToBeMoved,
            IWebElement moveToElement)
        {
            Actions action = new Actions(driver);
            action.DragAndDrop(elementToBeMoved, moveToElement).Perform();
        }

        public static void InsertFromClipboard(this RemoteWebDriver driver, IWebElement textbox)
        {
            Actions action = new Actions(driver);
            //TODO: below code stopped work on Aug 13 2019; splitted into 2 rows
            //action.Click(textbox).SendKeys(Keys.Shift + Keys.Insert).Build()
            //.Perform();
            action.Click(textbox);
            textbox.SendKeys(Keys.Shift + Keys.Insert);

            action.KeyUp(Keys.Shift).Build().Perform();
        }

        public static void SearchOnPage(this RemoteWebDriver driver)
        {
            Actions action = new Actions(driver);
            action.KeyDown(Keys.Control).SendKeys("F").Build().Perform();
            action.KeyUp(Keys.Control).Build().Perform();
        }

        #endregion Actions

        #region Actions with Javascript

        public static void MouseHoverByJavascript(this RemoteWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            ex.ExecuteScript("arguments[0].scrollIntoView(true);", element);
        }

        public static void SetAttributeByJavascript(this RemoteWebDriver driver, IWebElement element, string attribute,
            string text)
        {
            IJavaScriptExecutor ex = driver;
            ex.ExecuteScript($"arguments[0].setAttribute('{attribute}', '{text}')", element);
        }

        public static String GetNetworkLogByJavascript(this RemoteWebDriver driver)
        {
            String scriptToExecute = "var performance = window.performance  || window.mozPerformance  || window.msPerformance  || window.webkitPerformance || {}; var network = performance.getEntries() || {}; return JSON.stringify(network);";
            IJavaScriptExecutor ex = driver;
            var netData = ex.ExecuteScript(scriptToExecute);
            return netData.ToString();
        }

        public static bool IsElementHaveVerticalScrollbar(this RemoteWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            bool result = (bool)ex.ExecuteScript("return arguments[0].scrollHeight > arguments[0].clientHeight", element);
            return result;
        }

        public static bool IsElementHaveHorizontalScrollbar(this RemoteWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            bool result = (bool)ex.ExecuteScript("return arguments[0].scrollHeight > arguments[0].clientHeight", element);
            return result;
        }

        public static void ScrollGridToTheEnd(this RemoteWebDriver driver, IWebElement gridElement)
        {
            IJavaScriptExecutor ex = driver;

            var clientHeight = int.Parse(ex.ExecuteScript("return arguments[0].clientHeight", gridElement).ToString());
            if (clientHeight <= 0)
                throw new Exception("Unable to get client Height");
            var scrollHeight = int.Parse(ex.ExecuteScript("return arguments[0].scrollHeight", gridElement).ToString());

            for (int i = 0; i < scrollHeight / clientHeight; i++)
            {
                ex.ExecuteScript($"arguments[0].scrollTo(0,{clientHeight * i});", gridElement);
            }
            //Final scroll to get to the grid bottom
            ex.ExecuteScript($"arguments[0].scrollTo(0,{scrollHeight});", gridElement);
        }

        public static List<string> GetElementAttributes(this RemoteWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = driver;
            var attributesAndValues = (Dictionary<string, object>)ex.ExecuteScript("var items = { }; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", element);
            var attributes = attributesAndValues.Keys.ToList();
            return attributes;
        }

        public static string GetSelectedText(this RemoteWebDriver driver)
        {
            return ((IJavaScriptExecutor)driver).ExecuteScript("return window.getSelection().toString()").ToString();
        }

        #endregion Actions with Javascript

        #region JavaSctipt Alert

        public static void AcceptAlert(this RemoteWebDriver driver)
        {
            driver.SwitchTo().Alert().Accept();
        }

        public static void DismissAlert(this RemoteWebDriver driver)
        {
            driver.SwitchTo().Alert().Dismiss();
        }

        public static bool IsAlertPresent(this RemoteWebDriver driver)
        {
            IAlert alert = ExpectedConditions.AlertIsPresent().Invoke(driver);
            return (alert != null);
        }

        #endregion

        public static void SelectMatSelectbox(this RemoteWebDriver driver, IWebElement selectbox, string option)
        {
            //In case selectbox is already opened
            try
            {
                selectbox.Click();
            }
            catch { }
            foreach (IWebElement optionElement in GetOptionsFromMatSelectbox(driver, selectbox))
            {
                if (optionElement.Text.Equals(option))
                {
                    optionElement.Click();
                    break;
                }
            }
        }

        public static List<IWebElement> GetOptionsFromMatSelectbox(this RemoteWebDriver driver, IWebElement selectbox)
        {
            if (!driver.IsElementDisplayed(matOptionsSelector))
            {
                selectbox.Click();
                driver.WaitForElementsToBeDisplayed(matOptionsSelector);
            }
            return driver.FindElements(matOptionsSelector).ToList();
        }

        #region Wait for Element to be (not) Exists

        public static void WhatForElementToBeNotExists(this RemoteWebDriver driver, By by, int waitTimeout = WaitTimeoutSeconds)
        {
            WhatForElementToBeInExistsCondition(driver, by, false, waitTimeout);
        }

        public static void WhatForElementToBeNotExists(this RemoteWebDriver driver, IWebElement element, int waitTimeout = WaitTimeoutSeconds)
        {
            WhatForElementToBeInExistsCondition(driver, element, false, waitTimeout);
        }

        public static void WhatForElementToBeExists(this RemoteWebDriver driver, By by, int waitTimeout = WaitTimeoutSeconds)
        {
            WhatForElementToBeInExistsCondition(driver, by, true, waitTimeout);
        }

        public static void WhatForElementToBeExists(this RemoteWebDriver driver, IWebElement element, int waitTimeout = WaitTimeoutSeconds)
        {
            WhatForElementToBeInExistsCondition(driver, element, true, waitTimeout);
        }

        private static void WhatForElementToBeInExistsCondition(this RemoteWebDriver driver, By by, bool expectedCondition, int waitTimeout)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTimeout));
                wait.Until(ElementExists(by, expectedCondition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception(
                    $"Element located '{by}' by selector was not in '{expectedCondition}' Exists condition after {waitTimeout} seconds", e);
            }
        }

        private static void WhatForElementToBeInExistsCondition(this RemoteWebDriver driver, IWebElement element, bool expectedCondition, int waitTimeout)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitTimeout));
                wait.Until(ElementExists(element, expectedCondition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception(
                    $"Element was not in '{expectedCondition}' Exists condition after {waitTimeout} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ElementExists(IWebElement element, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var existsState = IsElementExists(driver, element);
                    return existsState.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementExists(By selector, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var existsState = IsElementExists(driver, driver.FindElement(selector));
                    return existsState.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Exists in Element

        public static void WaitForElementToBeNotExists(this RemoteWebDriver driver, IWebElement element, By selector, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementInElementExistsCondition(driver, element, selector, false, waitSeconds);
        }

        public static void WaitForElementInElementToBeExists(this RemoteWebDriver driver, IWebElement element, By selector, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementInElementExistsCondition(driver, element, selector, true, waitSeconds);
        }

        private static void WaitForElementInElementExistsCondition(this RemoteWebDriver driver, IWebElement element, By selector, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementInElementIsInExistsCondition(element, selector, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ElementInElementIsInExistsCondition(IWebElement element, By selector, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.IsElementExists(selector).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as element was staled
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for ElementS to be (not) Exists

        public static void WaitForElementsToBeExists(this RemoteWebDriver driver, By by, int waitSeconds = WaitTimeoutSeconds, bool allElements = true)
        {
            WaitForElementsExistsCondition(driver, by, true, waitSeconds);
        }

        public static void WaitForElementsToBeExists(this RemoteWebDriver driver, IList<IWebElement> elements, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementsExistsCondition(driver, elements, true, waitSeconds);
        }

        public static void WaitForElementsToBeNotExists(this RemoteWebDriver driver, By by, int waitSeconds = WaitTimeoutSeconds, bool allElements = true)
        {
            WaitForElementsExistsCondition(driver, by, false, waitSeconds);
        }

        public static void WaitForElementsToBeNotExists(this RemoteWebDriver driver, IList<IWebElement> elements, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementsExistsCondition(driver, elements, false, waitSeconds);
        }

        private static void WaitForElementsExistsCondition(this RemoteWebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ExistsConditionOfElementsLocatedBy(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{by}' selector were not changed Exists condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementsExistsCondition(this RemoteWebDriver driver, IList<IWebElement> elements, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ExistsConditionOfElementsLocatedBy(elements, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Not all from {elements.Count} elements were not changed Exists condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ExistsConditionOfElementsLocatedBy(By locator, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    return elements.All(element => IsElementExists(driver, element).Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> ExistsConditionOfElementsLocatedBy(IList<IWebElement> elements, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.All(element => IsElementExists(driver, element).Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
            };
        }

        #endregion

        #region Wait for ElementS to be (not) Displayed

        public static void WaitForElementsToBeNotDisplayed(this RemoteWebDriver driver, By by, int waitSeconds = WaitTimeoutSeconds, bool allElements = true)
        {
            if (allElements)
                WaitForElementsDisplayCondition(driver, by, false, waitSeconds);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, by, false, waitSeconds);
        }

        public static void WaitForElementsToBeNotDisplayed(this RemoteWebDriver driver, IList<IWebElement> elements, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementsDisplayCondition(driver, elements, false, waitSeconds);
        }

        public static void WaitForElementsToBeDisplayed(this RemoteWebDriver driver, By by, int waitSeconds = WaitTimeoutSeconds, bool allElements = true)
        {
            if (allElements)
                WaitForElementsDisplayCondition(driver, by, true, waitSeconds);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, by, true, waitSeconds);
        }

        public static void WaitForElementsToBeDisplayed(this RemoteWebDriver driver, IList<IWebElement> elements, int waitSeconds = WaitTimeoutSeconds, bool allElements = true)
        {
            if (allElements)
                WaitForElementsDisplayCondition(driver, elements, true, waitSeconds);
            else
                WaitForAtLeastOneElementDisplayCondition(driver, elements, true, waitSeconds);
        }

        private static void WaitForElementsDisplayCondition(this RemoteWebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAllElementsLocatedBy(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{by}' selector were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementsDisplayCondition(this RemoteWebDriver driver, IList<IWebElement> elements, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAllElementsLocatedBy(elements, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Not all from {elements.Count} elements were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForAtLeastOneElementDisplayCondition(this RemoteWebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAtLeastOneElementLocatedBy(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Elements with '{by}' selector were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForAtLeastOneElementDisplayCondition(this RemoteWebDriver driver, IList<IWebElement> elements, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(VisibleConditionOfAtLeastOneElementLocatedBy(elements, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Not all from {elements.Count} elements were not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAllElementsLocatedBy(By locator, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    return elements.All(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAllElementsLocatedBy(IList<IWebElement> elements, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.All(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAtLeastOneElementLocatedBy(By locator, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    return elements.Any(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> VisibleConditionOfAtLeastOneElementLocatedBy(IList<IWebElement> elements, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.Any(element => element.Displayed().Equals(expectedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Displayed

        public static void WaitForElementToBeNotDisplayed(this RemoteWebDriver driver, IWebElement element, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayCondition(driver, element, false, waitSeconds);
        }

        public static void WaitForElementToBeDisplayed(this RemoteWebDriver driver, IWebElement element, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayCondition(driver, element, true, waitSeconds);
        }

        public static void WaitForElementToBeDisplayed(this RemoteWebDriver driver, By locator, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayCondition(driver, locator, true, waitSeconds);
        }

        public static void WaitForElementToBeNotDisplayed(this RemoteWebDriver driver, By locator, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayCondition(driver, locator, false, waitSeconds);
        }

        private static void WaitForElementDisplayCondition(this RemoteWebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedCondition(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementDisplayCondition(this RemoteWebDriver driver, IWebElement element, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedCondition(element, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        //Return true if find at least one element by provided selector with Displayed condition true
        private static Func<IWebDriver, bool> ElementIsInDisplayedCondition(By locator, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    //If no elements found
                    if (!elements.Any())
                        return false.Equals(displayedCondition);
                    return elements.Any(x => x.Displayed().Equals(displayedCondition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInDisplayedCondition(IWebElement element, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.Displayed().Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Displayed in Element

        public static void WaitForElementToBeNotDisplayed(this RemoteWebDriver driver, IWebElement element, By selector, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementInElementDisplayCondition(driver, element, selector, false, waitSeconds);
        }

        public static void WaitForElementInElementToBeDisplayed(this RemoteWebDriver driver, IWebElement element, By selector, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementInElementDisplayCondition(driver, element, selector, true, waitSeconds);
        }

        public static void WaitForElementInElementDisplayCondition(this RemoteWebDriver driver, IWebElement element, By selector, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementInElementIsInDisplayedCondition(element, selector, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static Func<IWebDriver, bool> ElementInElementIsInDisplayedCondition(IWebElement element, By selector, bool displayedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.FindElement(selector).Displayed().Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Displayed After Refresh

        public static void WaitForElementToBeNotDisplayedAfterRefresh(this RemoteWebDriver driver, IWebElement element, bool waitForDataLoading = false, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayConditionAfterRefresh(driver, element, false, waitSeconds, waitForDataLoading);
        }

        //Only elements from PageObject are allowed!!!
        public static void WaitForElementToBeDisplayedAfterRefresh(this RemoteWebDriver driver, IWebElement element, bool waitForDataLoading = false, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayConditionAfterRefresh(driver, element, true, waitSeconds, waitForDataLoading);
        }

        public static void WaitForElementToBeDisplayedAfterRefresh(this RemoteWebDriver driver, IWebElement element, By by, bool waitForDataLoading = false, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayConditionAfterRefresh(driver, element, by, true, waitSeconds, waitForDataLoading);
        }

        public static void WaitForElementToBeDisplayedAfterRefresh(this RemoteWebDriver driver, By locator, bool waitForDataLoading = false, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayConditionAfterRefresh(driver, locator, true, waitSeconds, waitForDataLoading);
        }

        public static void WaitForElementToBeNotDisplayedAfterRefresh(this RemoteWebDriver driver, By locator, bool waitForDataLoading = false, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementDisplayConditionAfterRefresh(driver, locator, false, waitSeconds, waitForDataLoading);
        }

        private static void WaitForElementDisplayConditionAfterRefresh(this RemoteWebDriver driver, By by, bool condition, int waitSeconds, bool waitForDataLoading)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedConditionAfterRefresh(by, condition, waitForDataLoading));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementDisplayConditionAfterRefresh(this RemoteWebDriver driver, IWebElement element, bool condition, int waitSeconds, bool waitForDataLoading)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedConditionAfterRefresh(element, condition, waitForDataLoading));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementDisplayConditionAfterRefresh(this RemoteWebDriver driver, IWebElement element, By by, bool condition, int waitSeconds, bool waitForDataLoading)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInDisplayedConditionAfterRefresh(element, by, condition, waitForDataLoading));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Display condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        //Return true if find at least one element by provided selector with Displayed condition true
        private static Func<IWebDriver, bool> ElementIsInDisplayedConditionAfterRefresh(By locator, bool displayedCondition, bool waitForDataLoading)
        {
            return (driver) =>
            {
                try
                {
                    driver.Navigate().Refresh();

                    if (waitForDataLoading)
                        WaitForDataLoading((RemoteWebDriver)driver);

                    return IsElementDisplayed((RemoteWebDriver)driver, locator, WaitTime.Short).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInDisplayedConditionAfterRefresh(IWebElement element, bool displayedCondition, bool waitForDataLoading)
        {
            return (driver) =>
            {
                try
                {
                    driver.Navigate().Refresh();

                    if (waitForDataLoading)
                        WaitForDataLoading((RemoteWebDriver)driver);

                    return IsElementDisplayed((RemoteWebDriver)driver, element, WaitTime.Short).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInDisplayedConditionAfterRefresh(IWebElement element, By by, bool displayedCondition, bool waitForDataLoading)
        {
            return (driver) =>
            {
                try
                {
                    driver.Navigate().Refresh();

                    if (waitForDataLoading)
                        WaitForDataLoading((RemoteWebDriver)driver);

                    return IsElementInElementDisplayed((RemoteWebDriver)driver, element, by, WaitTime.Short).Equals(displayedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(displayedCondition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(displayedCondition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(displayedCondition);
                }
            };
        }

        #endregion

        #region Wait for Element to be (not) Enabled

        public static void WaitForElementToBeNotEnabled(this RemoteWebDriver driver, IWebElement element, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementEnabledCondition(driver, element, false, waitSeconds);
        }

        public static void WaitForElementToBeEnabled(this RemoteWebDriver driver, IWebElement element, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementEnabledCondition(driver, element, true, waitSeconds);
        }

        public static void WaitForElementToBeEnabled(this RemoteWebDriver driver, By locator, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementEnabledCondition(driver, locator, true, waitSeconds);
        }

        public static void WaitForElementToBeNotEnabled(this RemoteWebDriver driver, By locator, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementEnabledCondition(driver, locator, false, waitSeconds);
        }

        private static void WaitForElementEnabledCondition(this RemoteWebDriver driver, By by, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInEnabledCondition(by, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element with '{by}' selector was not changed Enabled condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        private static void WaitForElementEnabledCondition(this RemoteWebDriver driver, IWebElement element, bool condition, int waitSeconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
                wait.Until(ElementIsInEnabledCondition(element, condition));
            }
            catch (WebDriverTimeoutException e)
            {
                throw new Exception($"Element was not changed Enabled condition to '{condition}' after {waitSeconds} seconds", e);
            }
        }

        //Return true if find at least one element by provided selector with Displayed condition true
        private static Func<IWebDriver, bool> ElementIsInEnabledCondition(By locator, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var elements = driver.FindElements(locator);
                    //If no elements found
                    if (!elements.Any())
                        return false.Equals(condition);
                    return elements.Any(x => x.Enabled.Equals(condition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> ElementIsInEnabledCondition(IWebElement element, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.Enabled.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for text in Element

        public static void WaitForElementToNotHaveText(this RemoteWebDriver driver, IWebElement element, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, element, expectedText, false, waitSec);
        }

        public static void WaitForElementToNotHaveText(this RemoteWebDriver driver, By selector, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, selector, expectedText, false, waitSec);
        }

        public static void WaitForElementToHaveText(this RemoteWebDriver driver, IWebElement element, string expectedText, int waitSec = WaitTimeoutSeconds, bool throwException = true)
        {
            try
            {
                WaitElementContainsText(driver, element, expectedText, true, waitSec);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        public static void WaitForElementToHaveText(this RemoteWebDriver driver, IWebElement element, int waitSec = WaitTimeoutSeconds, bool throwException = true)
        {
            try
            {
                WaitElementHaveText(driver, element, true, waitSec);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        public static void WaitForElementToHaveText(this RemoteWebDriver driver, By selector, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, selector, expectedText, true, waitSec);
        }

        private static void WaitElementHaveText(this RemoteWebDriver driver, IWebElement element, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElement(element, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementHaveText(this RemoteWebDriver driver, By by, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElement(by, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBePresentInElement(IWebElement element, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.GetText().Equals(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBePresentInElement(By by, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.GetText().Equals(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for text in Elements

        public static void WaitForElementsToNotHaveText(this RemoteWebDriver driver, IList<IWebElement> elements, bool allElements = true, int waitSec = WaitTimeoutSeconds, bool throwException = true)
        {
            try
            {
                WaitElementsHaveText(driver, elements, string.Empty, false, waitSec, allElements);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        public static void WaitForElementsToNotHaveText(this RemoteWebDriver driver, IList<IWebElement> elements, string expectedText, bool allElements = true, int waitSec = WaitTimeoutSeconds, bool throwException = true)
        {
            try
            {
                WaitElementsHaveText(driver, elements, expectedText, false, waitSec, allElements);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        public static void WaitForElementsToHaveText(this RemoteWebDriver driver, IList<IWebElement> elements, string expectedText, bool allElements = true, int waitSec = WaitTimeoutSeconds, bool throwException = true)
        {
            try
            {
                WaitElementsHaveText(driver, elements, expectedText, true, waitSec, allElements);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        public static void WaitForElementsToHaveText(this RemoteWebDriver driver, IList<IWebElement> elements, bool allElements = true, int waitSec = WaitTimeoutSeconds, bool throwException = true)
        {
            try
            {
                WaitElementsHaveText(driver, elements, true, waitSec, allElements);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        private static void WaitElementsHaveText(this RemoteWebDriver driver, IList<IWebElement> elements, string expectedText, bool condition, int waitSec, bool allElements)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElements(elements, expectedText, condition, allElements));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the elements after {waitSec} seconds");
            }
        }

        private static void WaitElementsHaveText(this RemoteWebDriver driver, IList<IWebElement> elements, bool condition, int waitSec, bool allElements)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElements(elements, condition, allElements));
            }
            catch (Exception)
            {
                throw new Exception($"Text is not appears/disappears in the elements after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBePresentInElements(IList<IWebElement> elements, string text, bool condition, bool allElements)
        {
            return (driver) =>
            {
                try
                {
                    if (allElements)
                    {
                        return elements.All(x => x.GetText().Equals(text).Equals(condition));
                    }
                    else
                    {
                        return elements.Any(x => x.GetText().Equals(text).Equals(condition));
                    }
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBePresentInElements(IList<IWebElement> elements, bool condition, bool allElements)
        {
            return (driver) =>
            {
                try
                {
                    if (allElements)
                    {
                        return elements.All(x => !string.IsNullOrEmpty(x.GetText()).Equals(condition));
                    }
                    else
                    {
                        return elements.Any(x => !string.IsNullOrEmpty(x.GetText()).Equals(condition));
                    }
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for Element contains text

        public static void WaitForElementToNotContainsText(this RemoteWebDriver driver, IWebElement element, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, element, expectedText, false, waitSec);
        }

        public static void WaitForElementToNotContainsText(this RemoteWebDriver driver, By selector, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, selector, expectedText, false, waitSec);
        }

        public static void WaitForElementToContainsText(this RemoteWebDriver driver, IWebElement element, string expectedText, int waitSec = WaitTimeoutSeconds, bool throwException = true)
        {
            try
            {
                WaitElementContainsText(driver, element, expectedText, true, waitSec);
            }
            catch (Exception e)
            {
                if (throwException)
                {
                    throw e;
                }
            }
        }

        public static void WaitForElementToContainsText(this RemoteWebDriver driver, IList<IWebElement> elements, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, elements, expectedText, true, waitSec);
        }

        public static void WaitForElementToContainsText(this RemoteWebDriver driver, By selector, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, selector, expectedText, true, waitSec);
        }

        private static void WaitElementContainsText(this RemoteWebDriver driver, IWebElement element, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElement(element, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' contains condition was not changed to '{condition}' in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsText(this RemoteWebDriver driver, IList<IWebElement> elements, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElement(elements, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsText(this RemoteWebDriver driver, By by, string expectedText, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElement(by, expectedText, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static void WaitElementHaveText(this RemoteWebDriver driver, IWebElement elements, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBePresentInElement(elements, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElement(IWebElement element, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.GetText().Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElement(IList<IWebElement> elements, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.Any(x => x.GetText().Contains(text)).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElement(By by, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.GetText().Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBePresentInElement(IWebElement element, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return (element.GetText().Length > 0).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for text in Element after refresh

        public static void WaitForElementToNotContainsTextAfterRefresh(this RemoteWebDriver driver, IWebElement element, string expectedText, bool waitForDataLoading = false, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextAfterRefresh(driver, element, expectedText, false, waitSec, waitForDataLoading);
        }

        public static void WaitForElementToNotContainsTextAfterRefresh(this RemoteWebDriver driver, By selector, string expectedText, bool waitForDataLoading = false, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextAfterRefresh(driver, selector, expectedText, false, waitSec, waitForDataLoading);
        }

        public static void WaitForElementToContainsTextAfterRefresh(this RemoteWebDriver driver, IWebElement element, string expectedText, bool waitForDataLoading = false, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextAfterRefresh(driver, element, expectedText, true, waitSec, waitForDataLoading);
        }

        public static void WaitForElementToContainsTextAfterRefresh(this RemoteWebDriver driver, By selector, string expectedText, bool waitForDataLoading = false, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextAfterRefresh(driver, selector, expectedText, true, waitSec, waitForDataLoading);
        }

        private static void WaitElementContainsTextAfterRefresh(this RemoteWebDriver driver, IWebElement element, string expectedText, bool condition, int waitSec, bool waitForDataLoading)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAfterRefresh(element, expectedText, condition, waitForDataLoading));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsTextAfterRefresh(this RemoteWebDriver driver, By by, string expectedText, bool condition, int waitSec, bool waitForDataLoading)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAfterRefresh(by, expectedText, condition, waitForDataLoading));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAfterRefresh(IWebElement element, string text, bool condition, bool waitForDataLoading)
        {
            return (driver) =>
            {
                try
                {
                    WaitForElementToBeDisplayedAfterRefresh((RemoteWebDriver)driver, element, waitForDataLoading, 5);

                    return element.GetText().Contains(text).Equals(condition);
                }
                catch (TimeoutException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAfterRefresh(By by, string text, bool condition, bool waitForDataLoading)
        {
            return (driver) =>
            {
                try
                {
                    WaitForElementToBeDisplayedAfterRefresh((RemoteWebDriver)driver, by, waitForDataLoading, 5);

                    var element = driver.FindElement(by);
                    return element.GetText().Contains(text).Equals(condition);
                }
                catch (TimeoutException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Wait for text in Element attribute

        public static void WaitForElementToNotContainsTextInAttribute(this RemoteWebDriver driver, IWebElement element, string expectedText, string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextInAttribute(driver, element, expectedText, attribute, false, waitSec);
        }

        public static void WaitForElementToNotContainsTextInAttribute(this RemoteWebDriver driver, By selector, string expectedText, string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextInAttribute(driver, selector, expectedText, attribute, false, waitSec);
        }

        public static void WaitForElementToContainsTextInAttribute(this RemoteWebDriver driver, IWebElement element, string expectedText, string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextInAttribute(driver, element, expectedText, attribute, true, waitSec);
        }

        public static void WaitForElementToContainsTextInAttribute(this RemoteWebDriver driver, By selector, string expectedText, string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextInAttribute(driver, selector, expectedText, attribute, true, waitSec);
        }

        public static void WaitForAnyElementToContainsTextInAttribute(this RemoteWebDriver driver, IEnumerable<IWebElement> elements, string expectedText, string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsTextInAttribute(driver, elements, expectedText, attribute, true, waitSec);
        }

        private static void WaitElementContainsTextInAttribute(this RemoteWebDriver driver, IWebElement element, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAttribute(element, expectedText, attribute, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' element attribute after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsTextInAttribute(this RemoteWebDriver driver, By by, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAttribute(by, expectedText, attribute, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' element attribute located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static void WaitElementContainsTextInAttribute(this RemoteWebDriver driver, IEnumerable<IWebElement> elements, string expectedText, string attribute, bool condition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(TextToBeContainsInElementAttribute(elements, expectedText, attribute, condition));
            }
            catch (Exception)
            {
                throw new Exception($"Text '{expectedText}' is not appears/disappears in the '{attribute}' element attribute after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAttribute(IWebElement element, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.GetAttribute(attribute).Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAttribute(By by, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.GetAttribute(attribute).Contains(text).Equals(condition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElementAttribute(IEnumerable<IWebElement> elements, string text, string attribute, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return elements.Any(x => x.GetAttribute(attribute).Contains(text).Equals(condition));
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false.Equals(condition);
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false.Equals(condition);
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false.Equals(condition);
                }
            };
        }

        #endregion

        #region Element has child

        /// <summary>
        /// Wait while element do not have specified number of child elements
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        /// <param name="childSelector"></param>
        /// <param name="expectedCount"></param>
        public static void WaitForElementChildElements(this RemoteWebDriver driver, IWebElement element,
            By childSelector, int expectedCount)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(WaitTimeoutSeconds));
                wait.Until(ElementContainsChild(element, childSelector, expectedCount));
            }
            catch (Exception)
            {
                throw new Exception($"Required number of child elements are not appears in the element after {WaitTimeoutSeconds} seconds");
            }
        }

        private static Func<IWebDriver, bool> ElementContainsChild(IWebElement element, By selector, int childCount)
        {
            return (driver) =>
            {
                try
                {
                    return element.FindElements(selector).Count >= childCount;
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false;
                }
            };
        }

        #endregion

        #region Wait for attribute in Element

        public static void WaitForAttributePresentInElement(this RemoteWebDriver driver, IWebElement element,
            string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitForAttributeInElement(driver, element, attribute, true, waitSec);
        }

        public static void WaitForAttributeNotPresentInElement(this RemoteWebDriver driver, IWebElement element,
            string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitForAttributeInElement(driver, element, attribute, false, waitSec);
        }

        public static void WaitForAttributePresentInElement(this RemoteWebDriver driver, By by,
            string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitForAttributeInElement(driver, by, attribute, true, waitSec);
        }

        public static void WaitForAttributeNotPresentInElement(this RemoteWebDriver driver, By by,
            string attribute, int waitSec = WaitTimeoutSeconds)
        {
            WaitForAttributeInElement(driver, by, attribute, false, waitSec);
        }

        private static void WaitForAttributeInElement(this RemoteWebDriver driver, IWebElement element, string attribute, bool expectedCondition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(AttributeToBePresentInElement(element, attribute, expectedCondition));
            }
            catch (Exception)
            {
                throw new Exception($"Attribute '{attribute}' is not appears in the element after {waitSec} seconds");
            }
        }

        private static void WaitForAttributeInElement(this RemoteWebDriver driver, By by, string attribute, bool expectedCondition, int waitSec)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSec));
                wait.Until(AttributeToBePresentInElement(by, attribute, expectedCondition));
            }
            catch (Exception)
            {
                throw new Exception($"Attribute '{attribute}' is not appears in the element located by '{by}' selector after {waitSec} seconds");
            }
        }

        private static Func<IWebDriver, bool> AttributeToBePresentInElement(IWebElement element, string attribute, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    return element.IsAttributePresent(attribute).Equals(expectedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false;
                }
            };
        }

        private static Func<IWebDriver, bool> AttributeToBePresentInElement(By by, string attribute, bool expectedCondition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.IsAttributePresent(attribute).Equals(expectedCondition);
                }
                catch (NoSuchElementException)
                {
                    // Returns false because the element is not present in DOM.
                    return false;
                }
                catch (StaleElementReferenceException)
                {
                    // Returns false because stale element reference implies that element
                    // is no longer visible.
                    return false;
                }
                catch (InvalidOperationException)
                {
                    // Return false as no elements was located
                    return false;
                }
                catch (TargetInvocationException)
                {
                    // Return false as no elements was located
                    return false;
                }
            };
        }

        #endregion

        #region Actions with Javascript

        public static void ClickByJavascript(this RemoteWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = (IJavaScriptExecutor)driver;
            ex.ExecuteScript("arguments[0].click();", element);
        }

        public static void ClearByJavascript(this RemoteWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor ex = (IJavaScriptExecutor)driver;
            ex.ExecuteScript("arguments[0].value = '';", element);
        }

        public static void SendKeyByJavascript(this RemoteWebDriver driver, IWebElement element, string str)
        {
            IJavaScriptExecutor ex = (IJavaScriptExecutor)driver;
            ex.ExecuteScript($"arguments[0].value = '{str}';", element);
        }

        #endregion

        #region Frames

        public static void SwitchToFrame(this RemoteWebDriver driver, int frameNumber)
        {
            WebDriverWait wait = new WebDriverWait(driver, WaitTimeout);
            driver.SwitchTo().DefaultContent();
            wait.Until(x => x.FindElements(By.TagName("iframe")).Count > frameNumber);
            var frames = driver.FindElements(By.TagName("iframe"));
            driver.SwitchTo().Frame(frames[frameNumber]);
        }

        public static void SwitchToFrame(this RemoteWebDriver driver, string frameIdName)
        {
            WebDriverWait wait = new WebDriverWait(driver, WaitTimeout);
            driver.SwitchTo().DefaultContent();
            wait.Until(x => x.FindElements(By.Id(frameIdName)));
            driver.SwitchTo().Frame(frameIdName);
        }

        #endregion

        #region Availability of element

        public static bool IsElementDisplayed(this RemoteWebDriver driver, IWebElement element)
        {
            try
            {
                return element.Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementDisplayed(this RemoteWebDriver driver, IWebElement element, WaitTime waitTime)
        {
            try
            {
                var time = int.Parse(waitTime.GetValue());
                driver.WaitForElementToBeDisplayed(element, time);
                return element.Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementDisplayed(this RemoteWebDriver driver, By selector)
        {
            try
            {
                return driver.FindElement(selector).Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementDisplayed(this RemoteWebDriver driver, By selector, WaitTime waitTime)
        {
            try
            {
                var time = int.Parse(waitTime.GetValue());
                driver.WaitForElementToBeDisplayed(selector, time);
                return driver.FindElement(selector).Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementInElementDisplayed(this RemoteWebDriver driver, IWebElement element, By selector, WaitTime waitTime)
        {
            try
            {
                var time = int.Parse(waitTime.GetValue());
                driver.WaitForElementInElementToBeDisplayed(element, selector, time);
                return element.FindElement(selector).Displayed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public enum WaitTime
        {
            [System.ComponentModel.Description("6")]
            Short,
            [System.ComponentModel.Description("15")]
            Medium,
            [System.ComponentModel.Description("30")]
            Long
        }

        public static bool IsElementExists(this IWebDriver driver, By @by)
        {
            try
            {
                driver.FindElement(@by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public static bool IsElementExists(this IWebDriver driver, IWebElement element)
        {
            try
            {
                if (element == null)
                    return false;

                if (element.TagName.Contains("Exception"))
                    return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }

            return true;
        }

        public static bool IsElementExists(this RemoteWebDriver driver, By @by, WaitTime waitTime)
        {
            try
            {
                var time = int.Parse(waitTime.GetValue());
                WhatForElementToBeInExistsCondition(driver, @by, true, time);
                return driver.IsElementExists(@by);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsElementInElementExists(this RemoteWebDriver driver, IWebElement element, By selector, WaitTime waitTime)
        {
            try
            {
                var time = int.Parse(waitTime.GetValue());
                driver.WaitForElementInElementToBeExists(element, selector, time);
                var elementInElement = element.FindElement(selector);
                return IsElementExists(driver, elementInElement);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Checkbox

        //0 - not checked
        //1 - indeterminate
        //2 - all checked
        public static int GetEvergreenCheckboxTripleState(this RemoteWebDriver driver, IWebElement checkbox)
        {
            //Get mat-checkbox webElement
            var checkboxElement = checkbox.TagName.Equals("mat-checkbox")
                ? checkbox
                : checkbox.FindElement(By.XPath(".//ancestor::*[contains(@class,'mat-checkbox')][not(div)]"));
            var classAttribute = checkboxElement.GetAttribute("class");

            if (classAttribute.Contains("indeterminate"))
            {
                return 1;
            }
            else
            {
                if (!classAttribute.Contains("checked"))
                {
                    return 0;
                }
                else
                {
                    if (classAttribute.Contains("checked"))
                    {
                        return 2;
                    }
                }
            }

            throw new Exception("Unable to get checkbox selected state");
        }

        public static void SetEvergreenCheckboxState(this RemoteWebDriver driver, IWebElement checkbox, bool desiredState)
        {
            if (!checkbox.Selected.Equals(desiredState))
            {
                driver.ClickByActions(checkbox);
            }
        }

        #endregion

        public static void WaitFor(this RemoteWebDriver driver, Func<bool> flag)
        {
            for (int i = 0; i < 60; i++)
            {
                if (flag())
                    break;

                Thread.Sleep(1000);
            }
        }

        //For cases with _driver.FindBy
        public static void ExecuteAction(this RemoteWebDriver driver, Action actionToDo)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    actionToDo.Invoke();
                    return;
                }
                catch (NoSuchElementException)
                {
                    Thread.Sleep(1000);
                }
                catch (StaleElementReferenceException)
                {
                    Thread.Sleep(1000);
                }
                catch (NullReferenceException)
                {
                    Thread.Sleep(1000);
                }
                catch (TargetInvocationException)
                {
                    Thread.Sleep(1000);
                }
                catch (ElementClickInterceptedException)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        //For cases when we need return value
        public static bool ExecuteFunc(this RemoteWebDriver driver, Func<bool> actionToDo)
        {
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    return actionToDo.Invoke();
                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            }

            return false;
        }
    }
}
