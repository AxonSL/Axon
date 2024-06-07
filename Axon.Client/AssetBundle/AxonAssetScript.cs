using Axon.Client.AssetBundle;
using Il2CppInterop.Runtime.Attributes;
using MelonLoader;
using UnityEngine;

namespace Axon.Client.AssetBundle;

[RegisterTypeInIl2Cpp]
public class AxonAssetScript : MonoBehaviour
{
    public AxonAssetScript(IntPtr ptr) : base(ptr) { }

    [HideFromIl2Cpp]
    public SpawnedAsset SpawnedAsset { get; internal set; }
}
