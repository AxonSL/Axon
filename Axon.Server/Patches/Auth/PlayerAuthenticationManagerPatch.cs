using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axon.Server.Patches.Mirror;
using CentralAuth;
using HarmonyLib;
using Mirror.LiteNetLib4Mirror;

namespace Axon.Server.Patches.Auth;

[HarmonyPatch]
public static class PlayerAuthenticationManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerAuthenticationManager),nameof(PlayerAuthenticationManager.Start))]
    public static void AfterSart(PlayerAuthenticationManager __instance)
    {
        if(!__instance.isLocalPlayer)
        {
            var endPoint = LiteNetLib4MirrorServer.Peers[__instance.connectionToClient.connectionId].EndPoint;
            var player = ProcessConnectionRequestPatch.VerifiedPlayers[endPoint];
            __instance.UserId = player.UserId;
            __instance._hub.nicknameSync.UpdateNickname(player.Name);
        }
    }
}
