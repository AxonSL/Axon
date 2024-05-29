using Il2CppInterop.Runtime.Injection;

namespace Axon.NetworkMessages;

public class TestMessage : Il2CppSystem.Object //NetworkMessage
{
    public TestMessage(IntPtr ptr) : base(ptr) { }

    
    public TestMessage() : base(ClassInjector.DerivedConstructorPointer<TestMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
    

    public string message;
}
