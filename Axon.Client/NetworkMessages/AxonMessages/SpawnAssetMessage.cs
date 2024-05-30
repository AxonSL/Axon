using Axon.Client.AssetBundle;
using Axon.Client.Meta;
using Axon.Client.NetworkMessages;
using Il2CppInterop.Runtime.Injection;
using Il2CppMirror;
using MelonLoader;
using UnityEngine;

namespace Axon.NetworkMessages;

[Automatic]
[CustomNetworkMessage(MessageHelper = typeof(SpawnAssetMessageHelper))]
public class SpawnAssetMessage : Il2CppSystem.Object //NetworkMessage
{
    public SpawnAssetMessage(IntPtr ptr) : base(ptr) { }

    public SpawnAssetMessage() : base(ClassInjector.DerivedConstructorPointer<SpawnAssetMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public uint objectId;
    public string bundleName;
    public string assetName;
    public string gameObjectName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

public class SpawnAssetMessageHelper : CustomNetworkMessageHelper<SpawnAssetMessage>
{
    public override void OnMessage(SpawnAssetMessage message)
    {
        AssetBundleSpawner.OnSpawnAssetMessage(message);
    }

    public override SpawnAssetMessage Read(NetworkReader reader)
    {
        var msg = new SpawnAssetMessage()
        {
            objectId = reader.ReadUInt(),
            bundleName = reader.ReadString(),
            assetName = reader.ReadString(),
            gameObjectName = reader.ReadString(),
            position = reader.ReadVector3(),
            rotation = reader.ReadQuaternion(),
            scale = reader.ReadVector3(),
        };
        return msg;
    }

    public override void Write(NetworkWriter writer, SpawnAssetMessage message)
    {
        writer.WriteUInt(message.objectId);
        writer.WriteString(message.bundleName);
        writer.WriteString(message.assetName);
        writer.WriteString(message.gameObjectName);
        writer.WriteVector3(message.position);
        writer.WriteQuaternion(message.rotation);
        writer.WriteVector3(message.scale);
    }
}