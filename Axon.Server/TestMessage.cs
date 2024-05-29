using System;

namespace Axon.NetworkMessages;

public struct TestMessage : Mirror.NetworkMessage
{
    public string message;
}
