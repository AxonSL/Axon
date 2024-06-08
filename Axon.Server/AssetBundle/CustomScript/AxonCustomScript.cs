using Axon.NetworkMessages;
using Axon.Shared.CustomScripts;
using Exiled.API.Features;
using Mirror;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axon.Server.AssetBundle.CustomScript;

public abstract class AxonCustomScript : MonoBehaviour
{
    private ulong _syncDirtyBits = 0;

    public string UniqueName { get; internal set; }

    public AxonAssetScript AxonAssetScript { get; internal set; }

    public ReadOnlyDictionary<AxonSyncVarId, bool> SyncVars {  get; private set; }

    public virtual void Awake()
    {
        var dic = new Dictionary<AxonSyncVarId, bool>();
        foreach(var field in GetType().GetFields())
        {
            var attribute = field.GetCustomAttribute<AxonSyncVarAttribute>();
            if (attribute == null) continue;
            dic[attribute.SyncVarId] = attribute.ServerOnly;
        }
        SyncVars = new(dic);
    }

    public virtual void LateUpdate()
    {
        if (_syncDirtyBits == 0) return;
        var msg = new SyncVarMessage
        {
            objectId = AxonAssetScript.SpawnedAsset.Id,
            scriptName = UniqueName,
            syncDirtyBits = _syncDirtyBits,
        };

        var writer = new NetworkWriter();

        foreach(var syncVar in SyncVars.Keys)
        {
            if (((ulong)syncVar | _syncDirtyBits) == 0) continue;
            WriteSyncVar(syncVar, writer);
        }
        msg.data = writer.ToArraySegment();

        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost) continue;
            hub.connectionToClient.Send(msg);
        }
        _syncDirtyBits = 0;
    }

    protected virtual void WriteSyncVar(AxonSyncVarId syncVarId, NetworkWriter writer) { }

    protected virtual void ReadSyncVar(AxonSyncVarId syncVarId, NetworkReader reader) { }

    public virtual bool CheckIfAllowed(AxonSyncVarId syncVar, NetworkConnection connection) => true;

    public void UpdateSyncVar(AxonSyncVarId syncVarId)
    {
        _syncDirtyBits += (ulong)syncVarId;
    }

    internal void ReceiveMessage(SyncVarMessage message)
    {
        var dirty = message.syncDirtyBits;
        var reader = new NetworkReader(message.data);
        foreach(var id in (AxonSyncVarId[])Enum.GetValues(typeof(AxonSyncVarId)))
        {
            if ((dirty & (ulong)id) == 0) continue;

            if(!SyncVars.TryGetValue(id, out var serverOnly))
            {
                Log.Warn("Client tried to update a sync var of the component " + UniqueName + " that does not exist server side. Requested Sync Var Id:" + id.ToString());
                return;
            }

            if (serverOnly || !CheckIfAllowed(id, message.connection))
            {
                Log.Warn("Client tried to change a syncVar it is not allowed to change");
                return;
            }

            ReadSyncVar(id, reader);
        }
    }

    internal void WriteAll(NetworkWriter writer)
    {
        foreach(var  syncVar in SyncVars.Keys)
        {
            WriteSyncVar(syncVar, writer);
        }
    }
}
