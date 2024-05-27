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
        if (message.sceneId != AssetBundleHandler.AssetBundleMirrorId) return true;
        AxonMod.AssetBundleHandler.OnSpawnMessage(message);
        return false;
    }
}
