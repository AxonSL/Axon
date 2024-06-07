using Il2CppInterop.Runtime;
using Il2CppMirror;
using MelonLoader;
using Axon.NetworkMessages;
using System.Collections.ObjectModel;
using Axon.Shared.Meta;
using Il2CppInterop.Runtime.Injection;
using System.Threading.Channels;
using Il2Cpp;

namespace Axon.Client.NetworkMessages;

public static class MessageHandler
{
    public static ReadOnlyCollection<ICustomNetworkMessageHelper> Helpers { get; private set; } = new(new List<ICustomNetworkMessageHelper>());

    public static void Init()
    {
        MetaAnalyzer.OnMeta.Subscribe(OnMeta);
    }

    internal static void RegisterMessages()
    {
        foreach (var helper in Helpers)
        {
            var id = (ushort)helper.MessageType.FullName.GetStableHashCode();
            MelonLogger.Msg("Registering Networkmessage: " + helper.MessageType.Name + " with ID: " + id);
            Il2CppMirror.NetworkMessages.Lookup[id] = Il2CppType.Of<TestMessage>();
            NetworkClient.handlers[id] = new Action<NetworkConnection, NetworkReader, int>(helper.OnMessageReceived);
        }
    }

    private static void OnMeta(MetaEvent ev)
    {
        try
        {
            var attribute = ev.GetAttribute<CustomNetworkMessageAttribute>();
            if (attribute == null) return;
            if (!ev.Is<Il2CppSystem.Object>()) return;

            var helper = (ICustomNetworkMessageHelper)Activator.CreateInstance(attribute.MessageHelper);
            var list = new List<ICustomNetworkMessageHelper>(Helpers)
            {
                helper
            };
            Helpers = new(list);

            
            ClassInjector.RegisterTypeInIl2Cpp(ev.Type, new RegisterTypeOptions()
            {
                Interfaces = new Type[] { typeof(NetworkMessage) }
            });
        }
        catch(Exception e)
        {
            MelonLogger.Error("NetworkMessages.MessageHandler.OnMeta failed: " + e);
        }
    }

    public static void SendCustomNetworkMessage<T>(this T message, int channelId = 0) where T : Il2CppSystem.Object,new()
    {
        try
        {
            var writer = new NetworkWriter();
            writer.WriteUShort((ushort)typeof(T).FullName.GetStableHashCode());
            foreach (var helper in Helpers)
            {
                if (helper.MessageType != typeof(T)) continue;
                helper.WriteUnsafe(writer, message);
                break;
            }

            //NetworkDiagnostics.OnSend(message, channelId, writer.Position, 1);
            var data = writer.ToArraySegment();
            var batcher = NetworkClient.connection.GetBatchForChannelId(channelId);
            var timeStamp = NetworkTime.localTime;
            var connection = NetworkClient.connection;

            var num = Compression.VarUIntSize((ulong)((long)data.Count)) + data.Count;
            if(batcher.batch != null && batcher.batch.Position + num > batcher.threshold)
            {
                batcher.batches.Enqueue(batcher.batch);
                batcher.batch = null;
            }
            if(batcher.batch == null)
            {
                batcher.batch = NetworkWriterPool.Get();
                batcher.batch.WriteDouble(timeStamp);
            }
            Compression.CompressVarUInt(batcher.batch, (ulong)((long)data.Count));
            //TODO: Find a better way
            //For Some Reason It don't want to write the array segment so I just write the message again
            batcher.batch.WriteUShort((ushort)typeof(T).FullName.GetStableHashCode());
            foreach (var helper in Helpers)
            {
                if (helper.MessageType != typeof(T)) continue;
                helper.WriteUnsafe(batcher.batch, message);
                break;
            }
            //NetworkWriterPool.Return(writer);
        }
        catch (Exception e)
        {
            MelonLogger.Error("MessageHandler.SendCustomNetworkMessage Error: " + e);
        }
    }
}
