using Mirror;

namespace Axon.NetworkMessages;

public struct PostJoinAuthMessage : NetworkMessage
{
    public bool ServerRequestAuth { get; set; }
    public string PublicKey { get; set; }
    public string NickName { get; set; }
}
