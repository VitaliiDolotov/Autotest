using System;
using System.Collections.Generic;

namespace Autotest.DTO
{
    public class TestInfo
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public List<string> Tags { get; set; }
        public string Environment { get; set; }
        public string Exception { get; set; }
        public bool ShouldBeSkipped { get; set; }
    }
}
