using Mirror;
using System;

namespace Axon.NetworkMessages;

public struct SyncVarMessage : NetworkMessage
{
    public uint objectId;
    public string scriptName;
    public ulong syncDirtyBits;
    public ArraySegment<byte> data;

    [NonSerialized]
    public NetworkConnection connection; //This is Server only to figure out who requested the change
}
