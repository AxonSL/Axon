using Axon.Client.AssetBundle;
using UnityEngine;

namespace Axon.Client.AssetBundle;

public class SpawnedAsset
{
    public GameObject GameObject { get; internal set; }
    public AxonAssetScript Script { get; internal set; }
    public uint Id { get; internal set; }
    public string Bundle {  get; internal set; }
    public string AssetName { get; internal set; }
    public string[] Components { get; internal set; }
}
