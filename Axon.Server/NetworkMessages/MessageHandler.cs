using Axon.NetworkMessages;
using Exiled.API.Features;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Server.NetworkMessages;

public static class MessageHandler
{
    internal static void Init()
    {
        //Testmessage
        Writer<TestMessage>.write = WriteTest;
        Reader<TestMessage>.read = ReadTest;
        NetworkServer.RegisterHandler<TestMessage>(OnTestMessage);
        Log.Info("Testmessage: " + typeof(TestMessage).FullName.GetStableHashCode());

        //SpawnAssetMessage
        Writer<SpawnAssetMessage>.write = WriteSpawnAssetMessage;
        Reader<SpawnAssetMessage>.read = ReadSpawnAssetMessage;
        NetworkServer.RegisterHandler<SpawnAssetMessage>(OnSpawnAssetMessage);
        Log.Info("SpawnAssetMessage: " + typeof(SpawnAssetMessage).FullName.GetStableHashCode());
    }

    #region TestMessage
    private static void WriteTest(NetworkWriter writer, TestMessage message)
    {
        writer.WriteString(message.message);
    }

    private static TestMessage ReadTest(NetworkReader reader)
    {
        return new()
        {
            message = reader.ReadString(),
        };
    }

    private static void OnTestMessage(NetworkConnection connection, TestMessage message)
    {
        Log.Info("Got Testmessage: " + message);
    }
    #endregion

    #region SpawnAssetMessage
    private static void WriteSpawnAssetMessage(NetworkWriter writer, SpawnAssetMessage message)
    {
        writer.WriteUInt(message.objectId);
        writer.WriteString(message.bundleName);
        writer.WriteString(message.assetName);
        writer.WriteString(message.gameObjectName);
        writer.WriteVector3(message.position);
        writer.WriteQuaternion(message.rotation);
        writer.WriteVector3(message.scale);
    }

    private static SpawnAssetMessage ReadSpawnAssetMessage(NetworkReader reader)
    {
        return new SpawnAssetMessage()
        {
            objectId = reader.ReadUInt(),
            bundleName = reader.ReadString(),
            assetName = reader.ReadString(),
            gameObjectName = reader.ReadString(),
            position = reader.ReadVector3(),
            rotation = reader.ReadQuaternion(),
            scale = reader.ReadVector3(),
        };
    }

    private static void OnSpawnAssetMessage(NetworkConnection connection, SpawnAssetMessage message) { }
    #endregion
}
