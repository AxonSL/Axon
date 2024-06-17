using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Axon.Shared.Auth;

public class ServerAuthSession
{
    AsymmetricCipherKeyPair sessionKeyPair;
    byte[] challenge;

    public static ServerAuthSession Generate()
    {
        var env = new ServerAuthSession();
        env.sessionKeyPair = AuthCrypto.CreateX25519KeyPair();
        env.challenge = new byte[20];
        new SecureRandom().NextBytes(env.challenge);
        return env;
    }

    public ServerHandshake CreateHandshake()
    {
        return new ServerHandshake
        {
            sessionPublic = AuthCrypto.GetX25519Public(sessionKeyPair),
            challenge = challenge
        };
    }

    public byte[] GetSharedSecret(ClientHandshake handshake) => AuthCrypto.ComputeSharedSecret(sessionKeyPair, handshake.sessionPublic);

    public bool Validate(ClientHandshake handshake, ClientLoginAttempt attempt)
    {
        var buf = new byte[32];
        Array.Copy(handshake.nonce, 0, buf, 0, 12);
        Array.Copy(challenge, 0, buf, 12, 20);
        var sharedSecret = AuthCrypto.ComputeSharedSecret(sessionKeyPair, handshake.sessionPublic);
        var signature = AuthCrypto.AesDecrypt(sharedSecret, handshake.nonce, attempt.signature);
        var identityPublic = AuthCrypto.ReadPublic(handshake.identityPublic);
        return AuthCrypto.VerifyData(buf, signature, identityPublic);
    }
}
