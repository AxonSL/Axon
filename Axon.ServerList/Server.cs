using Newtonsoft.Json;

namespace Axon.ServerList;

[Serializable]
public struct Server
{
    [JsonProperty("version")] public string Version;                          //1
    [JsonProperty("info")] public string Info;                             //2
    [JsonProperty("pastebin")] public string Patsebin;                         //4

    [JsonProperty("geoblocking")] public bool Geoblocking;                        //8
    [JsonProperty("whitelist")] public bool Whitelist;                          //16
    [JsonProperty("accessRestriction")] public bool AccessRestriction;                  //32

    [JsonProperty("firendlyFire")] public bool FriendlyFire;                       //64
    [JsonProperty("players")] public int Players;                             //128
    [JsonProperty("maxPlayers")] public int MaxPlayers;                          //256
    [JsonProperty("playerList")] public List<string> PlayerList;   //512

    [JsonProperty("mods")] public ModInfo[] Mods;                //1024

    [JsonProperty("ip")] public string Ip;
    [JsonProperty("port")] public ushort Port;
    [JsonProperty("identifier")] public string Identifier;
}