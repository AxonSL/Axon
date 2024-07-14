using CommandSystem;
using Exiled.API.Features;
using Mirror;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Server.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class TestCommand : ICommand
{
    public string Command => "axon";

    public string[] Aliases => new string[0];

    public string Description => "test command";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        foreach(var connection in NetworkServer.connections.Values)
        {
            connection.Send(new Axon.NetworkMessages.TestMessage()
            {
                message = "TEST"
            });
        }

        response = "sended message";
        return true;
    }
}
