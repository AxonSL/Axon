using UnityEngine;

namespace Axon.Server.AssetBundle;

public class SpawnedAsset
{
    public GameObject GameObject { get; set; }
    public AxonAssetScript Script { get; set; }
    public uint Id { get; set; }
    public string Bundle {  get; set; }
    public string AssetName { get; set; }
}
