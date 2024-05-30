using Mirror;
using UnityEngine;

namespace Axon.NetworkMessages;

public struct UpdateAssetMessage : NetworkMessage
{
    public uint objectId;
    public ulong syncDirtyBits;
    public Vector3 position;    //syncBit: 1
    public Quaternion rotation; //syncBit: 2
    public Vector3 scale;       //syncBit: 4
}
