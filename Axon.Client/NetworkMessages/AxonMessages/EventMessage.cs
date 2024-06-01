using Axon.Client.Event;
using Axon.Client.Event.Handlers;
using Axon.Shared.Meta;
using Axon.Client.NetworkMessages;
using Il2CppInterop.Runtime.Injection;
using Il2CppMirror;

namespace Axon.NetworkMessages;

[Automatic]
[CustomNetworkMessage(MessageHelper = typeof(EventMessageHelper))]
public class EventMessage : Il2CppSystem.Object //NetworkMessage
{
    public EventMessage(IntPtr ptr) : base(ptr) { }

    public EventMessage() : base(ClassInjector.DerivedConstructorPointer<EventMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public EventMessageId eventId;
}

public class EventMessageHelper : CustomNetworkMessageHelper<EventMessage>
{
    public override void OnMessage(EventMessage message)
    {
        switch (message.eventId)
        {
            case EventMessageId.RoundStart:
                RoundHandler.RoundStart.Raise(new());
                break;

            case EventMessageId.RoundEnd:
                RoundHandler.RoundEnd.Raise(new());
                break;

            case EventMessageId.RoundRestart:
                RoundHandler.RoundRestart.Raise(new());
                break;
        }
    }

    public override EventMessage Read(NetworkReader reader)
    {
        return new EventMessage()
        {
            eventId = (EventMessageId)reader.ReadInt()
        };
    }

    public override void Write(NetworkWriter writer, EventMessage message)
    {
        writer.WriteInt((int)message.eventId);
    }
}
