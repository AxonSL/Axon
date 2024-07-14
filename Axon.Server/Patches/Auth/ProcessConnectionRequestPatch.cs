using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Axon.Shared.Auth;
using Discord;
using Exiled.API.Features;
using GameCore;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PluginAPI.Core;
using PluginAPI.Events;

namespace Axon.Server.Patches.Mirror;

[HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport), nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
public static class ProcessConnectionRequestPatch
{
    [HarmonyPrefix]
    public static bool OnConnectionRequest(ConnectionRequest request)
    {
        try
        {
            var ip = request.RemoteEndPoint.Address.ToString();

            #region RequestType
            if (!request.Data.TryGetByte(out var requestType))
            {
                RequestWriter.Reset();
                RequestWriter.Put(2);
                request.RejectForce(RequestWriter);
                return false;
            }
            //Vanilla would use 0 and central server 1
            if(requestType < 3)
            {
                RequestWriter.Reset();
                RequestWriter.Put((byte)RejectionReason.VersionMismatch);
                request.RejectForce(RequestWriter);
                return false;
            }
            #endregion

            #region GameVersion
            byte cBackwardRevision = 0;
            if (!request.Data.TryGetByte(out var cMajor)
                || !request.Data.TryGetByte(out var cMinor)
                || !request.Data.TryGetByte(out var cRevision)
                || !request.Data.TryGetBool(out var flag)
                || (flag && !request.Data.TryGetByte(out cBackwardRevision)))
            {
                RequestWriter.Reset();
                RequestWriter.Put(3);
                request.RejectForce(RequestWriter);
                return false;
            }

            if (!GameCore.Version.CompatibilityCheck(GameCore.Version.Major, GameCore.Version.Minor, GameCore.Version.Revision,
                cMajor, cMinor, cRevision, flag, cBackwardRevision))
            {
                RequestWriter.Reset();
                RequestWriter.Put(3);
                request.RejectForce(RequestWriter);
                return false;
            }
            #endregion

            #region DelayConnections
            if (CustomLiteNetLib4MirrorTransport.DelayConnections)
            {
                CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
                RequestWriter.Reset();
                RequestWriter.Put(17);
                RequestWriter.Put(CustomLiteNetLib4MirrorTransport.DelayTime);

                if (CustomLiteNetLib4MirrorTransport.DelayVolume < 255)
                    CustomLiteNetLib4MirrorTransport.DelayVolume += 1;

                if (CustomLiteNetLib4MirrorTransport.DelayVolume < CustomLiteNetLib4MirrorTransport.DelayVolumeThreshold)
                {
                    if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
                        ServerConsole.AddLog(string.Format("Delayed connection incoming from endpoint {0} by {1} seconds.", request.RemoteEndPoint, CustomLiteNetLib4MirrorTransport.DelayTime), ConsoleColor.Gray);
                    
                    request.Reject(RequestWriter);
                }
                else
                {
                    if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
                        ServerConsole.AddLog(string.Format("Force delayed connection incoming from endpoint {0} by {1} seconds.", request.RemoteEndPoint, CustomLiteNetLib4MirrorTransport.DelayTime), ConsoleColor.Gray);

                    request.RejectForce(RequestWriter);
                }

                return false;
            }
            #endregion

            //TODO: Reimplement Proxies

            #region CheckTimeStamp
            if (!request.Data.TryGetLong(out var timeStamp))
            {
                RequestWriter.Reset();
                RequestWriter.Put(2);
                request.RejectForce(RequestWriter);
                return false;
            }
            if(TimeBehaviour.CurrentUnixTimestamp > timeStamp)
            {
                RequestRejected();
                if(LogRejection)
                {
                    ServerConsole.AddLog("Player from endpoint " + ip + "sent expired preauthentication request.");
                    ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.", ConsoleColor.Gray);
                }
                RequestWriter.Reset();
                RequestWriter.Put(11);
                request.RejectForce(RequestWriter);
                CustomLiteNetLib4MirrorTransport.ResetIdleMode();
                return false;
            }
            #endregion

            #region AxonAuth
            if(!request.Data.TryGetBytesWithLength(out var authData))
            {
                RequestWriter.Reset();
                RequestWriter.Put(4);
                request.RejectForce(RequestWriter);
                return false;
            }

            var userId = string.Empty;
            var name = string.Empty;
            var sharedKey = new byte[0];

            switch (requestType)
            {
                case 3:
                    var serverEnv = ServerAuthSession.Generate();
                    var clientHandshake = ClientHandshake.Decode(authData);
                    userId = clientHandshake.GetUserId();
                    var serverIden = AuthHandler.AddConnection(serverEnv, clientHandshake);

                    RequestWriter.Reset();
                    RequestWriter.Put((byte)100);
                    RequestWriter.PutBytesWithLength(serverEnv.CreateHandshake().Encode());
                    RequestWriter.Put(serverIden);
                    request.RejectForce(RequestWriter);

                    Exiled.API.Features.Log.Debug("Got Handshake from user " + userId + " from ip " + ip);
                    CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
                    return false;

                case 4:
                    var attempt = ClientLoginAttempt.Decode(authData);

                    if (!request.Data.TryGetString(out var serverIdentifier))
                    {
                        RequestWriter.Reset();
                        RequestWriter.Put(4);
                        request.RejectForce(RequestWriter);
                        return false;
                    }

                    if(!AuthHandler._connections.TryGetValue(serverIdentifier,out var connection))
                    {
                        RequestWriter.Reset();
                        RequestWriter.Put(4);
                        request.RejectForce(RequestWriter);
                        return false;
                    }

                    if (!connection.ServerEnv.Validate(connection.Handshake, attempt))
                    {
                        RequestWriter.Reset();
                        RequestWriter.Put((byte)RejectionReason.InvalidToken);
                        request.RejectForce(RequestWriter);
                    }
                    AuthHandler._connections.Remove(serverIdentifier);

                    if (!request.Data.TryGetString(out name))
                    {
                        RequestWriter.Reset();
                        RequestWriter.Put(4);
                        request.RejectForce(RequestWriter);
                        return false;
                    }

                    userId = connection.Handshake.GetUserId();
                    sharedKey = connection.ServerEnv.GetSharedSecret(connection.Handshake);

                    Exiled.API.Features.Log.Debug($"Verified {name}({userId}) - " + ip);
                    break;

                default:
                    RequestWriter.Reset();
                    RequestWriter.Put(19);
                    request.RejectForce(RequestWriter);
                    return false;
            }
            #endregion

            #region Banned
            var banInfo = BanHandler.QueryBan(userId, ip);
            if(banInfo.Key != null || banInfo.Value != null)
            {
                var ban = banInfo.Key ?? banInfo.Value;
                RequestRejected();

                if(LogRejection)
                {
                    var msg = $"{(banInfo.Key == null ? "Player" : "Banned Player")} " +
                        $"{userId} tried to connect from {(banInfo.Value == null ? "" : "banned")} endpoint {ip}";
                    ServerConsole.AddLog(msg, ConsoleColor.Gray);
                    ServerLogs.AddLog(ServerLogs.Modules.Networking, msg, ServerLogs.ServerLogType.ConnectionUpdate);
                }

                RequestWriter.Reset();
                RequestWriter.Put(6);
                RequestWriter.Put(ban.Expires);
                RequestWriter.Put(ban.Reason);
                request.Reject(RequestWriter);
                CustomLiteNetLib4MirrorTransport.ResetIdleMode();
                return false;
            }

            #endregion

            #region RateLimit
            //Maybe make this earlier and let it use the ip instead of userid
            if (CustomLiteNetLib4MirrorTransport.UserRateLimiting)
            {
                if (CustomLiteNetLib4MirrorTransport.UserRateLimit.Contains(userId))
                {
                    RequestRejected();
                    if (LogRejection)
                    {
                        ServerConsole.AddLog($"Incoming connection from {userId} ({ip}) rejected due to exceeding the rate limit", ConsoleColor.Gray);
                    }

                    RequestWriter.Reset();
                    RequestWriter.Put(12);
                    request.RejectForce(RequestWriter);
                    CustomLiteNetLib4MirrorTransport.ResetIdleMode();
                    return false;
                }
                CustomLiteNetLib4MirrorTransport.UserRateLimit.Add(userId);
            }
            #endregion

            #region Whitelist
            if (ServerConsole.WhiteListEnabled && !WhiteList.IsWhitelisted(userId))
            {
                if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
                {
                    ServerConsole.AddLog($"Player {userId} tried joined from endpoint {ip}, but is not whitelisted", ConsoleColor.Gray);
                }

                RequestWriter.Reset();
                RequestWriter.Put(7);
                request.Reject(RequestWriter);
                CustomLiteNetLib4MirrorTransport.ResetIdleMode();
                return false;
            }
            #endregion

            #region Geoblocking
            var region = "";
            //TODO: Geobkocking
            //NW just sends the country along in the Request and that means it is really easy to manipulate
            if (false)
            {
                RequestRejected();
                if (LogRejection)
                {
                    ServerConsole.AddLog($"Player {userId} ({ip}) tried to join from a blocked country {("INSERT COUNTRY HERE")}", ConsoleColor.Gray);
                }

                RequestWriter.Reset();
                RequestWriter.Put(9);
                request.Reject(RequestWriter);
                CustomLiteNetLib4MirrorTransport.ResetIdleMode();
            }
            #endregion

            #region Slots
            if (!HasSlot(userId))
            {
                RequestWriter.Reset();
                RequestWriter.Put(1);
                request.Reject(RequestWriter);
                CustomLiteNetLib4MirrorTransport.ResetIdleMode();
            }
            #endregion

            #region NWAPI
            var ev = EventManager.ExecuteEvent<PreauthCancellationData>(new PlayerPreauthEvent(userId, ip, timeStamp, CentralAuthPreauthFlags.None, region, new byte[0], request, request.Data.Position));
            if (!CustomLiteNetLib4MirrorTransport.ProcessCancellationData(request, ev))
            {
                //ProcessCancellation Does already the rejecting
                return false;
            }
            #endregion

            #region Finalize Accepting
            if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
            {
                CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(userId);
            }
            else
            {
                CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint] = new PreauthItem(userId);
            }

            var netPeer = request.Accept();

            //This is for proxy support later
            //if(realIp != null)
            if (false)
            {
                CustomLiteNetLib4MirrorTransport.RealIpAddresses[netPeer.Id] = ip;//realIP
            }

            if(Statistics.PeakPlayers.Total < LiteNetLib4MirrorCore.Host.ConnectedPeersCount)
            {
                Statistics.PeakPlayers = new Statistics.Peak(LiteNetLib4MirrorCore.Host.ConnectedPeersCount, DateTime.Now);
            }

            var msg2 = $"Player {userId} preauthenticated from endpoint {ip}.";
            ServerConsole.AddLog(msg2, ConsoleColor.Gray);
            ServerLogs.AddLog(ServerLogs.Modules.Networking, msg2, ServerLogs.ServerLogType.ConnectionUpdate, false);

            CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
            #endregion

            AuthHandler._storedPlayers[request.RemoteEndPoint] = new Server.Auth.PlayerStorage
            {
                NickName = name,
                UserId = userId,
                SharedKey = sharedKey,
            };
            return false;
        }
        catch (Exception ex)
        {
            Exiled.API.Features.Log.Error(ex);
        }
        return true;
    }

    private static NetDataWriter RequestWriter
    {
        get => CustomLiteNetLib4MirrorTransport.RequestWriter;
    }

    private static bool LogRejection
    {
        get => !CustomLiteNetLib4MirrorTransport.SuppressRejections && CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs;
    }

    private static void RequestRejected()
    {
        CustomLiteNetLib4MirrorTransport.Rejected += 1u;
        if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
            CustomLiteNetLib4MirrorTransport.SuppressRejections = true;
    }

    private static bool HasSlot(string userId)
    {
        //When the configured max slots are not full yet allow connections
        if (LiteNetLib4MirrorCore.Host.ConnectedPeersCount < CustomNetworkManager.slots)
            return true;

        //Reject the Connection when Mirror can't handle another Request
        if (LiteNetLib4MirrorCore.Host.ConnectedPeersCount == LiteNetLib4MirrorNetworkManager.singleton.maxConnections)
            return false;

        if(ConfigFile.ServerConfig.GetBool("use_reserved_slots",true) && ReservedSlot.HasReservedSlot(userId,out var bypassMaxReservedSlots))
        {
            if (bypassMaxReservedSlots)
                return true;

            if (LiteNetLib4MirrorCore.Host.ConnectedPeersCount < CustomNetworkManager.slots + CustomNetworkManager.reservedSlots)
                return true;
        }

        return false;
    }
}
