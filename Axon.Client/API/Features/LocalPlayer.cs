using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace Axon.Client.API.Features;

public class LocalPlayer : Player
{
    private static LocalPlayer _localPlayer;

    public LocalPlayer(ReferenceHub hub) : base(hub) { }
    public LocalPlayer(GameObject obj) : base(obj) { }

    public static LocalPlayer Instance
    {
        get
        {
            if(_localPlayer?.ReferenceHub == null)
            {
                _localPlayer = new LocalPlayer(ReferenceHub.LocalHub);
            }

            return _localPlayer;
        }
    }
    

    public Camera Camera => ReferenceHub.PlayerCameraReference.gameObject.GetComponent<Camera>();
}