using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autotest.DTO
{
    public class AuthObject
    {
        public string Viewstate { get; set; }
        public string Eventvalidation { get; set; }
        public string ViewstateGenerator { get; set; }
    }
}
