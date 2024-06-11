using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.ServerList;

public struct VerifiedServers
{
    public string Token;
    public Guid Identifier;

    public string EMail;
    public string Discord;
}