using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axon.NetworkMessages;
using Axon.Server.Patches.Mirror;
using CentralAuth;
using Cryptography;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using Mirror.LiteNetLib4Mirror;

namespace Axon.Server.Patches.Auth;

[HarmonyPatch]
public static class PlayerAuthenticationManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.Start))]
    public static void Start(PlayerAuthenticationManager __instance)
    {
        if (!__instance.isLocalPlayer)
        {
            Timing.CallDelayed(1f, () =>
            {
                var endPoint = LiteNetLib4MirrorServer.Peers[__instance.connectionToClient.connectionId].EndPoint;
                var player = ProcessConnectionRequestPatch.VerifiedPlayers[endPoint];
                __instance.UserId = player.UserId;
                __instance._hub.nicknameSync.UpdateNickname(player.Name);
                __instance._hub.serverRoles.RefreshPermissions(false);
                Exiled.Events.Patches.Events.Player.Verified.Postfix(__instance);
            });
        }
    }
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAuthenticationManager),nameof(PlayerAuthenticationManager.FixedUpdate))]
    public static void AfterStart(PlayerAuthenticationManager __instance)
    {
        if (__instance.isLocalPlayer) return;
        if (__instance._authenticationRequested || !__instance.connectionToClient.isReady) return;

        var endPoint = LiteNetLib4MirrorServer.Peers[__instance.connectionToClient.connectionId].EndPoint;
        var player = ProcessConnectionRequestPatch.VerifiedPlayers[endPoint];
        __instance.UserId = player.UserId;
        __instance._hub.encryptedChannelManager.PrepareExchange();
        __instance.netIdentity.connectionToClient.Send(new PostJoinAuthMessage()
        {
            ServerRequestAuth = true,
            PublicKey = ECDSA.KeyToString(__instance._hub.encryptedChannelManager.EcdhKeys.Public)
        });
        __instance._authenticationRequested = true;
    }
    */
}
