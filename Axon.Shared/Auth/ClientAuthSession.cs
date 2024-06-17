using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;

namespace Axon.Shared.Auth;

public class ClientAuthSession
{
    ECDsa identityKeyPair;
    AsymmetricCipherKeyPair sessionKeyPair;
    byte[] nonce;

    public static ClientAuthSession Generate(byte[] identity)
    {
        var env = new ClientAuthSession();
        env.identityKeyPair = AuthCrypto.ReadIdentity(identity);
        env.sessionKeyPair = AuthCrypto.CreateX25519KeyPair();
        env.nonce = new byte[12];
        new SecureRandom().NextBytes(env.nonce);
        return env;
    }

    public ClientHandshake CreateHandshake()
    {
        return new ClientHandshake
        {
            identityPublic = AuthCrypto.WritePublic(identityKeyPair),
            sessionPublic = AuthCrypto.GetX25519Public(sessionKeyPair),
            nonce = nonce
        };
    }

    public byte[] GetSharedSecret(ServerHandshake handshake) => AuthCrypto.ComputeSharedSecret(sessionKeyPair, handshake.sessionPublic);

    public ClientLoginAttempt CreateLoginAttempt(ServerHandshake handshake)
    {
        var buf = new byte[32];
        Array.Copy(nonce, 0, buf, 0, 12);
        Array.Copy(handshake.challenge, 0, buf, 12, 20);
        var signature = identityKeyPair.SignData(buf, HashAlgorithmName.SHA256);

        var sharedSecret = AuthCrypto.ComputeSharedSecret(sessionKeyPair, handshake.sessionPublic);
        signature = AuthCrypto.AesEncrypt(sharedSecret, new byte[12], signature);
        return new ClientLoginAttempt { signature = signature };
    }
}
