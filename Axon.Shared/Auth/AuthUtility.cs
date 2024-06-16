using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Shared.Auth;

public static class AuthUtility
{
    public static string GetUserIdFromPub(RSAParameters parameters)
    {
        var sha = SHA256.Create();
        var hash = sha.ComputeHash(parameters.Modulus);
        return Convert.ToBase64String(hash)+"@axon";
    }
}
