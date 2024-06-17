using Axon.Server.Auth;
using Axon.Shared.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Server;

public static class AuthHandler
{
    internal static Dictionary<string, Connection> _connections = new();
    internal static Dictionary<IPEndPoint, PlayerStorage> _storedPlayers = new();

    internal static string AddConnection(ServerAuthSession session,ClientHandshake handshake)
    {
        string random;
        do
        {
            random = RandomGenerator.GetStringSecure(50);
        }
        while(_connections.ContainsKey(random));

        _connections[random] = new Connection
        {
            ServerEnv = session,
            Handshake = handshake,
        };

        return random;
    }
}
