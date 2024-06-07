using Axon.Client.AssetBundle;
using Axon.Client.NetworkMessages;
using Axon.Shared.Meta;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMirror;
using MelonLoader;

namespace Axon.NetworkMessages;

[Automatic]
[CustomNetworkMessage(MessageHelper = typeof(SyncVarMessageHelper))]
public class SyncVarMessage : Il2CppSystem.Object //NetworkMessage
{
    public SyncVarMessage(IntPtr ptr) : base(ptr) { }

    public SyncVarMessage() : base(ClassInjector.DerivedConstructorPointer<SyncVarMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public uint objectId;
    public string scriptName;
    public ulong syncDirtyBits;
    public Il2CppSystem.ArraySegment<byte> data;
}

public class SyncVarMessageHelper : CustomNetworkMessageHelper<SyncVarMessage>
{
    public override void OnMessage(SyncVarMessage message)
    {
        AssetBundleSpawner.OnSyncVarMessage(message);
    }

    public override SyncVarMessage Read(NetworkReader reader)
    {
        return new()
        {
            objectId = reader.ReadUInt(),
            scriptName = reader.ReadString(),
            syncDirtyBits = reader.ReadULong(),
            data = reader.ReadArraySegmentAndSize(),
        };
    }

    public override void Write(NetworkWriter writer, SyncVarMessage message)
    {
        writer.WriteUInt(message.objectId);
        writer.WriteString(message.scriptName);
        writer.WriteULong(message.syncDirtyBits);
        writer.WriteArraySegmentAndSize(message.data);
    }
}