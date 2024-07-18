using System;

namespace Axon.NetworkMessages;

public struct RemoveAssetComponentMessage : Mirror.NetworkMessage
{
    public uint objectId;
    public string componentName;
}
