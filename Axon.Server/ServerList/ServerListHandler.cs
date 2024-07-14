using Exiled.API.Features;
using GameCore;
using NorthwoodLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axon.Server.ServerList;

public static class ServerListHandler
{
    private static Server _lastSend;

    public static Thread ServerListThread { get; private set; }

    internal static void RunServer()
    {
        if (ServerListThread != null) return;
        ServerListThread = new Thread(new ThreadStart(RefreshServerData))
        {
            IsBackground = true,
            Priority = ThreadPriority.AboveNormal,
            Name = "SCP:SL Server list thread",
        };
        ServerListThread.Start();
    }

    internal static async void RefreshServerData()
    {
        if (string.IsNullOrWhiteSpace(AxonPlugin.Instance.Config.Token) || string.IsNullOrWhiteSpace(AxonPlugin.Instance.Config.Server))
        {
            Exiled.API.Features.Log.Info("Server won't be displayed on a Serverlist. Please set url and token to be displayed on the server list");
            return;
        }
        var client = new HttpClient();
        while (!ServerConsole._disposing)
        {
            var url = AxonPlugin.Instance.Config.Server;
            var json = "{";
            var bits = 0;
            var current = new Server()
            {
                Ip = ServerConsole.Ip,
                Port = ServerConsole.PortToReport,
                Version = GameCore.Version.VersionString,
                Info = StringUtils.Base64Encode(ServerConsole.singleton.RefreshServerNameSafe()).Replace('+', '-'),
                Patsebin = ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT").Replace("\n",""),
                Geoblocking = CustomLiteNetLib4MirrorTransport.Geoblocking != GeoblockingMode.None,
                Whitelist = ServerConsole.WhiteListEnabled,
                AccessRestriction = ServerConsole.AccessRestriction,
                FriendlyFire = ServerConsole.FriendlyFire,
                Players = ServerConsole._playersAmount,
                MaxPlayers = CustomNetworkManager.slots,
            };

            json += "\"ip\": \"" + ServerConsole.Ip + "\"";
            json += ",\"port\": " + ServerConsole.PortToReport;

            if(_lastSend == null || _lastSend.Version != current.Version)
            {
                bits |= 1;
                json += ",\"version\": \"" + current.Version + "\"";
            }

            if (_lastSend == null || _lastSend.Info != current.Info)
            {
                bits |= 2;
                json += ",\"info\": \"" + current.Info + "\"";
            }

            if (_lastSend == null || _lastSend.Patsebin != current.Patsebin)
            {
                bits |= 4;
                json += ",\"pastebin\": \"" + current.Patsebin + "\"";
            }

            if (_lastSend == null || _lastSend.Geoblocking != current.Geoblocking)
            {
                bits |= 8;
                json += ",\"geoblocking\": " + current.Geoblocking.ToString().ToLower();
            }

            if (_lastSend == null || _lastSend.Whitelist != current.Whitelist)
            {
                bits |= 16;
                json += ",\"whitelist\": " + current.Whitelist.ToString().ToLower();
            }

            if (_lastSend == null || _lastSend.AccessRestriction != current.AccessRestriction)
            {
                bits |= 32;
                json += ",\"accessRestriction\": " + current.AccessRestriction.ToString().ToLower();
            }

            if (_lastSend == null || _lastSend.FriendlyFire != current.FriendlyFire)
            {
                bits |= 64;
                json += ",\"friendlyFire\": " + current.FriendlyFire.ToString().ToLower();
            }

            if (_lastSend == null || _lastSend.Players != current.Players)
            {
                bits |= 128;
                json += ",\"players\": " + current.Players;
            }

            if (_lastSend == null || _lastSend.MaxPlayers != current.MaxPlayers)
            {
                bits |= 256;
                json += ",\"maxPlayers\": " + current.MaxPlayers;
            }


            json += "}";
            Exiled.API.Features.Log.Debug(json);

            url += "?token=" + AxonPlugin.Instance.Config.Token;
            if (_lastSend != null)
                url += "&updatebit=" + bits;

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                Exiled.API.Features.Log.Error("Couldn't update data on Serverlist: " + response.ReasonPhrase);
                _lastSend = null;
            }
            else
            {
                _lastSend = current;
            }
            Thread.Sleep(60000);
        }
    }
}
