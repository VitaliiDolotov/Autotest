using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutomationUtils.Extensions;
using AutomationUtils.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Autotest.Utils
{
    public class BrowsersList
    {
        private List<RemoteWebDriver> _drivers = new List<RemoteWebDriver>();

        private Thread _pingDriversThread = null;

        private RemoteWebDriver DriverInUse { get; set; }

        public RemoteWebDriver GetBrowser()
        {
            return DriverInUse;
        }

        //id starts from 0 where zero is the main browser
        public RemoteWebDriver GetBrowser(int id)
        {
            //Drop current driver
            DriverInUse = null;

            if (id > _drivers.Count - 1)
            {
                throw new Exception($"Unable to get driver with {id} id");
            }

            //Set new current driver
            DriverInUse = _drivers[id];

            //Start ping thread if not yet started
            if (_pingDriversThread == null)
            {
                _pingDriversThread = new Thread(PingDrivers);
            }

            return DriverInUse;
        }

        public void AddDriver(RemoteWebDriver driver)
        {
            _drivers.Add(driver);

            //First browser that was added will be main in focus
            if (_drivers.Count == 1)
            {
                DriverInUse = driver;
            }
        }

        public List<RemoteWebDriver> GetAllBrowsers()
        {
            //Stop ping
            _pingDriversThread?.Abort();
            return _drivers;
        }

        public void PingDrivers()
        {
            try
            {
                var driversForPing = _drivers.Where(x => !x.Equals(DriverInUse));

                foreach (RemoteWebDriver driver in driversForPing)
                {
                    try
                    {
                        driver.IsElementExists(By.XPath(".//body"));
                    }
                    catch (Exception e)
                    {
                        Logger.Write(e, Logger.LogLevel.Warning);
                    }
                }

                Thread.Sleep(20000);
            }
            catch (Exception e)
            {
                Logger.Write(e, Logger.LogLevel.Warning);
            }
        }
    }
}
