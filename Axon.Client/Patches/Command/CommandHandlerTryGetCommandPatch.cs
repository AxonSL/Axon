using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppCommandSystem;
using MelonLoader;

namespace Axon.Shared.Patches.Command;

[HarmonyPatch(typeof(Il2CppGameCore.Console),nameof(Il2CppGameCore.Console.TypeCommand))]
public class CommandHandlerTryGetCommandPatch
{
    [HarmonyPrefix]
    public static bool OnTryGetCommand(string cmd)
    {
        try
        {
            return !Axon.Shared.Command.CommandHandler.OnCommand(cmd);
        }
        catch(Exception e)
        {
            MelonLogger.Msg("Patch.Il2CppGameCore.Console.TypeCommand: " + e);
            return true;
        }
    }
}
