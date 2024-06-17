using System;

namespace Axon.Shared.Auth;

public class ServerHandshake
{
    public byte[] sessionPublic; // X25519 [32]
    public byte[] challenge; // Random [20]

    public const int Size = 52;

    public byte[] Encode()
    {
        var buf = new byte[Size];
        Array.Copy(sessionPublic, 0, buf, 0, 32);
        Array.Copy(challenge, 0, buf, 32, 20);
        return buf;
    }

    public static ServerHandshake Decode(byte[] buf)
    {
        var handshake = new ServerHandshake();
        handshake.sessionPublic = new byte[32];
        handshake.challenge = new byte[20];
        Array.Copy(buf, 0, handshake.sessionPublic, 0, 32);
        Array.Copy(buf, 32, handshake.challenge, 0, 20);
        return handshake;
    }
}
