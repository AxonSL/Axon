using System.Security.Cryptography;

namespace Axon.Shared.Auth;

public struct PlayerAuth
{
    public RSAParameters Key { get; set; }
    public string UserId { get; set; }
    public string Username { get; set; }
}
