using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace Axon.Shared.API.Features;

public class LocalPlayer : Player
{
    private static LocalPlayer _instance = new LocalPlayer();
    public static LocalPlayer Get()
    {
        return _instance;
    }
    public LocalPlayer()
    {
        ReferenceHub = ReferenceHub.LocalHub;
    }
    public Camera Camera => ReferenceHub.PlayerCameraReference.gameObject.GetComponent<Camera>();
}