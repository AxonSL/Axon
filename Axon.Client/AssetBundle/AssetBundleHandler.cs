using Il2CppMirror;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axon.Client.AssetBundle;

public class AssetBundleHandler
{
    public const uint AssetBundleMirrorId = 34324;

    public ReadOnlyDictionary<uint, GameObject> LoadedAssets { get; private set; } = new (new Dictionary<uint, GameObject>());   

    internal void OnSpawnMessage(SpawnMessage message)
    {
        if(message.netId == 0)
        {
            MelonLogger.Error("Server tried to spawn a Prefab without a proper NetId. Object can't be spawned");
            return;
        }
        if (LoadedAssets.ContainsKey(message.netId))
        {
            UpdateAsset(LoadedAssets[message.netId], message);
            return;
        }

        CreateAsset(message);
    }

    private void CreateAsset(SpawnMessage message)
    {
        MelonLogger.Msg("Loading Asset! " + message.netId);
        
        var asset = AxonMod.AssetBundleManager.AssetBundles.First().Value.LoadAsset<GameObject>("Assets/v1.prefab");
        var obj = GameObject.Instantiate(asset);
        obj.name = "Axon Asset";
        obj.transform.position = message.position;
        obj.transform.rotation = message.rotation;
        obj.transform.localScale = message.scale;


        var dic = new Dictionary<uint, GameObject>(LoadedAssets);
        dic[message.netId] = obj;
        LoadedAssets = new(dic);
    }

    private void UpdateAsset(GameObject asset, SpawnMessage message)
    {
        var transform = asset.transform;
        transform.position = message.position;
        transform.rotation = message.rotation;
        transform.localScale = message.scale;
    }
}
