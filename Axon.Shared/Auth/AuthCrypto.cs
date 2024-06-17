using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

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
        Console.WriteLine(clientHandshake.GetUserId());
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