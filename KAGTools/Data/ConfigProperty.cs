using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Data
{
    public class ConfigProperty
    {
        protected string _propertyName;
        protected object _value;
        
        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public ConfigProperty(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }
    }

    public class ConfigPropertyDouble : ConfigProperty
    {
        public new double Value
        {
            get { return (double)_value; }
            set { _value = value; }
        }

        public ConfigPropertyDouble(string propertyName, double value) : base(propertyName, value) { }
    }

    public class ConfigPropertyString : ConfigProperty
    {
        public new string Value
        {
            get { return (string)_value; }
            set { _value = value; }
        }

        public ConfigPropertyString(string propertyName, string value) : base(propertyName, value) { }
    }

    public class ConfigPropertyBoolean : ConfigProperty
    {
        public new bool Value
        {
            get { return (bool)_value; }
            set { _value = value; }
        }

        public ConfigPropertyBoolean(string propertyName, bool value) : base(propertyName, value) { }
    }
}
