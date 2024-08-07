﻿using Axon.NetworkMessages;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
using Axon.Shared.Meta;
using UnityAssetBundle = UnityEngine.AssetBundle;
using Axon.Server.AssetBundle.CustomScript;
using System;
using Axon.Shared.CustomScripts;
using Mirror;
using System.Linq;

namespace Axon.Server.AssetBundle;

public static class AssetBundleSpawner
{
    private static uint _counter = 0;
    public static string AssetBundlesPath { get; private set; }

    public static ReadOnlyDictionary<string, UnityAssetBundle> AssetBundles { get; private set; } = new(new Dictionary<string, UnityAssetBundle>());
    public static ReadOnlyCollection<SpawnedAsset> SpawnedAssets { get; private set; } = new(new List<SpawnedAsset>());
    public static ReadOnlyDictionary<string, Type> CustomComponents { get; private set; } = new(new Dictionary<string, Type>());


    /// <summary>
    /// Spawns an asset from an asset bundle.
    /// </summary>
    /// <param name="bundle">The name of the asset bundle.</param>
    /// <param name="asset">The name of the asset within the bundle.</param>
    /// <param name="gameObjectName">The name of the game object.</param>
    /// <param name="position">The position of the spawned game object.</param>
    /// <param name="rotation">The rotation of the spawned game object.</param>
    /// <param name="scale">The scale of the spawned game object.</param>
    /// <param name="components">The names of the components to attach to the spawned game object.</param>
    /// <returns>The spawned game object's AxonAssetScript component.</returns>
    public static AxonAssetScript SpawnAsset(string bundle, string asset, string gameObjectName = "Axon Asset", params string[] components)
    => SpawnAsset(bundle, asset, gameObjectName, Vector3.zero, Quaternion.identity, Vector3.one, components);

    /// <summary>
    /// Spawns an asset from an asset bundle.
    /// </summary>
    /// <param name="bundle">The name of the asset bundle.</param>
    /// <param name="asset">The name of the asset within the bundle.</param>
    /// <param name="gameObjectName">The name of the game object.</param>
    /// <param name="position">The position of the spawned game object.</param>
    /// <param name="rotation">The rotation of the spawned game object.</param>
    /// <param name="scale">The scale of the spawned game object.</param>
    /// <param name="components">The names of the components to attach to the spawned game object.</param>
    /// <returns>The spawned game object's AxonAssetScript component.</returns>
    public static AxonAssetScript SpawnAsset(string bundle, string asset, string gameObjectName, Vector3 position, params string[] components)
        => SpawnAsset(bundle, asset, gameObjectName, position, Quaternion.identity, Vector3.one, components);

    /// Spawns an asset from an asset bundle
    /// </summary>
    /// <param name="bundle">The name of the asset bundle.</param>
    /// <param name="asset">The name of the asset within the bundle.</param>
    /// <param name="gameObjectName">The name of the game object</param>
    /// <param name="position">The position of the spawned game object.</param>
    /// <param name="rotation">The rotation of the spawned game object.</param>
    /// <param name="scale">The scale of the spawned game object.</param>
    /// <param name="components">The names of the components to attach to the spawned game object.</param>
    /// <returns>The spawned game object's AxonAssetScript component.</returns>
    public static AxonAssetScript SpawnAsset(string bundle, string asset, string gameObjectName, Vector3 position, Quaternion rotation, params string[] components)
        => SpawnAsset(bundle, asset, gameObjectName, position, rotation, Vector3.one, components);

    /// <summary>
    /// Spawns an asset from an asset bundle
    /// </summary>
    /// <param name="bundle">The name of the asset bundle.</param>
    /// <param name="asset">The name of the asset within the bundle.</param>
    /// <param name="gameObjectName">The name of the game object</param>
    /// <param name="position">The position of the spawned game object.</param>
    /// <param name="rotation">The rotation of the spawned game object.</param>
    /// <param name="scale">The scale of the spawned game object.</param>
    /// <param name="components">The names of the components to attach to the spawned game object.</param>
    /// <returns>The spawned game object's AxonAssetScript component.</returns>
    public static AxonAssetScript SpawnAsset(string bundle, string asset, string gameObjectName, Vector3 position, Quaternion rotation, Vector3 scale, params string[] components)
    {
        GameObject obj;
        if (bundle == "default" && asset == "empty")
        {
            obj = new GameObject();
        }
        else
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
            obj = UnityEngine.Object.Instantiate(prefab);
        }
        var comp = SpawnGameObject(obj, bundle, asset, gameObjectName, position, rotation, scale, components);
        return comp;
    }



    internal static void Init()
    {
        MetaAnalyzer.OnMeta.Subscribe(OnMeta);

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

    internal static void OnJoin(JoinedEventArgs ev)
    {
        foreach(var spawned in SpawnedAssets)
        {
            var msg = new SpawnAssetMessage()
            {
                objectId = spawned.Id,
                bundleName = spawned.Bundle,
                assetName = spawned.AssetName,
                gameObjectName = spawned.GameObject.name,
                position = spawned.GameObject.transform.position,
                rotation = spawned.GameObject.transform.rotation,
                scale = spawned.GameObject.transform.localScale,
                components = spawned.Components,
            };

            var allComps = spawned.GameObject.GetComponents<AxonCustomScript>().Where(x => x.SyncVars.Count > 0);
            var writer = NetworkWriterPool.Get();
            writer.WriteUShort((ushort)allComps.Count());
            
            foreach (var component in allComps)
            {
                writer.WriteString(component.UniqueName);
                component.WriteAll(writer);
            }

            msg.componetsData = writer.ToArraySegment();
            ev.Player.Connection.Send(msg);
            NetworkWriterPool.Return(writer);
  
            spawned.RemoveAllComponents(ev.Player);
        }
    }

    internal static void OnRoundRestart()
    {
        SpawnedAssets = new(new List<SpawnedAsset>());
    }

    internal static void OnSyncVarMessage(SyncVarMessage msg)
    {
        try
        {
            if (msg.objectId == 0) return;
            foreach (var spawnedAsset in SpawnedAssets)
            {
                if (msg.objectId != spawnedAsset.Id) continue;

                if(!CustomComponents.TryGetValue(msg.scriptName, out var type))
                {
                    Log.Warn("Client tried to update a SyncVar of an Component that is not registered Server side: " + msg.scriptName);
                    return;
                }

                if(!spawnedAsset.GameObject.TryGetComponent(type,out var comp))
                {
                    Log.Warn("Client tried to update a SyncVar of a component that the gameobject doesn't own server side");
                    return;
                }

                var axonComp = comp as AxonCustomScript;

                if(axonComp == null)
                {
                    Log.Warn("Client tried to update a SyncVar of a component that somehow is not an AxonScript?");
                    return;
                }

                axonComp.ReceiveMessage(msg);
                
                return;
            }
        }
        catch(Exception e)
        {
            Log.Error("SyncVarMessage failed: " + e);
        }
    }




    private static void OnMeta(MetaEvent ev)
    {
        if (!ev.Is<AxonCustomScript>()) return;
        var attribute = ev.GetAttribute<AxonScriptAttribute>();
        if (attribute == null) return;
        var dic = new Dictionary<string, Type>(CustomComponents);
        dic[attribute.UniqueName] = ev.Type;
        CustomComponents = new(dic);
    }

    private static AxonAssetScript SpawnGameObject(GameObject obj, string bundle, string asset, string name, Vector3 position, Quaternion rotation, Vector3 scale, params string[] components)
    {
        //Setup Gameobject
        obj.name = name;
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.transform.localScale = scale;

        //Register Server Side
        _counter++;
        var id = _counter;
        var comp = obj.AddComponent<AxonAssetScript>();
        var spawnedAsset = new SpawnedAsset
        {
            Id = id,
            Bundle = bundle,
            AssetName = asset,
            GameObject = obj,
            Script = comp,
            Components = components,
            RemovedComponents = new(),
        };
        comp.SpawnedAsset = spawnedAsset;
        var list = new List<SpawnedAsset>(SpawnedAssets)
        {
            spawnedAsset
        };
        SpawnedAssets = new(list);
        foreach(var component in components)
        {
            //It doesn't throw a error so that you can add a component only client side
            if (!CustomComponents.ContainsKey(component)) continue;
            var type = CustomComponents[component];
            var customComp = (AxonCustomScript)obj.AddComponent(type);
            customComp.AxonAssetScript = comp;
            customComp.UniqueName = component;
        }

        //Register Client Side
        var msg = new SpawnAssetMessage()
        {
            objectId = id,
            bundleName = bundle,
            assetName = asset,
            gameObjectName = name,
            position = position,
            rotation = rotation,
            scale = scale,
            components = components,
            componetsData = new ArraySegment<byte>(),
        };
        foreach(var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost) continue;
            hub.connectionToClient.Send(msg);
        }

        return comp;
    }
}
