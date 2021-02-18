using System;
using System.IO;
using System.Reflection;
using AutomationUtils.Utils;
using Autotest.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;

namespace Autotest.Utils
{
    public static class BrowserFactory
    {
        public static RemoteWebDriver CreateDriver()
        {
            try
            {
                return CreateDriverInstance();
            }
            //Just retry when node was busy
            catch (AggregateException)
            {
                Logger.Write("AggregateException: Browser was not created. Retry to create browser", Logger.LogLevel.Warning);
                return CreateDriverInstance();
            }
            //Retry for below error
            //A exception with a null response was thrown sending an HTTP request to the remote WebDriver server for URL http://autohub.corp.juriba.com:4444/wd/hub/session. The status of the exception was UnknownError, and the message was: Only one usage of each socket address (protocol/network address/port) is normally permitted.
            catch (WebDriverException e)
            {
                Logger.Write($"WebDriverException: Browser was not created. Retry to create browser: {e}", Logger.LogLevel.Warning);
                return CreateDriverInstance();
            }
        }

        private static RemoteWebDriver CreateDriverInstance()
        {
            switch (Browser.RemoteDriver)
            {
                case "local":
                    return CreateLocalDriver();
                case "remote":
                    return CreateRemoteDriver();

                default:
                    throw new Exception($"Browser type '{Browser.Type}' was not identified");
            }
        }

        private static RemoteWebDriver CreateLocalDriver()
        {
            switch (Browser.Type)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--safebrowsing-disable-download-protection");

                    chromeOptions.AddUserProfilePreference("download.default_directory", PathProvider.DownloadsFolder);
                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    chromeOptions.AddUserProfilePreference("download.directory_upgrade", true);
                    chromeOptions.AddUserProfilePreference("safebrowsing.enabled", false);
                    if (Browser.RemoteDriver.Equals("local"))
                    {
                        chromeOptions.AddArgument("--start-maximized");
                    }
                    //options.UseSpecCompliantProtocol = false;
                    chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);

                    var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), chromeOptions);

                    return driver;

                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.SetPreference("browser.download.folderList", 2);
                    firefoxOptions.SetPreference("browser.helperApps.alwaysAsk.force", false);
                    firefoxOptions.SetPreference("browser.download.manager.showWhenStarting", false);
                    firefoxOptions.SetPreference("browser.helperApps.neverAsk.openFile",
                        "image/png, text/html, image/tiff, text/csv, application/zip, application/octet-stream");
                    firefoxOptions.SetPreference("browser.helperApps.neverAsk.saveToDisk", "image/png, text/html, image/tiff, text/csv, application/zip, application/octet-stream");
                    return new FirefoxDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), firefoxOptions);

                case "internet explorer":
                    return new InternetExplorerDriver();

                case "edge":
                    return new EdgeDriver();

                default:
                    throw new Exception($"Browser type '{Browser.Type}' was not identified");
            }
        }

        private static RemoteWebDriver CreateRemoteDriver()
        {
            switch (Browser.Type)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();

                    chromeOptions.AddArguments("headless", "--window-size=1920,1080", "--safebrowsing-disable-download-protection");

                    chromeOptions.AddUserProfilePreference("download.default_directory", "/home/selenium/Downloads");
                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    chromeOptions.AddUserProfilePreference("download.directory_upgrade", true);
                    chromeOptions.AddUserProfilePreference("safebrowsing.enabled", false);

                    //chromeOptions.UseSpecCompliantProtocol = false;
                    chromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
                    //typeof(CapabilityType).GetField(nameof(CapabilityType.LoggingPreferences), BindingFlags.Static | BindingFlags.Public).SetValue(null, "goog:loggingPrefs");
                    return new RemoteWebDriver(new Uri(Browser.HubUri), chromeOptions);

                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    firefoxOptions.SetPreference("browser.helperApps.neverAsk.saveToDisk", "application/octet-stream");
                    return new RemoteWebDriver(new Uri(Browser.HubUri), firefoxOptions);

                case "internet explorer":
                    var ieOptions = new InternetExplorerOptions();
                    return new RemoteWebDriver(new Uri(Browser.HubUri), ieOptions);

                case "edge":
                    var edgeOptions = new EdgeOptions();
                    return new RemoteWebDriver(new Uri(Browser.HubUri), edgeOptions);

                default:
                    throw new Exception($"Browser type '{Browser.Type}' was not identified");
            }
        }
    }
}
