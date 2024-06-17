using Axon.Server.Patches.Mirror;
using CentralAuth;
using Exiled.API.Features;
using HarmonyLib;
using Mirror.LiteNetLib4Mirror;
using System;

namespace Axon.Server.Patches.Auth;

[HarmonyPatch]
public static class PlayerAuthenticationManagerPatch
{
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.Start))]
    public static void Start(PlayerAuthenticationManager __instance)
    {
        if (__instance.isLocalPlayer) return;
        var endPoint = LiteNetLib4MirrorServer.Peers[__instance.connectionToClient.connectionId].EndPoint;
        var player = ProcessConnectionRequestPatch.VerifiedPlayers[endPoint];
        __instance.UserId = player.UserId;
        __instance._hub.nicknameSync.UpdateNickname(player.Name);
    }
    */

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FixedUpdate))]
    public static void FixedUpdate(PlayerAuthenticationManager __instance)
    {
        if (__instance.isLocalPlayer) return;
        if (__instance._authenticationRequested) return;
        if (!__instance.connectionToClient.isReady) return;

        var endPoint = LiteNetLib4MirrorServer.Peers[__instance.connectionToClient.connectionId].EndPoint;
        if(!AuthHandler._storedPlayers.TryGetValue(endPoint,out var player))
        {
            Log.Error("Player joines the server without being authenticated?");
            __instance.RejectAuthentication("Authentication failed Server Side", null, false);
            return;
        }

        __instance.UserId = player.UserId;
        __instance._hub.nicknameSync.UpdateNickname(player.NickName);
        __instance._hub.encryptedChannelManager.EncryptionKey = player.SharedKey;
        Log.Warn("SHARED KEY: " + Convert.ToBase64String(player.SharedKey));
        __instance._hub.serverRoles.RefreshPermissions(false);
        Exiled.Events.Patches.Events.Player.Verified.Postfix(__instance);

        __instance._authenticationRequested = true;
        AuthHandler._storedPlayers.Remove(endPoint);
    }
}
