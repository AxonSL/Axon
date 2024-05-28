using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Command;

public static class CommandHandler
{
    public static bool OnCommand(string command)
    {
        MelonLogger.Msg("Command: " + command);
        return true;
    }
}
