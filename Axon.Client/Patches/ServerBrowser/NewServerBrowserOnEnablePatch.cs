using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;

namespace Axon.Client.Patches.ServerBrowser;

[HarmonyPatch(typeof(NewServerBrowser), nameof(NewServerBrowser.OnEnable))]
public class NewServerBrowserOnEnablePatch
{
    [HarmonyPrefix]
    public static bool OnServerListEnable(NewServerBrowser __instance)
    {
        MelonLogger.Msg("ServerList Enable!");
        var filter = __instance.GetComponent<ServerFilter>();
        var gameObject = UnityEngine.GameObject.Find("New Main Menu/Servers/Auth Status");
        gameObject.SetActive(false);
        return true;
    }
}
