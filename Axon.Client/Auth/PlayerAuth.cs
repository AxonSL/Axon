using System.Security.Cryptography;

namespace Axon.Client.Auth;

public struct PlayerAuth
{
    public string Identity { get; set; }
    public string Username { get; set; }

    public void SetIdentity(byte[] identity)
    {
        Identity = Convert.ToBase64String(identity);
    }
    public byte[] GetIdentity() => Convert.FromBase64String(Identity);
}
