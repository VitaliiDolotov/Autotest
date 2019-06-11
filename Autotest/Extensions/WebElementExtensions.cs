using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using SfsExtras.Base;
using SfsExtras.Utils;

namespace SfsExtras.Extensions
{
    public static class WebElementExtensions
    {
        //This is specific method for 'ng-table-select-count' elements
        public static void SelectboxSelect(this IWebElement selectbox, string option, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                var selectElement = new SelectElement(selectbox);
                IList<IWebElement> options = selectElement.Options;
                for (int i = 0; i < options.Count; i++)
                {
                    if (string.Equals(options[i].Text, option, StringComparison.InvariantCultureIgnoreCase))
                    {
                        selectElement.SelectByIndex(i);
                        break;
                    }
                }
            }
            else
            {
                var selectElement = new SelectElement(selectbox);
                selectElement.SelectByText(option);
            }
        }

        //This is specific method for 'ng-table-select-count' elements
        public static string GetSelectedValue(this IWebElement selectbox)
        {
            try
            {
                return selectbox.FindElement(By.XPath(".//div[contains(@ng-bind,'select.selected.name')]")).Text;
            }
            catch (Exception e)
            {
                Logger.Write("Unable to get selected value from selectbox");
                throw e;
            }
        }

        public static IWebElement UntilElementHasChilds(this IWebElement element, RemoteWebDriver driver, By locator, TimeSpan timeOut, int childsCount = 4)
        {
            new WebDriverWait(driver, timeOut).Until(d => element.FindElements(locator).Count >= childsCount);

            return element;
        }

        public static bool Displayed(this IWebElement element)
        {
            try
            {
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        #region Checkboxes

        public static bool IsCheckBoxSelected(this IWebElement checkbox)
        {
            return checkbox.Selected;
        }

        public static void CheckCheckBox(this IWebElement checkbox)
        {
            if (!checkbox.Selected)
                checkbox.Click();
        }

        public static void UncheckCheckBox(this IWebElement checkbox)
        {
            if (checkbox.Selected)
                checkbox.Click();
        }

        public static void SetCheckboxState(this IWebElement checkbox, bool desiredState)
        {
            if (desiredState)
            {
                if (!checkbox.Selected)
                    checkbox.Click();
            }
            else
            {
                if (checkbox.Selected)
                    checkbox.Click();
            }
        }

        #endregion

        public static bool IsAttributePresent(this IWebElement element, string attribute)
        {
            try
            {
                var value = element.GetAttribute(attribute);
                return value != null;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
