using Axon.Shared.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Server.Auth;

public class Connection
{
    public ServerAuthSession ServerEnv { get; set; }
    public ClientHandshake Handshake {  get; set; }
}
