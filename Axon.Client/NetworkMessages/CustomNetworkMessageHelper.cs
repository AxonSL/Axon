using Il2CppMirror;

namespace Axon.Client.NetworkMessages;

public abstract class CustomNetworkMessageHelper<T> : ICustomNetworkMessageHelper
    where T : Il2CppSystem.Object, new()
{
    public Type MessageType => typeof(T);

    public void WriteUnsafe(NetworkWriter writer, Il2CppSystem.Object message)
        => Write(writer, message.Cast<T>());

    public abstract void Write(NetworkWriter writer, T message);

    public abstract T Read(NetworkReader reader);

    public abstract void OnMessage(T message);

    public void OnMessageReceived(NetworkConnection conn, NetworkReader reader, int channelId)
    {
        var message = Read(reader);
        OnMessage(message);
    }
}

public interface ICustomNetworkMessageHelper
{
    public Type MessageType { get; }

    public void WriteUnsafe(NetworkWriter writer, Il2CppSystem.Object message);

    public void OnMessageReceived(NetworkConnection conn, NetworkReader reader, int channelId);
}
