using Axon.Shared.NetworkMessages;
using HarmonyLib;
using Il2CppMirror;
using MelonLoader;

namespace Axon.Shared.Patches.Mirror;

[HarmonyPatch(typeof(NetworkManager),nameof(NetworkManager.RegisterClientMessages))]
public static class NetworkManagerRegisterClientMessages
{
    [HarmonyPostfix]
    public static void OnRegister()
    {
        try
        {
            MessageHandler.RegisterMessages();
        }
        catch(Exception e)
        {
            MelonLogger.Error("Patch.Il2CppMirror.NetworkManager.RegisterClientMessages: " + e);
        }
    }
}
