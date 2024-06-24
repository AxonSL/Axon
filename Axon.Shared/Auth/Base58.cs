using System;
using System.ComponentModel;
using System.Numerics;

namespace Axon.Shared.Auth;

public static class Base58
{
    private const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private static readonly int[] Base58Indexes = new int[128];

    static Base58()
    {
        for (int i = 0; i < Base58Indexes.Length; i++)
        {
            Base58Indexes[i] = -1;
        }
        for (int i = 0; i < Alphabet.Length; i++)
        {
            Base58Indexes[Alphabet[i]] = i;
        }
    }

    public static string Encode(byte[] bytes)
    {
        BigInteger intData = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            intData = intData * 256 + bytes[i];
        }

        string result = "";
        while (intData > 0)
        {
            int remainder = (int)(intData % 58);
            intData /= 58;
            result = Alphabet[remainder] + result;
        }

        for (int i = 0; i < bytes.Length && bytes[i] == 0; i++)
        {
            result = '1' + result;
        }

        return result;
    }
}
