using System;
using System.Security.Cryptography;

namespace Axon.Shared.Auth;

public class ClientHandshake
{
    public byte[] identityPublic; // ECDSA [64]
    public byte[] sessionPublic; // X25519 [32]
    public byte[] nonce; // Random [12]

    public const int Size = 112;

    public byte[] Encode()
    {
        var buf = new byte[Size];
        Array.Copy(identityPublic, 0, buf, 0, 64);
        Array.Copy(sessionPublic, 0, buf, 64, 32);
        Array.Copy(nonce, 0, buf, 96, 12);
        return buf;
    }

    public static ClientHandshake Decode(byte[] buf)
    {
        var handshake = new ClientHandshake();
        handshake.identityPublic = new byte[64];
        handshake.sessionPublic = new byte[32];
        handshake.nonce = new byte[12];
        Array.Copy(buf, 0, handshake.identityPublic, 0, 64);
        Array.Copy(buf, 64, handshake.sessionPublic, 0, 32);
        Array.Copy(buf, 96, handshake.nonce, 0, 12);
        return handshake;
    }

    public string GetUserId()
    {
        var sha = SHA256.Create();
        var hash = sha.ComputeHash(identityPublic);
        return Convert.ToBase64String(hash) + "@axon";
    }
}
