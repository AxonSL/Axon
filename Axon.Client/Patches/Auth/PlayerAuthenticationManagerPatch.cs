using Axon.Client.Auth;
using HarmonyLib;
using Il2Cpp;
using Il2CppCentralAuth;
using Il2CppCryptography;
using MelonLoader;

namespace Axon.Client.Patches.Auth;

[HarmonyPatch]
public static class PlayerAuthenticationManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.Start))]
    public static void Start(PlayerAuthenticationManager __instance)
    {
        if (!__instance.isLocalPlayer) return;
        __instance._hub.encryptedChannelManager.EncryptionKey = AuthHandler.CurrentKey;
        MelonLogger.Warning("SHARED KEY: "+ Convert.ToBase64String(AuthHandler.CurrentKey));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EncryptedChannelManager), nameof(EncryptedChannelManager.TryPack))]
    public static void OnPack(EncryptedChannelManager __instance)
    {
        MelonLogger.Warning("SHARED KEY: " + Convert.ToBase64String(__instance.EncryptionKey));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AES), nameof(AES.AesGcmDecrypt))]
    public static void OnDecrypt(byte[] data, byte[] secret)
    {
        MelonLogger.Warning("SHARED KEY: " + Convert.ToBase64String(secret));
    }
}
