using System;
using System.Threading;
using AutomationUtils.Utils;
using Autotest.Providers;
using Autotest.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Autotest.Extensions
{
    public static class WebDriverExtensions
    {
        public static string CreateScreenshot(this RemoteWebDriver driver, string fileName)
        {
            try
            {
                FileSystemHelper.EnsureFolderExists(PathProvider.ScreenshotsFolder);
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
                Logger.Write($"Unable to get screenshot: {e.Message}", Logger.LogLevel.Error);
                return string.Empty;
            }
        }

        public static void QuitDriver(this RemoteWebDriver driver)
        {
            try
            {
                driver.Manage().Cookies.DeleteAllCookies();
            }
            catch (Exception e)
            {
                Logger.Write($"Error during cookie deleting: {e}", Logger.LogLevel.Warning);
            }

            try
            {
                driver.Close();
            }
            catch (Exception e)
            {
                Logger.Write($"Error during driver closing: {e}", Logger.LogLevel.Warning);
            }

            try
            {
                driver.Quit();
            }
            catch (Exception e)
            {
                Logger.Write($"Error on driver quite: {e}", Logger.LogLevel.Warning);
                try
                {
                    Thread.Sleep(3000);
                    Logger.Write("Retrying to quite driver", Logger.LogLevel.Info);
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    Logger.Write($"Driver was not quite on retry: {ex}", Logger.LogLevel.Error);
                }
            }

            try
            {
                driver.Dispose();
            }
            catch (Exception e)
            {
                Logger.Write($"Error disposing webdriver: {e}", Logger.LogLevel.Warning);
            }
        }
    }
}
