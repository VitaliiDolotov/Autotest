﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SfsExtras.Utils;

namespace Autotest.Providers
{
    class UrlProvider
    {
        public static string Url => ConfigurationReader.AppSettings("appURL");
    }
}