using System.Collections.Generic;
using Axon.NetworkMessages;
using Axon.Server.NetworkMessages;
using HarmonyLib;
using PluginAPI.Core;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace Axon.Server.AssetBundle;

public class AxonAssetScript : MonoBehaviour
{
    public SpawnedAsset SpawnedAsset { get; internal set; }

    private Vector3 _lastPos = Vector3.zero;
    private Quaternion _lastRot = Quaternion.identity;
    private Vector3 _lastScale = Vector3.one;

    public void Awake()
    {
        _lastPos = transform.position;
        _lastRot = transform.rotation;
        _lastScale = transform.localScale;
    }

    public void LateUpdate()
    {
        SendUpdateMessage();
    }


    /// <summary>
    /// Removes a component from the asset and sends a message to all connected players to inform them of the removal.
    /// </summary>
    /// <typeparam name="T">The type of the component to remove.</typeparam>
    /// <param name="players">A list of players to send the remove component message to. If null, the message will be sent to all connected players.</param>
    public void RemoveComponent<T>() where T : MonoBehaviour
    {
        foreach (var player in Player.List)
        {
            player.Connection.Send(new RemoveAssetComponentMessage(){objectId = SpawnedAsset.Id, componentName = typeof(T).FullName});
        }
        if(!SpawnedAsset.RemovedComponents.Contains(typeof(T).FullName))
            SpawnedAsset.RemovedComponents.Add(typeof(T).FullName);
    }

    /// <summary>
    /// Removes a component from the asset and sends a message to all connected players to inform them of the removal.
    /// </summary>
    /// <typeparam name="T">The type of the component to remove.</typeparam>
    /// <param name="players">A list of players to send the remove component message to. If null, the message will be sent to all connected players.</param>
    public void RemoveComponent<T>(List<Player> players) where T : MonoBehaviour
    {
        foreach (var player in players)
        {
            player.Connection.Send(new RemoveAssetComponentMessage(){
                objectId = SpawnedAsset.Id, componentName = nameof(T)
            });
        }
    }

    /// Removes a component from the asset and sends a message to all connected players to inform them of the removal.
    /// </summary>
    /// <typeparam name="T">The type of the component to remove.</typeparam>
    /// <param name="players">A list of players to send the remove component message to. If null, the message will be sent to all connected players.</param>
    public void RemoveComponent<T>(Player player) where T : MonoBehaviour
    {
        player.Connection.Send(new RemoveAssetComponentMessage(){objectId = SpawnedAsset.Id, componentName = nameof(T)});
    }

    /// <summary>
    /// Sends an update message to all connected players to synchronize the position, rotation, and scale of the asset.
    /// </summary>
    /// <remarks>
    /// This method compares the current position, rotation, and scale of the asset with the previous values and sends an update message if any changes are detected.
    /// The update message contains the asset's object ID and the synchronization dirty bits that indicate which properties have changed.
    /// The message is sent to each connected player's connection using the Mirror networking library.
    /// </remarks>
    public void SendUpdateMessage()
    {
        var msg = new UpdateAssetMessage()
        {
            objectId = SpawnedAsset.Id,
            syncDirtyBits = 0,
            position = _lastPos,
            rotation = _lastRot,
            scale = _lastScale,
        };

        if(_lastPos != transform.position)
        {
            msg.position = transform.position;
            _lastPos = transform.position;
            msg.syncDirtyBits |= 1;
        }
        if(_lastRot != transform.rotation)
        {
            msg.rotation = transform.rotation;
            _lastRot = transform.rotation;
            msg.syncDirtyBits |= 2;
        }
        if(_lastScale != transform.localScale)
        {
            msg.scale = transform.localScale;
            _lastScale = transform.localScale;
            msg.syncDirtyBits |= 4;
        }
        if (msg.syncDirtyBits == 0) return;

        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost) continue;
            hub.connectionToClient.Send(msg);
        }
    }
}
