using Axon.Client.AssetBundle;
using Axon.Client.NetworkMessages;
using Il2CppInterop.Runtime.Injection;
using Il2CppMirror;
using MelonLoader;
using Axon.Shared.Meta;
using Il2Cpp;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Axon.NetworkMessages;

[Automatic]
[CustomNetworkMessage(MessageHelper = typeof(RemoveAssetComponentMessageHelper))]
public class RemoveAssetComponentMessage : Il2CppSystem.Object //NetworkMessage
{
    public RemoveAssetComponentMessage(IntPtr ptr) : base(ptr) { }
    
    public RemoveAssetComponentMessage() : base(ClassInjector.DerivedConstructorPointer<TestMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public uint objectId;
    public string componentName;
}

public class RemoveAssetComponentMessageHelper : CustomNetworkMessageHelper<RemoveAssetComponentMessage>
{
    public override RemoveAssetComponentMessage Read(NetworkReader reader)
    {
        return new RemoveAssetComponentMessage(){
            objectId = reader.ReadUInt(), componentName = reader.ReadString(),
        };

    }
    public override void OnMessage(RemoveAssetComponentMessage message)
    {
        var obj = AssetBundleSpawner.SpawnedAssets.FirstOrDefault(x => x.Id == message.objectId);
        if (obj == null)
        {
            MelonLogger.Warning("Server tried to destroy the component of a non existent asset");
            return;
        }
        var components = obj.GameObject.GetComponents<MonoBehaviour>().Where(x => x.GetType().FullName == message.componentName);
        foreach (var behaviour in components)
        {
            Object.Destroy(behaviour);
        }

    }

    public override void Write(NetworkWriter writer, RemoveAssetComponentMessage message)
    {
        writer.WriteUInt(message.objectId);
        writer.WriteString(message.componentName);

    }
}

