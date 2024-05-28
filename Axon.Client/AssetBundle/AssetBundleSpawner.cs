using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMirror;
using MelonLoader;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axon.Client.AssetBundle;

public static class AssetBundleSpawner
{
    public const uint AssetBundleMirrorId = 34324;

    public static ReadOnlyDictionary<uint, GameObject> LoadedAssets { get; private set; } = new (new Dictionary<uint, GameObject>());   

    internal static void OnSpawnMessage(SpawnMessage message)
    {
        try
        {
            if (message.netId == 0)
            {
                MelonLogger.Error("Server tried to spawn a Asset without a proper NetId. Object can't be spawned");
                return;
            }
            if (LoadedAssets.ContainsKey(message.netId))
            {
                UpdateAsset(LoadedAssets[message.netId], message);
                return;
            }

            CreateAsset(message);
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
        }
    }

    private static void CreateAsset(SpawnMessage message)
    {
        MelonLogger.Msg("Loading Asset! " + message.netId);
        /*
        var reader = new NetworkReader();
        MelonLogger.Msg("Created Reader " + (reader == null));
        MelonLogger.Msg(message.payload == null);
        var bundleName = reader.ReadString();
        var assetName = reader.ReadString();
        var name = reader.ReadString();
        MelonLogger.Msg("Read Data");

        if (!AxonMod.AssetBundleManager.AssetBundles.TryGetValue(bundleName,out var bundle))
        {
            MelonLogger.Error("Server tried to spawn a Asset from an not installed bundle");
            return;
        }

        var asset = AxonMod.AssetBundleManager.AssetBundles.First().Value.LoadAsset<GameObject>(assetName);
        var obj = GameObject.Instantiate(asset);
        obj.name = name;
        obj.transform.position = message.position;
        obj.transform.rotation = message.rotation;
        obj.transform.localScale = message.scale;


        var dic = new Dictionary<uint, GameObject>(LoadedAssets);
        dic[message.netId] = obj;
        LoadedAssets = new(dic);
        */
    }

    private static void UpdateAsset(GameObject asset, SpawnMessage message)
    {
        var transform = asset.transform;
        transform.position = message.position;
        transform.rotation = message.rotation;
        transform.localScale = message.scale;
    }
}
