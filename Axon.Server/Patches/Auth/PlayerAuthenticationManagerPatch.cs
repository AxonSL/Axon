using Axon.Server.Patches.Mirror;
using CentralAuth;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using System;
using System.Text;
using UnityEngine;
using static EncryptedChannelManager;

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
            Log.Error("Player joined the server without being authenticated?");
            __instance.RejectAuthentication("Authentication failed Server Side", null, false);
            return;
        }

        __instance._authenticationRequested = true;
        AuthHandler._storedPlayers.Remove(endPoint);

        __instance.UserId = player.UserId;
        __instance._hub.nicknameSync.UpdateNickname(player.NickName);
        __instance._hub.encryptedChannelManager.EncryptionKey = player.SharedKey;
        Timing.CallDelayed(1f, () =>
        {
            __instance._hub.serverRoles.RefreshPermissions(false);
            Exiled.Events.Patches.Events.Player.Verified.Postfix(__instance);
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EncryptedChannelManager), nameof(EncryptedChannelManager.TrySendMessageToClient), new Type[] { typeof(string), typeof(EncryptedChannelManager.EncryptedChannel) })]
    public static bool OnPack(EncryptedChannelManager __instance, out bool __result,
        string content, EncryptedChannelManager.EncryptedChannel channel)
    {
        if (__instance._txCounter == 4294967295U)
            __instance._txCounter = 0;
        __instance._txCounter++;

        EncryptedChannelManager.EncryptedMessageOutside messageOut;

        var data = new byte[Misc.Utf8Encoding.GetByteCount(content)+5];
        data[0] = (byte)channel;
        BitConverter.GetBytes(__instance._txCounter).CopyTo(data, 1);
        Encoding.UTF8.GetBytes(content, 0, content.Length, data, 5);

        if (__instance.isLocalPlayer)
        {
            messageOut = new EncryptedChannelManager.EncryptedMessageOutside(EncryptedChannelManager.SecurityLevel.Unsecured, data);
        }
        else
        {
            if (__instance.EncryptionKey == null)
            {
                Log.Warn("Tried to send encrypted message, but no key was found");
                __result = false;
                return false;
            }
            var encryptedData = data;
            messageOut = new EncryptedChannelManager.EncryptedMessageOutside(EncryptedChannelManager.SecurityLevel.EncryptedAndAuthenticated, encryptedData);
        }

        __instance.connectionToClient.Send(messageOut);
        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EncryptedChannelManager), nameof(EncryptedChannelManager.ServerReceivePackedMessage))]
    public static bool OnReceivePackedMessage(NetworkConnection conn, EncryptedChannelManager.EncryptedMessageOutside packed)
    {
        if (!ReferenceHub.TryGetHub(conn.identity.gameObject, out var hub))
            return false;

        EncryptedChannel channel;
        uint counter;
        string content;

        if (conn.identity.isLocalPlayer)
        {
            channel = (EncryptedChannel)packed.Data[0];
            counter = BitConverter.ToUInt32(packed.Data, 1);
            content = Encoding.UTF8.GetString(packed.Data, 5, packed.Data.Length - 5);
        }
        else
        {
            if (hub.encryptedChannelManager.EncryptionKey == null)
            {
                Log.Warn("Got EncryptedMessage eventhough no Key was set");
                return false;
            }

            var decryptedData = packed.Data;

            channel = (EncryptedChannel)decryptedData[0];
            counter = BitConverter.ToUInt32(decryptedData, 1);
            content = Encoding.UTF8.GetString(decryptedData, 5, decryptedData.Length - 5);

            if (hub.encryptedChannelManager._rxCounter == 4294967295U)
                hub.encryptedChannelManager._rxCounter = 0;

            if (counter <= hub.encryptedChannelManager._rxCounter)
            {
                GameCore.Console.AddLog(string.Format("Received message with counter {0}, which is lower or equal to last received message counter {1}. Discarding message!", counter, hub.encryptedChannelManager._rxCounter), Color.red, false, GameCore.Console.ConsoleLogType.Log);
                return false;
            }

            hub.encryptedChannelManager._rxCounter = counter;
        }


        if (!EncryptedChannelManager.ServerChannelHandlers.ContainsKey(channel))
        {
            GameCore.Console.AddLog(string.Format("No handler is registered for encrypted channel {0} (server).", channel), Color.red, false, GameCore.Console.ConsoleLogType.Log);
            return false;
        }

        try
        {
            EncryptedChannelManager.ServerChannelHandlers[channel].Invoke(hub, content, packed.Level);
        }
        catch (Exception ex)
        {
            GameCore.Console.AddLog(string.Format("Exception while handling encrypted message on channel {0} (server, running a handler). Exception: {1}", channel, ex.Message), Color.red, false, GameCore.Console.ConsoleLogType.Log);
            GameCore.Console.AddLog(ex.StackTrace, Color.red, false, GameCore.Console.ConsoleLogType.Log);
        }
        return false;
    }
}
