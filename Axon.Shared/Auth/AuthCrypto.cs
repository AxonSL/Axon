using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.X9;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace Axon.Shared.Auth;

public class AuthCrypto
{

    public static X9ECParameters SingingCurve = ECNamedCurveTable.GetByName("secp256r1");

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
        Console.WriteLine(valid);

        var shared = serverEnv.GetSharedSecret(clientHandshake);
        var message = Encoding.UTF8.GetBytes("Hallo Welt");
        var encoded = AesEncryptWithNonce(shared, message); // Use this for remote admin
        Console.WriteLine(Encoding.UTF8.GetString(AesDecryptWithNonce(shared, encoded)));
    }

    public static byte[] CreateIdentity()
    {
        var buf = new byte[97];
        var generator = new ECKeyPairGenerator();
        generator.Init(new ECKeyGenerationParameters(new ECDomainParameters(SingingCurve), new SecureRandom()));
        var pair = generator.GenerateKeyPair();
        var privateParams = pair.Private as ECPrivateKeyParameters;
        var publicParams = pair.Public as ECPublicKeyParameters;
        Array.Copy(privateParams.D.ToByteArrayUnsigned(), 0, buf, 0, 32);
        Array.Copy(publicParams.Q.GetEncoded(), 0, buf, 32, 65);
        return buf;
    }

    public static AsymmetricCipherKeyPair ReadIdentity(byte[] buf)
    {
        var privateKeyBytes = new byte[32];
        Array.Copy(buf, 0, privateKeyBytes, 0, 32);
        var privateKey = new ECPrivateKeyParameters(
            new BigInteger(1, privateKeyBytes),
            new ECDomainParameters(SingingCurve));

        var publicKey = new byte[65];
        Array.Copy(buf, 32, publicKey, 0, 65);
        var publicKeyParams = new ECPublicKeyParameters(
            SingingCurve.Curve.DecodePoint(publicKey),
            new ECDomainParameters(SingingCurve));

        return new AsymmetricCipherKeyPair(publicKeyParams, privateKey);
    }

    public static byte[] WritePublic(AsymmetricCipherKeyPair ecdsa)
    {
        var publicKeyParameters = ecdsa.Public as ECPublicKeyParameters;
        return publicKeyParameters!.Q.GetEncoded();
    }

    public static ECPublicKeyParameters ReadPublic(byte[] buf)
    {
        return new ECPublicKeyParameters(SingingCurve.Curve.DecodePoint(buf), new ECDomainParameters(SingingCurve));
    }

    public static byte[] SignData(byte[] data, AsymmetricCipherKeyPair pair)
    {
        var s = SignerUtilities.GetSigner("SHA-256withECDSA");
        s.Init(true, pair.Private);
        s.BlockUpdate(data, 0, data.Length);
        return s.GenerateSignature();
    }

    public static bool VerifyData(byte[] data, byte[] signature, ECPublicKeyParameters publicKey)
    {
        var s = SignerUtilities.GetSigner("SHA-256withECDSA");
        s.Init(false, publicKey);
        s.BlockUpdate(data, 0, data.Length);
        return s.VerifySignature(signature);
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

    public static byte[] AesEncryptWithNonce(byte[] key, byte[] data)
    {
        var iv = new byte[12];
        new SecureRandom().NextBytes(iv);
        var bytes = AesEncrypt(key, iv, data);
        var buf = new byte[iv.Length + bytes.Length];
        Array.Copy(iv, 0, buf, 0, iv.Length);
        Array.Copy(bytes, 0, buf, iv.Length, bytes.Length);
        return buf;
    }

    public static byte[] AesDecryptWithNonce(byte[] key, byte[] data)
    {
        var iv = new byte[12];
        var bytes = new byte[data.Length - iv.Length];
        Array.Copy(data, 0, iv, 0, iv.Length);
        Array.Copy(data, iv.Length, bytes, 0, bytes.Length);
        return AesDecrypt(key, iv, bytes);
    }
}