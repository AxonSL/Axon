using Axon.Client.AssetBundle;
using Axon.Client.Meta;
using Axon.Client.NetworkMessages;
using Il2CppInterop.Runtime.Injection;
using Il2CppMirror;
using MelonLoader;
using UnityEngine;

namespace Axon.NetworkMessages;

[Automatic]
[CustomNetworkMessage(MessageHelper = typeof(UpdateAssetMessageHelper))]
public class UpdateAssetMessage : Il2CppSystem.Object //NetworkMessage
{
    public UpdateAssetMessage(IntPtr ptr) : base(ptr) { }

    public UpdateAssetMessage() : base(ClassInjector.DerivedConstructorPointer<UpdateAssetMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public uint objectId;
    public ulong syncDirtyBits;
    public Vector3 position;    //syncBit: 1
    public Quaternion rotation; //syncBit: 2
    public Vector3 scale;       //syncBit: 4
}

public class UpdateAssetMessageHelper : CustomNetworkMessageHelper<UpdateAssetMessage>
{
    public override void OnMessage(UpdateAssetMessage message)
    {
        AssetBundleSpawner.UpdateAsset(message);
    }

    public override UpdateAssetMessage Read(NetworkReader reader)
    {
        var message = new UpdateAssetMessage()
        {
            objectId = reader.ReadUInt(),
            syncDirtyBits = reader.ReadULong(),
        };

        if ((message.syncDirtyBits & 1) == 1)
            message.position = reader.ReadVector3();

        if ((message.syncDirtyBits & 2) == 2)
            message.rotation = reader.ReadQuaternion();

        if ((message.syncDirtyBits & 4) == 4)
            message.scale = reader.ReadVector3();

        return message;
    }

    public override void Write(NetworkWriter writer, UpdateAssetMessage message)
    {
        writer.WriteUInt(message.objectId);
        writer.WriteULong(message.syncDirtyBits);

        if ((message.syncDirtyBits & 1) == 1)
            writer.WriteVector3(message.position);

        if ((message.syncDirtyBits & 2) == 2)
            writer.WriteQuaternion(message.rotation);

        if ((message.syncDirtyBits & 4) == 4)
            writer.WriteVector3(message.scale);
    }
}