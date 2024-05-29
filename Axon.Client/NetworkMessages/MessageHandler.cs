using Il2CppInterop.Runtime;
using Il2CppMirror;
using MelonLoader;
using Axon.NetworkMessages;
using System.Collections.ObjectModel;
using Axon.Client.Meta;
using Il2CppInterop.Runtime.Injection;

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
}
