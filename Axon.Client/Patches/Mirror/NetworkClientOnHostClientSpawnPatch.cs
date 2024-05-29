using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axon.Client.AssetBundle;
using HarmonyLib;
using Il2CppMirror;
using MelonLoader;
using UnityEngine;

namespace Axon.Client.Patches.Mirror;

[HarmonyPatch(typeof(NetworkClient),nameof(NetworkClient.OnSpawn))]
public static class NetworkClientOnHostClientSpawnPatch
{
    [HarmonyPrefix]
    public static bool OnSpawnMessage(SpawnMessage message)
    {
        try
        {
            if (message.sceneId != AssetBundleSpawner.AssetBundleMirrorId) return true;
            AssetBundleSpawner.OnSpawnMessage(message);
            return false;
        }
        catch(Exception e)
        {
            MelonLogger.Msg("Patch.Il2cppMirror.NetworkClient.OnSpawn: " + e);
            return true;
        }
    }
}
