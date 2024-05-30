using Axon.NetworkMessages;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityAssetBundle = UnityEngine.AssetBundle;

namespace Axon.Server.AssetBundle;

public static class AssetBundleSpawner
{
    private static uint _counter = 0;

    public static string AssetBundlesPath { get; private set; }

    public static ReadOnlyDictionary<string, UnityAssetBundle> AssetBundles { get; private set; } = new(new Dictionary<string, UnityAssetBundle>());

    public static ReadOnlyDictionary<uint, GameObject> SpawnedGameObjects { get; private set; } = new(new Dictionary<uint, GameObject>());

    internal static void Init()
    {
        AssetBundlesPath = Path.Combine(Exiled.API.Features.Paths.Exiled, "AssetBundles");
        if (!Directory.Exists(AssetBundlesPath))
            Directory.CreateDirectory(AssetBundlesPath);

        var dic = new Dictionary<string, UnityAssetBundle>(AssetBundles);

        foreach (var file in Directory.GetFiles(AssetBundlesPath))
        {
            var assetBundle = UnityAssetBundle.LoadFromFile(file);
            var name = Path.GetFileNameWithoutExtension(file);
            dic[name] = assetBundle;
        }

        AssetBundles = new(dic);
    }


    public static GameObject SpawnAsset(string bundle, string asset, string gameObjectName = "Axon Asset")
        => SpawnAsset(bundle, asset, gameObjectName, Vector3.zero, Quaternion.identity, Vector3.one);

    public static GameObject SpawnAsset(string bundle, string asset, string gameObjectName, Vector3 position)
        => SpawnAsset(bundle, asset, gameObjectName, position, Quaternion.identity, Vector3.one);

    public static GameObject SpawnAsset(string bundle, string asset, string gameObjectName, Vector3 position, Quaternion rotation)
        => SpawnAsset(bundle, asset, gameObjectName, position, rotation, Vector3.one);

    public static GameObject SpawnAsset(string bundle, string asset, string gameObjectName, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (!AssetBundles.TryGetValue(bundle, out var assetBundle))
        {
            Log.Warn("Server tried to spawn a Bundle that isn't installed: " + bundle);
            return null;
        }

        if (!assetBundle.GetAllAssetNames().Contains(asset.ToLower()))
        {
            Log.Warn($"Server tried to spawn the Asset {asset} from the bundle {bundle}, but it is not part of the bundle");
            return null;
        }

        var prefab = assetBundle.LoadAsset<GameObject>(asset.ToLower());
        var gameObject = SpawnGameObject(prefab, bundle, asset, gameObjectName, position, rotation, scale);
        return gameObject;
    }

    //TODO: Send object Data again once someone new joins
    private static GameObject SpawnGameObject(GameObject prefab, string bundle, string asset, string name, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        //Create Gameobject
        var obj = UnityEngine.Object.Instantiate(prefab);
        obj.name = name;
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.localScale = scale;
        obj.SetActive(true);

        //Register Server Side
        _counter++;
        var id = _counter;
        var dic = new Dictionary<uint, GameObject>(SpawnedGameObjects);
        dic[id] = obj;
        SpawnedGameObjects = new(dic);

        //Register Client Side
        var msg = new SpawnAssetMessage()
        {
            objectId = id,
            bundleName = bundle,
            assetName = asset,
            gameObjectName = name,
            position = position,
            rotation = rotation,
            scale = scale
        };
        foreach(var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost) continue;
            Log.Info(hub.nicknameSync.Network_myNickSync);
            hub.connectionToClient.Send(msg);
        }

        return obj;
    }
}
