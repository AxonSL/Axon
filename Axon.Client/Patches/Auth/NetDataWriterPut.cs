using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppLiteNetLib;
using Il2CppLiteNetLib.Utils;
using Il2CppMirror.LiteNetLib4Mirror;
using MelonLoader;
using static Il2CppSystem.Globalization.CultureInfo;

namespace Axon.Client.Patches.Auth;

[HarmonyPatch]
public static class NetDataWriterPut
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),nameof(CustomLiteNetLib4MirrorTransport.GetConnectData))]
    [HarmonyPrefix]
    public static bool OnPrefix1(NetDataWriter writer)
    {
        AuthHandler.AuthWrite(writer);
        return false;
    }

    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.OnConncetionRefused))]
    [HarmonyPrefix]
    public static bool ConnectionRefused(DisconnectInfo disconnectinfo)
    {
        AuthHandler.RejectAuth(disconnectinfo);
        return true;
    }
}
