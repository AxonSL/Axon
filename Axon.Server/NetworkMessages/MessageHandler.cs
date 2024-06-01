using Axon.NetworkMessages;
using Axon.Server.Event;
using Exiled.API.Features;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        //UpdateAssetMessage
        Writer<UpdateAssetMessage>.write = WriteUpdateAssetMessage;
        Reader<UpdateAssetMessage>.read = ReadUpdateAssetMessage;
        NetworkServer.RegisterHandler<UpdateAssetMessage>(OnUpdateAssetMessage);

        //EventMessage
        Writer<EventMessage>.write = WriteEventMessage;
        Reader<EventMessage>.read = ReadEventMessage;
        NetworkServer.RegisterHandler<EventMessage>(OnEventMessage);
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
        Log.Info("Got Testmessage: " + message.message);
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

    private static void OnSpawnAssetMessage(NetworkConnection connection, SpawnAssetMessage message)
    {
        Log.Warn("Client sended SpawnAssetMessage");
    }
    #endregion

    #region UpdateAssetMessage
    private static void WriteUpdateAssetMessage(NetworkWriter writer, UpdateAssetMessage message)
    {
        writer.WriteUInt(message.objectId);
        writer.WriteULong(message.syncDirtyBits);

        if (message.syncDirtyBits == 0) return;

        if ((message.syncDirtyBits & 1) == 1)
            writer.WriteVector3(message.position);

        if ((message.syncDirtyBits & 2) == 2)
            writer.WriteQuaternion(message.rotation);

        if ((message.syncDirtyBits & 4) == 4)
            writer.WriteVector3(message.scale);
    }

    private static UpdateAssetMessage ReadUpdateAssetMessage(NetworkReader reader)
    {
        var message = new UpdateAssetMessage()
        {
            objectId = reader.ReadUInt(),
            syncDirtyBits = reader.ReadULong(),
        };

        if(message.syncDirtyBits == 0)
            return message;

        if ((message.syncDirtyBits & 1) == 1)
            message.position = reader.ReadVector3();

        if ((message.syncDirtyBits & 2) == 2)
            message.rotation = reader.ReadQuaternion();

        if ((message.syncDirtyBits & 4) == 4)
            message.scale = reader.ReadVector3();

        return message;
    }

    private static void OnUpdateAssetMessage(NetworkConnection connection, UpdateAssetMessage message)
    {
        Log.Warn("Client sended UpdateAssetMessage");
    }
    #endregion

    #region EventMessage
    private static void WriteEventMessage(NetworkWriter writer, EventMessage message)
    {
        writer.WriteInt((int)message.eventId);
    }

    private static EventMessage ReadEventMessage(NetworkReader reader)
    {
        return new EventMessage()
        {
            eventId = (EventMessageId)reader.ReadInt()
        };
    }

    private static void OnEventMessage(NetworkConnection connection, EventMessage message)
    {
        Log.Info("Client sended a Event message");
    }
    #endregion
}
