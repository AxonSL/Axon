using System;
using System.Security.Cryptography;

namespace Axon.Shared.Auth;

public class ClientHandshake
{
    public byte[] identityPublic; // ECDSA [65]
    public byte[] sessionPublic; // X25519 [32]
    public byte[] nonce; // Random [12]

    public const int Size = 113;

    public byte[] Encode()
    {
        var buf = new byte[Size];
        Array.Copy(identityPublic, 0, buf, 0, 65);
        Array.Copy(sessionPublic, 0, buf, 65, 32);
        Array.Copy(nonce, 0, buf, 97, 12);
        return buf;
    }

    public static ClientHandshake Decode(byte[] buf)
    {
        var handshake = new ClientHandshake();
        var identityPublic = new byte[65];
        var sessionPublic = new byte[32];
        var nonce = new byte[12];
        Array.Copy(buf, 0, identityPublic, 0, 65);
        Array.Copy(buf, 65, sessionPublic, 0, 32);
        Array.Copy(buf, 97, nonce, 0, 12);
        handshake.identityPublic = identityPublic;
        handshake.sessionPublic = sessionPublic;
        handshake.nonce = nonce;
        return handshake;
    }

    public string GetUserId()
    {
        var sha = SHA256.Create();
        var hash = sha.ComputeHash(identityPublic);
        return Base58.Encode(hash)+"@axon";
    }
}
