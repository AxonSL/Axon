using Mirror;
using UnityEngine;

namespace Axon.NetworkMessages;

public struct SpawnAssetMessage : NetworkMessage
{
    public uint objectId;
    public string bundleName;
    public string assetName;
    public string gameObjectName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}
