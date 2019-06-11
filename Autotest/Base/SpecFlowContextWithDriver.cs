using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace SfsExtras.Base
{
    [Binding]
    public class SpecFlowContextWithDriver : SpecFlowContext
    {
        protected readonly RemoteWebDriver Driver;

        public SpecFlowContextWithDriver(RemoteWebDriver driver)
        {
            Driver = driver;
        }
    }
}
