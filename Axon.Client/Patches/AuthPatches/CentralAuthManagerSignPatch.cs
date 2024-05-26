using Il2CppCentralAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace Axon.Client.Patches.AuthPatches;

[HarmonyPatch(typeof(CentralAuthManager), nameof(CentralAuthManager.Sign))]
public class Sign
{
    [HarmonyPrefix]
    public static bool OnCentralAuthManagerSign(ref string __result, string ticket)
    {
        CentralAuthManager.Authenticated = true;
        __result = "TICKET";
        return false;
    }
}
