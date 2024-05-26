using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppCommandSystem;
using MelonLoader;

namespace Axon.Client.Patches.Command;

[HarmonyPatch(typeof(CommandHandler),nameof(CommandHandler.TryGetCommand))]
public class CommandHandlerTryGetCommandPatch
{
    [HarmonyPrefix]
    public static bool OnTryGetCommand(string query)
    {
        MelonLogger.Msg("Command: " + query);
        return true;
    }
}
