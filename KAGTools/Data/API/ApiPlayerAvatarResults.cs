using Newtonsoft.Json;

namespace KAGTools.Data.API
{
    public class ApiPlayerAvatarResults
    {
        [JsonProperty("large")]
        public string LargeUrl { get; set; }

        [JsonProperty("medium")]
        public string MediumUrl { get; set; }

        [JsonProperty("small")]
        public string SmallUrl { get; set; }
    }
}
