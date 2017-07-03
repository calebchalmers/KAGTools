using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Data
{
    public class ManualItem
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public ManualItem(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
