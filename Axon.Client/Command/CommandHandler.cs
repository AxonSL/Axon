using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Command;

public class CommandHandler
{
    public bool OnCommand(string command)
    {
        MelonLogger.Msg("Command: " + command);
        return true;
    }
}
