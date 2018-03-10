using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Data
{
    public class ApiFilter
    {
        public ApiFilter(string field, FilterOperator op, object value)
        {
            Field = field;
            Operator = op;
            Value = value;
        }

        public ApiFilter(string field, object value) :
            this(field, FilterOperator.eq, value)
        {

        }

        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("op")]
        public FilterOperator Operator { get; set; }

        [JsonProperty("value")]
        public object Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum FilterOperator
        {
            eq, // =
            ne, // !=
            le, // <=
            lt, // <
            ge, // >=
            gt, // >
        }
    }
}
