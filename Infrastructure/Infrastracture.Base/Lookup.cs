using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastracture.Base
{
    public class Lookup
    {
        public int ID { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Dictionary<string,string> Translation { get; set; }
    }
}
