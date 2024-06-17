using Axon.Client;
using Axon.Client.API.Features;
using Axon.Client.NetworkMessages;
using Axon.Shared.Meta;
using Il2Cpp;
using Il2CppCryptography;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppMirror;
using MelonLoader;

namespace Axon.NetworkMessages;

[Automatic]
[CustomNetworkMessage(MessageHelper = typeof(PostJoinAuthMessageMessageHelper))]
public class PostJoinAuthMessage : Il2CppSystem.Object //NetworkMessage
{
    public PostJoinAuthMessage(IntPtr ptr) : base(ptr) { }

    public PostJoinAuthMessage() : base(ClassInjector.DerivedConstructorPointer<PostJoinAuthMessage>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public bool ServerRequestAuth { get; set; }
    public string PublicKey { get; set; }
    public string NickName { get; set; }
}

public class PostJoinAuthMessageMessageHelper : CustomNetworkMessageHelper<PostJoinAuthMessage>
{
    public override void OnMessage(PostJoinAuthMessage message)
    {
        MelonLogger.Msg("Got server Post JoinAuth Message Key:\n" + message.PublicKey);
        if(message.ServerRequestAuth)
        {

        }
    }

    public override PostJoinAuthMessage Read(NetworkReader reader)
    {
        var msg = new PostJoinAuthMessage()
        {
            ServerRequestAuth = reader.ReadBool(),
            PublicKey = reader.ReadString(),
        };
        if (!msg.ServerRequestAuth)
        {
            msg.NickName = reader.ReadString();
        }
        return msg;
    }

    public override void Write(NetworkWriter writer, PostJoinAuthMessage message)
    {
        writer.WriteBool(message.ServerRequestAuth);
        writer.WriteString(message.PublicKey);
        if (!message.ServerRequestAuth)
        {
            writer.WriteString(message.NickName);
        }
    }
}