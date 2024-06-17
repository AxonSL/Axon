namespace Axon.Server.Auth;

public class PlayerStorage
{
    public string NickName { get; set; }
    public string UserId { get; set; }
    public byte[] SharedKey { get; set; }
}
