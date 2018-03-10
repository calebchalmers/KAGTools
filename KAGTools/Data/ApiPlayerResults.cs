using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KAGTools.Data
{
    public class ApiPlayerResults
    {
        [JsonProperty("playerBanFlags")]
        public PlayerBanFlags BanFlags { get; set; }

        [JsonProperty("playerInfo")]
        public PlayerInfo Info { get; set; }

        [JsonProperty("playerStatus")]
        public PlayerStatus Status { get; set; }

        public class PlayerBanFlags
        {
            [JsonProperty("all")]
            public string[] All { get; set; }

            [JsonProperty("current")]
            public string[] Current { get; set; }
        }

        public class PlayerInfo
        {
            [JsonProperty("active")]
            public bool IsActive { get; set; }

            [JsonProperty("banExpiration")]
            public string BanExpiration { get; set; }

            [JsonProperty("banReason")]
            public string BanReason { get; set; }

            [JsonProperty("banned")]
            public bool IsBanned { get; set; }

            [JsonProperty("gold")]
            public bool HasGold { get; set; }

            [JsonProperty("gold_storm")]
            public bool HasGoldStorm { get; set; }

            [JsonProperty("gold_trenchrun")]
            public bool HasGoldTrenchRun { get; set; }

            [JsonProperty("regUnixTime")]
            public int RegisterUnixTime { get; set; }

            [JsonProperty("registered")]
            public string RegisterDate { get; set; }

            [JsonProperty("role")]
            public int Role { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }
        }

        public class PlayerStatus
        {
            [JsonProperty("action")]
            public int Action { get; set; }

            [JsonProperty("lastUpdate")]
            public string LastUpdateDate { get; set; }

            [JsonProperty("server")]
            public PlayerServer Server { get; set; }

            public class PlayerServer
            {
                [JsonProperty("serverIPv4Address")]
                public string IPv4Address { get; set; }

                [JsonProperty("serverIPv6Address")]
                public string IPv6Address { get; set; }

                [JsonProperty("serverPort")]
                public string Port { get; set; }
            }
        }
    }
}
