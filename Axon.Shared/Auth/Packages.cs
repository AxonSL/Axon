using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Axon.Shared.Auth;

public class AuthCrypto
{

    public static void Main()
    {
        var clientEnv = ClientAuthSession.Generate(CreateIdentity()); // On Client
        var clientHandshake = clientEnv.CreateHandshake(); // On Client
        // clientHandshake -> Server
        var serverEnv = ServerAuthSession.Generate(); // On Server
        var serverHandshake = serverEnv.CreateHandshake(); // On Server
        // serverHandshake -> Client
        var attempt = clientEnv.CreateLoginAttempt(serverHandshake); // On Client
        // attempt -> Server
        var valid = serverEnv.Validate(clientHandshake, attempt); // On Server
        Console.WriteLine(attempt.signature.Length);
        Console.WriteLine(valid);
        Console.WriteLine(serverEnv.GetSharedSecret(clientHandshake).Length);
    }

    public static byte[] CreateIdentity()
    {
        var buf = new byte[96];
        var ecdsa = ECDsa.Create();
        ecdsa.GenerateKey(ECCurve.NamedCurves.nistP256);
        var parameters = ecdsa.ExportParameters(true);
        Array.Copy(parameters.D, 0, buf, 0, 32);
        Array.Copy(parameters.Q.X, 0, buf, 32, 32);
        Array.Copy(parameters.Q.Y, 0, buf, 64, 32);
        return buf;
    }
    
    public static ECDsa ReadIdentity(byte[] buf)
    {
        var ecdsa = ECDsa.Create();
        var parameters = new ECParameters();
        var d = new byte[32];
        var x = new byte[32];
        var y = new byte[32];
        Array.Copy(buf, 0, d, 0, 32);
        Array.Copy(buf, 32, x, 0, 32);
        Array.Copy(buf, 64, y, 0, 32);
        parameters.Curve = ECCurve.NamedCurves.nistP256;
        parameters.D = d;
        parameters.Q.X = x;
        parameters.Q.Y = y;
        ecdsa.ImportParameters(parameters);
        return ecdsa;
    }

    public static byte[] WritePublic(ECDsa ecdsa)
    {
        var parameters = ecdsa.ExportParameters(false);
        var buf = new byte[64];
        Array.Copy(parameters.Q.X, 0, buf, 0, 32);
        Array.Copy(parameters.Q.Y, 0, buf, 32, 32);
        return buf;
    }
    
    public static ECDsa ReadPublic(byte[] buf)
    {
        var ecdsa = ECDsa.Create();
        var parameters = new ECParameters();
        var x = new byte[32];
        var y = new byte[32];
        Array.Copy(buf, 0, x, 0, 32);
        Array.Copy(buf, 32, y, 0, 32);
        parameters.Curve = ECCurve.NamedCurves.nistP256;
        parameters.Q.X = x;
        parameters.Q.Y = y;
        ecdsa.ImportParameters(parameters);
        return ecdsa;
    }
    
    public static AsymmetricCipherKeyPair CreateX25519KeyPair()
    {
        var g = new X25519KeyPairGenerator();
        g.Init(new X25519KeyGenerationParameters(new SecureRandom()));
        return g.GenerateKeyPair();
    }
    
    public static byte[] GetX25519Public(AsymmetricCipherKeyPair pair)
    {
        return ((X25519PublicKeyParameters)pair.Public).GetEncoded();
    }
    
    public static byte[] ComputeSharedSecret(AsymmetricCipherKeyPair pair, byte[] publicKey)
    {
        var s = new X25519Agreement();
        s.Init(pair.Private);
        var x25519PublicKeyParameters = new X25519PublicKeyParameters(publicKey);
        var secret = new byte[s.AgreementSize];
        s.CalculateAgreement(x25519PublicKeyParameters, secret, 0);
        return secret;
    }
    
    public static byte[] AesEncrypt(byte[] key, byte[] iv, byte[] data)
    {
        var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
        cipher.Init(true, new AeadParameters(new KeyParameter(key), 128, iv));
        return cipher.DoFinal(data);
    }
    
    public static byte[] AesDecrypt(byte[] key, byte[] iv, byte[] data)
    {
        var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
        cipher.Init(false, new AeadParameters(new KeyParameter(key), 128, iv));
        return cipher.DoFinal(data);
    }
}

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
        var signature = AuthCrypto.AesDecrypt(sharedSecret, new byte[12], attempt.signature);
        var identityPublic = AuthCrypto.ReadPublic(handshake.identityPublic);
        return identityPublic.VerifyData(buf, signature, HashAlgorithmName.SHA256);
    }
}

public class ClientHandshake
{
    public byte[] identityPublic; // ECDSA [64]
    public byte[] sessionPublic; // X25519 [32]
    public byte[] nonce; // Random [12]
    
    public const int Size = 112;
    
    public byte [] Encode()
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
}

public class ServerHandshake
{
    public byte[] sessionPublic; // X25519 [32]
    public byte[] challenge; // Random [20]
    
    public const int Size = 52;
    
    public byte [] Encode()
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

public class ClientLoginAttempt
{
    public byte[] signature; // Normally [80], not sure if this stays the same always after encryption
    
    public byte[] Encode()
    {
        return signature;
    }
    
    public static ClientLoginAttempt Decode(byte[] buf)
    {
        var attempt = new ClientLoginAttempt();
        attempt.signature = buf;
        return attempt;
    }
}