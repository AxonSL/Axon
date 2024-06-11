using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axon.Server.ServerList;
using CentralAuth;
using Exiled.API.Features;
using GameCore;
using HarmonyLib;
using MEC;
using Mirror.LiteNetLib4Mirror;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Axon.Server.Patches.ServerList;

[HarmonyPatch(typeof(CustomNetworkManager),nameof(CustomNetworkManager._CreateLobby))]
public static class CustomNetworkManagerCreateLobby
{
    [HarmonyPrefix]
    public static bool OnCreateLobby(CustomNetworkManager __instance,out IEnumerator<float> __result)
    {
        __result = CreateLobby(__instance);
        return false;
    }

    private static IEnumerator<float> CreateLobby(CustomNetworkManager manager)
    {
        if (manager._queryEnabled)
        {
            manager._queryPort = (int)LiteNetLib4MirrorTransport.Singleton.port + ConfigFile.ServerConfig.GetInt("query_port_shift", 0);
            ServerConsole.AddLog("Query port will be enabled on port " + manager._queryPort.ToString() + " TCP.", ConsoleColor.Gray);
            CustomNetworkManager._queryserver = new QueryServer(manager._queryPort, ConfigFile.ServerConfig.GetBool("query_use_IPv6", true));
            CustomNetworkManager._queryserver.StartServer();
        }
        else
        {
            ServerConsole.AddLog("Query port disabled in config!", ConsoleColor.Gray);
        }

        if (ConfigFile.HosterPolicy.GetString("server_ip", "none") != "none")
        {
            ServerConsole.Ip = ConfigFile.HosterPolicy.GetString("server_ip", "none");
            ServerConsole.AddLog("Server IP address set to " + ServerConsole.Ip + " by your hosting provider.", ConsoleColor.Gray);
        }
        else
        {
            if (ConfigFile.ServerConfig.GetString("server_ip", "auto") != "auto")
            {
                ServerConsole.Ip = ConfigFile.ServerConfig.GetString("server_ip", "auto");
                ServerConsole.AddLog("Custom config detected. Your server IP address is " + ServerConsole.Ip, ConsoleColor.Gray);
            }
            else
            {
                ServerConsole.AddLog("Obtaining your external IP address...", ConsoleColor.Gray);
                for (; ; )
                {
                    UnityWebRequest www = UnityWebRequest.Get(CentralServer.StandardUrl + "ip.php");
                    yield return Timing.WaitUntilDone(www.SendWebRequest());
                    if (string.IsNullOrEmpty(www.error))
                    {
                        ServerConsole.Ip = www.downloadHandler.text;
                        ServerConsole.AddLog("Done, your server IP address is " + ServerConsole.Ip, ConsoleColor.Gray);
                        break;
                    }
                    ServerConsole.AddLog(string.Concat(new string[]
                    {
                            "Error: connection to ",
                            CentralServer.StandardUrl,
                            " failed. Website returned: ",
                            www.error,
                            " | Retrying in ",
                            180.ToString(),
                            " seconds..."
                    }), ConsoleColor.DarkRed);
                    www = null;
                    yield return Timing.WaitForSeconds(180f);
                }
            }
        }

        ServerConsole.AddLog("Initializing game server...", ConsoleColor.Gray);
        if (ConfigFile.HosterPolicy.GetString("ipv4_bind_ip", "none") != "none")
        {
            LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress = ConfigFile.HosterPolicy.GetString("ipv4_bind_ip", "0.0.0.0");
            if (LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress == "0.0.0.0")
            {
                ServerConsole.AddLog("Server starting at all IPv4 addresses and port " + LiteNetLib4MirrorTransport.Singleton.port.ToString() + " - set by your hosting provider.", ConsoleColor.Gray);
            }
            else
            {
                ServerConsole.AddLog(string.Concat(new string[]
                {
                    "Server starting at IPv4 ",
                    LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress,
                    " and port ",
                    LiteNetLib4MirrorTransport.Singleton.port.ToString(),
                    " - set by your hosting provider."
                }), ConsoleColor.Gray);
            }
        }
        else
        {
            LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress = ConfigFile.ServerConfig.GetString("ipv4_bind_ip", "0.0.0.0");
            if (LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress == "0.0.0.0")
            {
                ServerConsole.AddLog("Server starting at all IPv4 addresses and port " + LiteNetLib4MirrorTransport.Singleton.port.ToString(), ConsoleColor.Gray);
            }
            else
            {
                ServerConsole.AddLog("Server starting at IPv4 " + LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress + " and port " + LiteNetLib4MirrorTransport.Singleton.port.ToString(), ConsoleColor.Gray);
            }
        }
        if (ConfigFile.HosterPolicy.GetString("ipv6_bind_ip", "none") != "none")
        {
            LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress = ConfigFile.HosterPolicy.GetString("ipv6_bind_ip", "::");
            if (LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress == "::")
            {
                ServerConsole.AddLog("Server starting at all IPv6 addresses and port " + LiteNetLib4MirrorTransport.Singleton.port.ToString() + " - set by your hosting provider.", ConsoleColor.Gray);
            }
            else
            {
                ServerConsole.AddLog(string.Concat(new string[]
                {
                    "Server starting at IPv6 ",
                    LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress,
                    " and port ",
                    LiteNetLib4MirrorTransport.Singleton.port.ToString(),
                    " - set by your hosting provider."
                }), ConsoleColor.Gray);
            }
        }
        else
        {
            LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress = ConfigFile.ServerConfig.GetString("ipv6_bind_ip", "::");
            if (LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress == "::")
            {
                ServerConsole.AddLog("Server starting at all IPv6 addresses and port " + LiteNetLib4MirrorTransport.Singleton.port.ToString(), ConsoleColor.Gray);
            }
            else
            {
                ServerConsole.AddLog("Server starting at IPv6 " + LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress + " and port " + LiteNetLib4MirrorTransport.Singleton.port.ToString(), ConsoleColor.Gray);
            }
        }

        LiteNetLib4MirrorTransport.Singleton.useNativeSockets = ConfigFile.ServerConfig.GetBool("use_native_sockets", true);
        ServerConsole.AddLog("Network sockets mode: " + (LiteNetLib4MirrorTransport.Singleton.useNativeSockets ? "Native" : "Unity"), ConsoleColor.Gray);
        manager.StartHost();
        while (SceneManager.GetActiveScene().name != "Facility")
        {
            yield return Timing.WaitForOneFrame;
        }
        ServerConsole.AddLog("Level loaded. Creating match...", ConsoleColor.Gray);
        ServerListHandler.RunServer();
        yield break;
    }
}
