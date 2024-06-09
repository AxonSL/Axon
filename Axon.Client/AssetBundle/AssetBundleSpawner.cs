using MelonLoader;
using System.Collections.ObjectModel;
using UnityEngine;
using Axon.NetworkMessages;
using Object = UnityEngine.Object;
using Axon.Shared.CustomScripts;
using Axon.Shared.Meta;
using Axon.Client.AssetBundle.CustomScript;
using Il2CppInterop.Runtime;
using Il2CppGameCore;
using Il2CppMirror;

namespace Axon.Client.AssetBundle;

public static class AssetBundleSpawner
{
    public const string AssetDirectoryName = "AssetBundles";
    public static string AssetDirectory { get; private set; }

    public static ReadOnlyDictionary<string, Il2CppAssetBundle> AssetBundles { get; private set; } = new(new Dictionary<string, Il2CppAssetBundle>());
    //TODO: Clear this on Disconnect
    public static ReadOnlyCollection<SpawnedAsset> SpawnedAssets { get; internal set; } = new (new List<SpawnedAsset>());
    public static ReadOnlyDictionary<string, Il2CppSystem.Type> CustomComponents { get; private set; } = new(new Dictionary<string, Il2CppSystem.Type>());



    internal static void Init()
    {
        MetaAnalyzer.OnMeta.Subscribe(OnMeta);

        var current = Directory.GetCurrentDirectory();
        AssetDirectory = Path.Combine(current, AssetDirectoryName);
        if (!Directory.Exists(AssetDirectory))
            Directory.CreateDirectory(AssetDirectory);

        LoadAssets();
    }

    internal static void OnSpawnAssetMessage(SpawnAssetMessage message)
    {
        try
        {
            if (message.objectId == 0)
            {
                MelonLogger.Error("Server tried to spawn a Asset without a proper NetId. Object can't be spawned");
                return;
            }
            var spawned = SpawnedAssets.FirstOrDefault(x => x.Id == message.objectId);
            if (spawned != null && spawned.GameObject != null)
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

    internal static void OnSyncVarMessage(SyncVarMessage msg)
    {
        try
        {
            if (msg.objectId == 0) return;
            foreach (var spawnedAsset in SpawnedAssets)
            {
                if (msg.objectId != spawnedAsset.Id) continue;

                if (!CustomComponents.TryGetValue(msg.scriptName, out var type))
                {
                    MelonLogger.Warning("Server tried to update a SyncVar of an Component that is not registered Client side: " + msg.scriptName);
                    return;
                }

                if (!spawnedAsset.GameObject.TryGetComponent(type, out var comp))
                {
                    MelonLogger.Warning("Servertried to update a SyncVar of a component that the gameobject doesn't own client side");
                    return;
                }

                var axonComp = comp.Cast<AxonCustomScript>();

                if (axonComp == null)
                {
                    MelonLogger.Warning("Server tried to update a SyncVar of a component that somehow is not an AxonScript?");
                    return;
                }

                axonComp.ReceiveMessage(msg);

                return;
            }
        }
        catch (Exception e)
        {
            MelonLogger.Error("SyncVarMessage failed: " + e);
        }
    }

    internal static void UpdateAsset(UpdateAssetMessage message)
    {
        try
        {
            var spawned = SpawnedAssets.FirstOrDefault(x => x.Id == message.objectId);
            if(spawned == null || spawned.GameObject == null)
            {
                MelonLogger.Warning("Server tried to update an non existing object");
                return;
            }

            var asset = spawned.GameObject;

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



    private static void OnMeta(MetaEvent ev)
    {
        if (!ev.Is<AxonCustomScript>()) return;
        var attribute = ev.GetAttribute<AxonScriptAttribute>();
        if (attribute == null) return;
        var dic = new Dictionary<string, Il2CppSystem.Type>(CustomComponents);
        dic[attribute.UniqueName] = Il2CppType.From(ev.Type);
        CustomComponents = new(dic);
    }

    private static void CreateAsset(SpawnAssetMessage message)
    {
        MelonLogger.Msg("Loading Asset with objectID: " + message.objectId);
        GameObject obj;

        if(message.bundleName == "default" && message.assetName == "empty")
        {
            obj = new GameObject();
        }
        else
        {
            if (!AssetBundles.TryGetValue(message.bundleName, out var bundle))
            {
                MelonLogger.Warning("Server tried to spawn a Asset from an not installed bundle");
                return;
            }

            if (!bundle.GetAllAssetNames().Contains(message.assetName.ToLower()))
            {
                MelonLogger.Warning($"Server tried to spawn the Asset {message.assetName} from the bundle {message.bundleName}, but it is not part of the bundle");
                return;
            }

            var asset = bundle.LoadAsset<GameObject>(message.assetName);
            obj = Object.Instantiate(asset);
        }
        obj.name = message.gameObjectName;
        obj.transform.position = message.position;
        obj.transform.rotation = message.rotation;
        obj.transform.localScale = message.scale;

        var list = new List<SpawnedAsset>();
        var spawned = new SpawnedAsset
        {
            Id = message.objectId,
            GameObject = obj,
            Bundle = message.bundleName,
            AssetName = message.assetName,
            Components = message.components,
        };
        spawned.Script = obj.AddComponent<AxonAssetScript>();
        spawned.Script.SpawnedAsset = spawned;
        list.Add(spawned);
        SpawnedAssets = new(list);
        foreach (var component in message.components)
        {
            if (!CustomComponents.ContainsKey(component))
            {
                MelonLogger.Warning("Server tried to add a component that isn't registered client side: " + component);
                return;
            }
            var t = CustomComponents[component];
            var customComp = obj.AddComponent(t).Cast<AxonCustomScript>();
            customComp.AxonAssetScript = spawned.Script;
            customComp.UniqueName = component;
        }

        if (message.componetsData.Count > 0)
            ApplySyncVars(message, obj);
    }

    private static void ApplySyncVars(SpawnAssetMessage message, GameObject gameObject)
    {
        var reader = NetworkReaderPool.Get(message.componetsData);
        var count = reader.ReadUShort();

        for (var i = 0; i < count; i++)
        {
            var compName = reader.ReadString();
            if (!CustomComponents.TryGetValue(compName, out var compType))
            {
                MelonLogger.Warning("Server tried to update a SyncVar of a component that is not registered client side");
                break;
            }

            var component = gameObject?.GetComponent(compType)?.Cast<AxonCustomScript>();
            if (component == null)
            {
                MelonLogger.Warning("Server tried to update a SyncVar of a component it forgot to add to the gameobject?");
                break;
            }
            component.ReadAllSyncVar(reader);
        }
    }

    private static void LoadAssets()
    {
        foreach (var assetPath in Directory.GetFiles(AssetDirectory))
        {
            var asset = Il2CppAssetBundleManager.LoadFromFile(assetPath);
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var dic = new Dictionary<string, Il2CppAssetBundle>(AssetBundles);
            dic[fileName] = asset;
            AssetBundles = new(dic);
        }
    }
}
