using Axon.Shared.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axon.Client.API.Features;
using System.Security.Cryptography;
using Il2Cpp;
using MelonLoader;
using System.Xml.Serialization;
using System.Reflection.Metadata;
using Il2CppLiteNetLib.Utils;
using Il2CppLiteNetLib;
using System.Collections;
using UnityEngine;

namespace Axon.Client.Auth;

public static class AuthHandler
{
    private static Dictionary<string, ServerConnection> _connections = new();
    
    public static byte[] CurrentKey { get; private set; }

    public static string AuthFilePath { get; private set; }
    public static PlayerAuth PlayerAuth { get; private set; }

    internal static void Init()
    {
        AuthFilePath = Path.Combine(Paths.AxonPath, "user.xml");
        if (!File.Exists(AuthFilePath))
            CreateNew();

        var serializer = new XmlSerializer(typeof(PlayerAuth));
        var stream = new FileStream(AuthFilePath, FileMode.Open, FileAccess.Read);
        PlayerAuth = (PlayerAuth)serializer.Deserialize(stream);
        stream.Close();
    }

    internal static void AuthWrite(NetDataWriter writer)
    {
        try
        {
            var server = Il2CppMirror.LiteNetLib4Mirror.LiteNetLib4MirrorTransport.Singleton.clientAddress;
            var isSecondTime = false;

            if (_connections.TryGetValue(server, out var connection))
            {
                if (connection.Handshake != null)
                {
                    isSecondTime = true;
                }
                else
                {
                    _connections.Remove(server);
                }
            }

            //Writes the Game Version
            writer.Put((byte)(isSecondTime ? 4 : 3));
            writer.Put(Il2CppGameCore.Version.Major);
            writer.Put(Il2CppGameCore.Version.Minor);
            writer.Put(Il2CppGameCore.Version.Revision);
            writer.Put(Il2CppGameCore.Version.BackwardCompatibility);
            if (Il2CppGameCore.Version.BackwardCompatibility)
                writer.Put(Il2CppGameCore.Version.BackwardRevision);

            //Writes how long this Request is valid
            writer.Put(TimeBehaviour.CurrentUnixTimestamp + 30);

            if (!isSecondTime)
            {
                var clientEnv = ClientAuthSession.Generate(PlayerAuth.GetIdentity());
                _connections[server] = new ServerConnection
                {
                    Session = clientEnv
                };

                var clientHandshake = clientEnv.CreateHandshake();

                writer.PutBytesWithLength(clientHandshake.Encode());
            }
            else
            {
                _connections.Remove(server);
                var attempt = connection.Session.CreateLoginAttempt(connection.Handshake);
                writer.PutBytesWithLength(attempt.Encode());
                writer.Put(connection.ServerIdentifier);
                writer.Put(PlayerAuth.Username);

                CurrentKey = connection.Session.GetSharedSecret(connection.Handshake);
            }
        }
        catch(Exception e)
        {
            MelonLogger.Error("Error during authentication to server: " + e);
        }
    }

    internal static void RejectAuth(DisconnectInfo info)
    {
        MelonLogger.Warning("Rejected Auth");
        var data = info.AdditionalData;
        var requestType = data.GetByte();
        if (requestType != 100)
        {
            info.AdditionalData._position = 0;
            return;
        }

        var encrypted = data.GetBytesWithLength();
        var handShake = ServerHandshake.Decode(encrypted);
        var server = Il2CppMirror.LiteNetLib4Mirror.LiteNetLib4MirrorTransport.Singleton.clientAddress;

        if (!_connections.TryGetValue(server, out var connection))
        {
            MelonLogger.Error("Got an Handshake from a Server eventough no ClientSession exist");
            return;
        }
        connection.Handshake = handShake;
        connection.ServerIdentifier = data.GetString();

        MelonCoroutines.Start(ConnectAgain(server, Il2CppMirror.LiteNetLib4Mirror.LiteNetLib4MirrorTransport.Singleton.port.ToString()));
    }

    private static IEnumerator ConnectAgain(string ip, string port)
    {
        yield return new WaitForSeconds(2);
        Il2CppGameCore.Console.singleton.TypeCommand("connect " + ip + ":" + port);
    }

    private static void CreateNew()
    {
        var auth = new PlayerAuth()
        {
            Username = Welcome.CurrentNickname
        };
        auth.SetIdentity(AuthCrypto.CreateIdentity());

        var sw = new StringWriter();
        var serializer = new XmlSerializer(typeof(PlayerAuth));
        serializer.Serialize(sw, auth);

        File.Create(AuthFilePath).Close();
        File.WriteAllText(AuthFilePath, sw.ToString(), Encoding.Unicode);
    }
}
