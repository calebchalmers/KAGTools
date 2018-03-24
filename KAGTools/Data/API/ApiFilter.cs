using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Data.API
{
    public struct ApiFilter
    {
        public ApiFilter(string field, object value, FilterOperator op = FilterOperator.eq)
        {
            Field = field;
            Operator = op;
            Value = value;
        }

        [JsonProperty("field")]
        public string Field { get; }

        [JsonProperty("op")]
        public FilterOperator Operator { get; }

        [JsonProperty("value")]
        public object Value { get; }
    }

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
