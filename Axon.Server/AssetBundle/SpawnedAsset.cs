using System;
using System.Collections.Generic;
using Axon.NetworkMessages;
using Exiled.API.Features;
using UnityEngine;

namespace Axon.Server.AssetBundle;

public class SpawnedAsset
{
    public GameObject GameObject { get; internal set; }
    public AxonAssetScript Script { get; internal set; }
    public uint Id { get; internal set; }
    public string Bundle {  get; internal set; }
    public string AssetName { get; internal set; }
    public string[] Components { get; internal set; }
    public List<string> RemovedComponents { get; internal set; }
    internal void RemoveAllComponents(Player player)
    {
        foreach (var s in RemovedComponents)
        {
            player.Connection.Send(new RemoveAssetComponentMessage(){objectId = Id, componentName = s});
        }
    }

}
