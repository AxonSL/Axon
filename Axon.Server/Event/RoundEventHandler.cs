using Axon.NetworkMessages;
using Exiled.Events.EventArgs.Server;

namespace Axon.Server.Event;

public static class RoundEventHandler
{
    public static void OnRoundStart() => SendEventToClients(EventMessageId.RoundStart);

    public static void OnRoundEnd(RoundEndedEventArgs ev)
    {
        SendEventToClients(EventMessageId.RoundEnd);
    }

    public static void OnRoundRestart() => SendEventToClients(EventMessageId.RoundRestart);

    private static void SendEventToClients(EventMessageId eventMessageId)
    {
        var msg = new EventMessage { eventId = eventMessageId };
        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost) continue;
            hub.connectionToClient.Send(msg);
        }
    }
}
