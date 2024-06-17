using Axon.Shared.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Auth;

public class ServerConnection
{
    public ClientAuthSession Session { get; set; }
    public ServerHandshake Handshake { get; set; }
    public string ServerIdentifier { get; set; }
}
