using Axon.NetworkMessages;
using UnityEngine;

namespace Axon.Server.AssetBundle;

public class AxonAssetScript : MonoBehaviour
{
    public uint ObjectId { get; internal set; }

    private Vector3 _lastPos = Vector3.zero;
    private Quaternion _lastRot = Quaternion.identity;
    private Vector3 _lastScale = Vector3.one;

    public void Awake()
    {
        _lastPos = transform.position;
        _lastRot = transform.rotation;
        _lastScale = transform.localScale;
    }

    public void LateUpdate()
    {
        SendUpdateMessage();
    }

    public void SendUpdateMessage()
    {
        var msg = new UpdateAssetMessage()
        {
            objectId = ObjectId,
            syncDirtyBits = 0,
            position = _lastPos,
            rotation = _lastRot,
            scale = _lastScale,
        };

        if(_lastPos != transform.position)
        {
            msg.position = transform.position;
            _lastPos = transform.position;
            msg.syncDirtyBits |= 1;
        }
        if(_lastRot != transform.rotation)
        {
            msg.rotation = transform.rotation;
            _lastRot = transform.rotation;
            msg.syncDirtyBits |= 2;
        }
        if(_lastScale != transform.localScale)
        {
            msg.scale = transform.localScale;
            _lastScale = transform.localScale;
            msg.syncDirtyBits |= 4;
        }
        if (msg.syncDirtyBits == 0) return;

        foreach (var hub in ReferenceHub.AllHubs)
        {
            if (hub.IsHost) continue;
            hub.connectionToClient.Send(msg);
        }
    }
}
