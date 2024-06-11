using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.ServerList;

[Serializable]
public struct ServerListConfiguration
{
    public string Url;
    public VerifiedServers[] VerifiedServers;
}
