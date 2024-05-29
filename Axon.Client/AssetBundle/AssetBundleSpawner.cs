using MelonLoader;
using System.Collections.ObjectModel;
using UnityEngine;
using Axon.NetworkMessages;

namespace Axon.Client.AssetBundle;

public static class AssetBundleSpawner
{
    public static ReadOnlyDictionary<uint, GameObject> LoadedAssets { get; private set; } = new (new Dictionary<uint, GameObject>());   

    internal static void OnSpawnAssetMessage(SpawnAssetMessage message)
    {
        try
        {
            if (message.objectId == 0)
            {
                MelonLogger.Error("Server tried to spawn a Asset without a proper NetId. Object can't be spawned");
                return;
            }
            if (LoadedAssets.ContainsKey(message.objectId))
            {
                UpdateAsset(LoadedAssets[message.objectId], message);
                return;
            }

            CreateAsset(message);
        }
        catch (Exception e)
        {
            MelonLogger.Error(e);
        }
    }

    private static void CreateAsset(SpawnAssetMessage message)
    {
        MelonLogger.Msg("Loading Asset! " + message.objectId);

        if (!AssetBundleManager.AssetBundles.TryGetValue(message.bundleName, out var bundle)) 
        {
            MelonLogger.Error("Server tried to spawn a Asset from an not installed bundle");
            return;
        }

        var asset = bundle.LoadAsset<GameObject>(message.assetName);
        var obj = GameObject.Instantiate(asset);
        obj.name = message.assetName;
        obj.transform.position = message.position;
        obj.transform.rotation = message.rotation;
        obj.transform.localScale = message.scale;


        var dic = new Dictionary<uint, GameObject>(LoadedAssets);
        dic[message.objectId] = obj;
        LoadedAssets = new(dic);
    }

    private static void UpdateAsset(GameObject asset, SpawnAssetMessage message)
    {
        var transform = asset.transform;
        transform.position = message.position;
        transform.rotation = message.rotation;
        transform.localScale = message.scale;
    }
}
