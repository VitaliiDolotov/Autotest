using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SfsExtras.Base;
using SfsExtras.Utils;

namespace SfsExtras.Extensions
{
    public static class WebDriverExtensions
    {
        private const int WaitTimeoutSeconds = 30;
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(WaitTimeoutSeconds);
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
        private const int NumberOfTimesToWait = 2;

        public static T NowAt<T>(this RemoteWebDriver driver) where T : SeleniumBasePage, new()
        {
            try
            {
                var page = new T { Driver = driver, Actions = new Actions(driver) };
                driver.WaitForLoadingElements(page, null);
                page.InitElements();
                return page;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception($"Unable to init page: {e}");
            }
        }

        public static string CreateScreenshot(this RemoteWebDriver driver, string fileName)
        {
            try
            {
                FileSystemHelper.EnsureScreensotsFolderExists();
                var formatedFileName = fileName.Replace("\\", string.Empty).Replace("/", string.Empty).Replace("\"", "'");
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
            Thread.Sleep(1000);

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

        public static void WaitForLoadingElements(this RemoteWebDriver driver, SeleniumBasePage page, By bySelector)
        {
            var bys = bySelector != null ? new List<By> { bySelector } : page.GetPageIdentitySelectors();

            foreach (var by in bys)
            {
                driver.WaitForElement(by);
            }

            page.InitElements();
        }

        public static void WaitForElement(this RemoteWebDriver driver, By by)
        {
            var attempts = NumberOfTimesToWait;
            while (attempts > 0)
            {
                try
                {
                    attempts--;
                    ExecuteWithLogging(() => FluentWait.Create(driver)
                        .WithTimeout(WaitTimeout)
                        .WithPollingInterval(PollingInterval)
                        .Until(ElementIsInDisplayedCondition(by, true)), by);

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

        #region Locate element

        public static IList<IWebElement> LocateElements(this RemoteWebDriver driver, By selector)
        {
            try
            {
                return driver.FindElements(selector);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IWebElement LocateElement(this RemoteWebDriver driver, By selector)
        {
            try
            {
                return driver.FindElement(selector);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

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
            };
        }

        #endregion

        #region Wait for ElementS to be (not) Displayed

        public static void WaitForElementsToBeNotDisplayed(this RemoteWebDriver driver, By by, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementsDisplayCondition(driver, by, false, waitSeconds);
        }

        public static void WaitForElementsToBeNotDisplayed(this RemoteWebDriver driver, IList<IWebElement> elements, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementsDisplayCondition(driver, elements, false, waitSeconds);
        }

        public static void WaitForElementsToBeDisplayed(this RemoteWebDriver driver, By by, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementsDisplayCondition(driver, by, true, waitSeconds);
        }

        public static void WaitForElementsToBeDisplayed(this RemoteWebDriver driver, IList<IWebElement> elements, int waitSeconds = WaitTimeoutSeconds)
        {
            WaitForElementsDisplayCondition(driver, elements, true, waitSeconds);
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

        private static Func<IWebDriver, bool> InvisibilityOfAllElementsLocatedBy(By locator)
        {
            return VisibleConditionOfAllElementsLocatedBy(locator, false);
        }

        private static Func<IWebDriver, bool> InvisibilityOfAllElementsLocatedBy(IList<IWebElement> elements)
        {
            return VisibleConditionOfAllElementsLocatedBy(elements, false);
        }

        private static Func<IWebDriver, bool> VisibilityOfAllElementsLocatedBy(By locator)
        {
            return VisibleConditionOfAllElementsLocatedBy(locator, true);
        }

        private static Func<IWebDriver, bool> VisibilityOfAllElements(IList<IWebElement> elements)
        {
            return VisibleConditionOfAllElementsLocatedBy(elements, true);
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
            };
        }

        #endregion

        #region Wait for text in Element

        public static void WaitForElementToNotContainsText(this RemoteWebDriver driver, IWebElement element, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, element, expectedText, false, waitSec);
        }

        public static void WaitForElementToNotContainsText(this RemoteWebDriver driver, By selector, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, selector, expectedText, false, waitSec);
        }

        public static void WaitForElementToContainsText(this RemoteWebDriver driver, IWebElement element, string expectedText, int waitSec = WaitTimeoutSeconds)
        {
            WaitElementContainsText(driver, element, expectedText, true, waitSec);
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

        private static Func<IWebDriver, bool> TextToBeContainsInElement(IWebElement element, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    return element.Text.Contains(text).Equals(condition);
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
            };
        }

        private static Func<IWebDriver, bool> TextToBeContainsInElement(By by, string text, bool condition)
        {
            return (driver) =>
            {
                try
                {
                    var element = driver.FindElement(by);
                    return element.Text.Contains(text).Equals(condition);
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
            };
        }

        #endregion

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

        #endregion

        public static void OpenInNewTab(this RemoteWebDriver driver, string url)
        {
            driver.ExecuteScript($"window.open('{url}','_blank');");
            driver.SwitchTo().Window(driver.WindowHandles.Last());
        }
    }
}
