﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using SfsExtras.Providers;

namespace SfsExtras.Base
{
    public class BaseTest : IBaseTest
    {
        public RemoteWebDriver Driver { get; set; }

        public RemoteWebDriver CreateBrowserDriver()
        {
            switch (BrowserProvider.BrowserType)
            {
                case "local":
                    return CreateLocalDriver();
                case "remote":
                    return CreateRemoteDriver();
                default:
                    throw new Exception($"Browser type '{BrowserProvider.BrowserType}' was not identified");
            }
        }

        private RemoteWebDriver CreateLocalDriver()
        {
            switch (BrowserProvider.TargetBrowser)
            {
                case "chrome":
                    var options = new ChromeOptions();
                    options.AddArgument("--safebrowsing-disable-download-protection");
                    options.AddUserProfilePreference("download.prompt_for_download", false);
                    options.AddUserProfilePreference("download.directory_upgrade", true);
                    options.AddUserProfilePreference("safebrowsing.enabled", true);
                    if (BrowserProvider.Resolution.Equals("maximized"))
                        options.AddArgument("--start-maximized");
                    string executingAssemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    return new ChromeDriver(executingAssemblyFolder, options);

                case "firefox":
                    return new FirefoxDriver();

                case "internet explorer":
                    return new InternetExplorerDriver();

                case "edge":
                    return new EdgeDriver();

                default:
                    throw new Exception($"Browser type '{BrowserProvider.TargetBrowser}' was not identified");
            }
        }

        private RemoteWebDriver CreateRemoteDriver()
        {
            switch (BrowserProvider.TargetBrowser)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--safebrowsing-disable-download-protection");
                    chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
                    chromeOptions.AddUserProfilePreference("download.directory_upgrade", true);
                    chromeOptions.AddUserProfilePreference("safebrowsing.enabled", true);
                    return new RemoteWebDriver(new Uri(BrowserProvider.HubUri), chromeOptions);

                case "firefox":
                    var firefoxOptions = new FirefoxOptions();
                    return new RemoteWebDriver(new Uri(BrowserProvider.HubUri), firefoxOptions);

                case "internet explorer":
                    var ieOptions = new InternetExplorerOptions();
                    return new RemoteWebDriver(new Uri(BrowserProvider.HubUri), ieOptions);

                case "edge":
                    var edgeOptions = new EdgeOptions();
                    return new RemoteWebDriver(new Uri(BrowserProvider.HubUri), edgeOptions);

                default:
                    throw new Exception($"Browser type '{BrowserProvider.TargetBrowser}' was not identified");
            }
        }
    }
}