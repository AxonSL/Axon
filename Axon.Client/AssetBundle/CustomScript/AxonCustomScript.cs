using Axon.Client.NetworkMessages;
using Axon.NetworkMessages;
using Axon.Shared.CustomScripts;
using Axon.Shared.Meta;
using Il2Cpp;
using Il2CppGameCore;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppMirror;
using MelonLoader;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

namespace Axon.Client.AssetBundle.CustomScript;

public abstract class AxonCustomScript : MonoBehaviour
{
    public AxonCustomScript(IntPtr ptr) : base(ptr) { }

    private ulong _syncDirtyBits = 0;

    public string UniqueName { get; internal set; }

    [HideFromIl2Cpp]
    public AxonAssetScript AxonAssetScript { get; internal set; }

    [HideFromIl2Cpp]
    public ReadOnlyDictionary<AxonSyncVarId, bool> SyncVars { get; private set; }

    public virtual void Awake()
    {
        var dic = new Dictionary<AxonSyncVarId, bool>();
        foreach (var field in GetType().GetFields())
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

        foreach (var syncVar in SyncVars.Keys)
        {
            if (((ulong)syncVar | _syncDirtyBits) == 0) continue;
            WriteSyncVar(syncVar, writer);
        }
        msg.data = writer.buffer;
        msg.SendCustomNetworkMessage();
        _syncDirtyBits = 0;
    }

    [HideFromIl2Cpp]
    protected virtual void WriteSyncVar(AxonSyncVarId syncVarId, NetworkWriter writer) { }

    [HideFromIl2Cpp]
    protected virtual void ReadSyncVar(AxonSyncVarId syncVarId, NetworkReader reader) { }

    [HideFromIl2Cpp]
    public virtual bool CheckIfAllowed(AxonSyncVarId syncVar) => true;

    [HideFromIl2Cpp]
    public void UpdateSyncVar(AxonSyncVarId syncVarId)
    {
        _syncDirtyBits += (ulong)syncVarId;
        //TODO: Implemts check if the client is allowed to do this
    }

    [HideFromIl2Cpp]
    internal void ReceiveMessage(SyncVarMessage message)
    {
        var dirty = message.syncDirtyBits;
        var reader = NetworkReaderPool.Get(message.data);
        foreach (var id in (AxonSyncVarId[])Enum.GetValues(typeof(AxonSyncVarId)))
        {
            if ((dirty & ((ulong)id)) == 0) continue;

            if (!SyncVars.TryGetValue(id, out _))
            {
                MelonLogger.Warning("Server tried to update a sync var of the component " + UniqueName + " that does not exist client side. Requested Sync Var Id:" + id.ToString());
                return;
            }

            ReadSyncVar(id, reader);
        }
        NetworkReaderPool.Return(reader);
    }

    [HideFromIl2Cpp]
    internal void ReadAllSyncVar(NetworkReader reader)
    {
        foreach(var sync in SyncVars.Keys)
        {
            ReadSyncVar(sync, reader);
        }
    }
}