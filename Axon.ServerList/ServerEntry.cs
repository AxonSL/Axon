using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.ServerList;

public class ServerEntry
{
    public Server Server;
    public DateTime LastUpdate;
    public string Identifier => Server.Identifier + "-" + Server.Ip + ":" + Server.Port;
}
