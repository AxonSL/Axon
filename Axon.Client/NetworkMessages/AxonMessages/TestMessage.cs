using Axon.Client.NetworkMessages;
using Il2CppInterop.Runtime.Injection;
using Il2CppMirror;
using MelonLoader;
using Axon.Shared.Meta;
using Il2Cpp;

namespace Axon.NetworkMessages;

[Automatic]
[CustomNetworkMessage(MessageHelper = typeof(TestMessageHelper))]
public class TestMessage : Il2CppSystem.Object //NetworkMessage
{
    public TestMessage(IntPtr ptr) : base(ptr) { }
    
    public TestMessage() : base(ClassInjector.DerivedConstructorPointer<TestMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public string message;
}

public class TestMessageHelper : CustomNetworkMessageHelper<TestMessage>
{
    public override void OnMessage(TestMessage message)
    {
        MelonLogger.Msg("Got Testmessage: " + message.message);
        var msg = new TestMessage()
        {
            message = "Return Message",
        };
        try
        {
            msg.SendCustomNetworkMessage();
        }
        catch (Exception ex)
        {
            MelonLogger.Error(ex);
        }
    }

    public override TestMessage Read(NetworkReader reader)
    {
        return new TestMessage()
        {
            message = reader.ReadString(),
        };
    }

    public override void Write(NetworkWriter writer, TestMessage message)
    {
        writer.WriteString(message.message);
    }
}
