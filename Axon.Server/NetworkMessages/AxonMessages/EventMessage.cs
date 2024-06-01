using Axon.Server.Event;
using Mirror;

namespace Axon.NetworkMessages;

public struct EventMessage : NetworkMessage
{
    public EventMessageId eventId;
}

