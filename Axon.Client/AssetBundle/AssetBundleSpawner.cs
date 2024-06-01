using MelonLoader;
using System.Collections.ObjectModel;
using UnityEngine;
using Axon.NetworkMessages;
using Object = UnityEngine.Object;

namespace Axon.Client.AssetBundle;

public static class AssetBundleSpawner
{
    //TODO: Clear this on RoundRestart
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
            if (LoadedAssets.ContainsKey(message.objectId) && LoadedAssets[message.objectId] != null)
            {
                MelonLogger.Warning("Server tried to spawn an already existing object");
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
        MelonLogger.Msg("Loading Asset with objectID: " + message.objectId);

        if (!AssetBundleManager.AssetBundles.TryGetValue(message.bundleName, out var bundle)) 
        {
            MelonLogger.Error("Server tried to spawn a Asset from an not installed bundle");
            return;
        }

        var asset = bundle.LoadAsset<GameObject>(message.assetName);
        var obj = Object.Instantiate(asset);
        obj.name = message.gameObjectName;
        obj.transform.position = message.position;
        obj.transform.rotation = message.rotation;
        obj.transform.localScale = message.scale;

        var dic = new Dictionary<uint, GameObject>(LoadedAssets);
        dic[message.objectId] = obj;
        LoadedAssets = new(dic);
    }

    internal static void UpdateAsset(UpdateAssetMessage message)
    {
        try
        {
            if(!LoadedAssets.TryGetValue(message.objectId, out var asset))
            {
                MelonLogger.Warning("Server tried to update an non existing object");
                return;
            }

            var transform = asset.transform;

            if ((message.syncDirtyBits & 1) == 1)
                transform.position = message.position;

            if ((message.syncDirtyBits & 2) == 2)
                transform.rotation = message.rotation;

            if ((message.syncDirtyBits & 4) == 4)
                transform.localScale = message.scale;
        }
        catch(Exception e)
        {
            MelonLogger.Error("Axon.Client.AssetBundle.AssetBundleSpawner failed: " + e);
        }
    }
}
