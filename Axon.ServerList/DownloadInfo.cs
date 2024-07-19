using Newtonsoft.Json;

namespace Axon.ServerList;

[Serializable]
public class ModInfo
{
    [JsonProperty("name")] public string Name = "";
    [JsonProperty("version")] public string Version = "";
}
