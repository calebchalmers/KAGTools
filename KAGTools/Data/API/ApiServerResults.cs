using Newtonsoft.Json;
using System;

namespace KAGTools.Data.API
{
    public class ApiServerResults
    {
        [JsonProperty("serverList")]
        public ApiServer[] Servers { get; set; }

        [JsonProperty("serverStatus")]
        public ApiServer Server { get; set; }
    }

    public class ApiServer
    {
        [JsonProperty("DNCycle")]
        public bool DNCycle { get; set; }

        [JsonProperty("DNState")]
        public int DNState { get; set; }

        [JsonProperty("IPv4Address")]
        public string IPv4Address { get; set; }

        [JsonProperty("IPv6Address")]
        public string IPv6Address { get; set; }

        [JsonProperty("build")]
        public int Build { get; set; }

        [JsonProperty("buildType")]
        public string BuildType { get; set; }

        [JsonProperty("connectable")]
        public bool IsConnectable { get; set; }

        [JsonProperty("currentPlayers")]
        public int PlayerCount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("firstSeen")]
        public DateTime FirstSeenDate { get; set; }

        [JsonProperty("gameID")]
        public int GameID { get; set; }

        [JsonProperty("gameMode")]
        public string Gamemode { get; set; }

        [JsonProperty("gameState")]
        public ServerGameState GameState { get; set; }

        [JsonProperty("gold")]
        public bool HasGold { get; set; }

        [JsonProperty("internalIPv4")]
        public string InternalIPv4 { get; set; }

        [JsonProperty("lastUpdate")]
        public DateTime LastUpdateDate { get; set; }

        [JsonProperty("mapH")]
        public int MapHeight { get; set; }

        [JsonProperty("mapName")]
        public string MapName { get; set; }

        [JsonProperty("mapW")]
        public int MapWidth { get; set; }

        [JsonProperty("maxPlayers")]
        public int MaxPlayers { get; set; }

        [JsonProperty("maxSpectatorPlayers")]
        public int MaxSpectatorPlayers { get; set; }

        [JsonProperty("modName")]
        public string ModName { get; set; }

        [JsonProperty("modsVerified")]
        public bool ModsVerified { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("numBots")]
        public int BotCount { get; set; }

        [JsonProperty("password")]
        public bool HasPassword { get; set; }

        [JsonProperty("playerList")]
        public string[] Players { get; set; }

        [JsonProperty("playerPercentage")]
        public float PlayerPercentage { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("preferAF")]
        public int PreferAF { get; set; }

        [JsonProperty("reservedPlayers")]
        public int ReservedPlayers { get; set; }

        [JsonProperty("spectatorPlayers")]
        public int SpectatorPlayers { get; set; }

        [JsonProperty("subGameMode")]
        public string SubGameMode { get; set; }

        [JsonProperty("usingMods")]
        public bool UsingMods { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public enum ServerGameState
    {
        Warmup = 0,
        Active = 1
    }
}
