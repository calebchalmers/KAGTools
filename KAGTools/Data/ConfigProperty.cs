using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Data
{
    public class BaseConfigProperty
    {
        public string Name { get; }
        public string Value { get; set; }

        public BaseConfigProperty(string name)
        {
            Name = name;
        }
    }

    public class StringConfigProperty : BaseConfigProperty
    {
        public StringConfigProperty(string name, string value) : base(name)
        {
            Value = value;
        }
    }

    public class IntConfigProperty : BaseConfigProperty
    {
        public new int Value
        {
            get { return int.TryParse(base.Value, out var result) ? result : -1; }
            set { base.Value = value.ToString(); }
        }

        public IntConfigProperty(string name, int value) : base(name)
        {
            Value = value;
        }
    }

    public class BoolConfigProperty : BaseConfigProperty
    {
        public new bool Value
        {
            get { return base.Value == "1"; }
            set { base.Value = value ? "1" : "0"; }
        }

        public BoolConfigProperty(string name, bool value) : base(name)
        {
            Value = value;
        }
    }
}
