using CommandSystem;
using Exiled.API.Features;
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
        try
        {
            Player.Get(sender).Position = UnityEngine.Vector3.up * 500f;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        response = "set to role 44";
        return true;
    }
}
