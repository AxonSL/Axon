using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.ServerList;

public class VerifiedServers
{
    [JsonProperty("token")] public string Token = "";
    [JsonProperty("identifier")] public Guid Identifier;

    [JsonProperty("email")] public string EMail = "";
    [JsonProperty("discord")] public string Discord = "";
}