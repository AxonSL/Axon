using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppCommandSystem;
using MelonLoader;

namespace Axon.Client.Patches.Command;

[HarmonyPatch(typeof(Il2CppGameCore.Console),nameof(Il2CppGameCore.Console.TypeCommand))]
public class CommandHandlerTryGetCommandPatch
{
    [HarmonyPrefix]
    public static bool OnTryGetCommand(string cmd) => AxonMod.CommandHandler.OnCommand(cmd);
}
