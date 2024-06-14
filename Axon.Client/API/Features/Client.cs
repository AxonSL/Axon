using Il2Cpp;
using Il2CppMirror;
using Il2CppMirror.LiteNetLib4Mirror;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Axon.Client.API.Features;

public static class Client
{
    private static NewMainMenu _mainMenu;
    public static NewMainMenu MainMenu
    {
        get
        {
            if(_mainMenu == null)
            {
                _mainMenu = UnityEngine.Object.FindObjectOfType<NewMainMenu>();
            }

            return _mainMenu;
        }
    }

    public static string CurrentSceneName => SceneManager.GetActiveScene().name;

    public static string ServerIp => LiteNetLib4MirrorTransport.Singleton.clientAddress;
    public static ushort ServerPort => LiteNetLib4MirrorTransport.Singleton.port;

    public static bool IsConnected => NetworkClient.isConnected;

    public static void QuitGame() => Application.Quit();

    public static void Connect(string address)
    {
        if(!IsConnected)
            MainMenu.Connect(address);
    }

    public static void Disconnect()
    {
        if (IsConnected)
        {
            Il2CppGameCore.Console.singleton.TypeCommand("disconnect");
            Il2CppGameCore.Console.singleton._clientCommandLogs.RemoveAt(Il2CppGameCore.Console.singleton._clientCommandLogs.Count - 1);
        }
    }

    public static void Reconnect()
        => Reconnect(ServerIp + ":" + ServerPort);

    public static void Reconnect(string address)
    {
        Disconnect();
        Coroutines.CallDelayed(0.5f, () => Connect(address));
    }
}
