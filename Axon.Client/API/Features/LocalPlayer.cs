using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace Axon.Client.API.Features;
public class LocalPlayer
{
    public static Il2Cpp.ReferenceHub ReferenceHub => ReferenceHub.LocalHub;
    public static Dictionary<GameObject, Player> Dictionary = new Dictionary<GameObject, Player>();
    public Transform Transform => ReferenceHub.gameObject.transform;
}