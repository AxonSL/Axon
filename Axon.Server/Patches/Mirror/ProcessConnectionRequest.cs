using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Axon.Shared.Auth;
using Exiled.API.Features;
using HarmonyLib;
using LiteNetLib;
using Mirror;

namespace Axon.Server.Patches.Mirror;

[HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
public static class ProcessConnectionRequest
{
    public static Dictionary<string, byte[]> Requests = new Dictionary<string, byte[]>();

    [HarmonyPrefix]
    public static bool OnConnectionRequest(ConnectionRequest request)
    {
        try
        {
            if (!request.Data.TryGetByte(out var requestType))
            {
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(4);
                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                return false;
            }

            byte cBackwardRevision = 0;
            if (!request.Data.TryGetByte(out var cMajor)
                || !request.Data.TryGetByte(out var cMinor)
                || !request.Data.TryGetByte(out var cRevision)
                || !request.Data.TryGetBool(out var flag)
                || (flag && !request.Data.TryGetByte(out cBackwardRevision)))
            {
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(3);
                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                return false;
            }

            if (!GameCore.Version.CompatibilityCheck(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision,
                cMajor, cMinor, cRevision, flag, cBackwardRevision))
            {
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(3);
                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                return false;
            }

            if (!request.Data.TryGetBytesWithLength(out var exponent)
                        || !request.Data.TryGetBytesWithLength(out var pubKeyData)
                        || !request.Data.TryGetString(out var userId)
                        || !request.Data.TryGetString(out var name))
            {
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(4);
                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                return false;
            }

            var key = new RSAParameters()
            {
                Exponent = exponent,
                Modulus = pubKeyData
            };
            if (!string.Equals(userId, AuthUtility.GetUserIdFromPub(key)))
            {
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.VerificationRejected);
                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                return false;
            }

            var service = new RsaService(key);
            switch (requestType)
            {
                case 3:
                    var randomData = RandomGenerator.GetBytes(20, true);
                    Requests[userId] = randomData;
                    var encrypted = service.Encript(randomData);

                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)100);
                    CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(encrypted);
                    request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);

                    Log.Info("User: " + name + " tried to authenticate using this userId: " + userId + ". Validating Key...");
                    return false;

                case 4:
                    if(!Requests.TryGetValue(userId, out var data) || !request.Data.TryGetBytesWithLength(out var sendedData))
                    {
                        Requests.Remove(userId);
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put(19);
                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                        return false;
                    }

                    if (!data.SequenceEqual(sendedData))
                    {
                        Requests.Remove(userId);
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                        CustomLiteNetLib4MirrorTransport.RequestWriter.Put(19);
                        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                        return false;
                    }

                    Requests.Remove(userId);
                    Log.Info("Verified Key from user: " + name + " with userid: " + userId);
                    //TODO: Set UserId and other stuff

                    request.Accept();
                    break;

                default:
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put(19);
                    request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                    return false;
            }
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
        return true;
    }
}
