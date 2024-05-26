using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.AssetBundle;

public class AssetBundleManager
{
    private bool _initiated = false;

    public const string AssetDirectoryName = "AssetBundles";
    public string AssetDirectory { get; private set; }
    public Dictionary<string, Il2CppAssetBundle> AssetBundles { get; private set; } = new();

    public void Init()
    {
        if(_initiated) return;

        var current = Directory.GetCurrentDirectory();
        AssetDirectory = Path.Combine(current, AssetDirectoryName);
        if (!Directory.Exists(AssetDirectory))
            Directory.CreateDirectory(AssetDirectory);

        LoadAssets();
        _initiated = true;
    }

    private void LoadAssets()
    {
        foreach(var assetPath in Directory.GetFiles(AssetDirectory))
        {
            var asset = Il2CppAssetBundleManager.LoadFromFile(assetPath);
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            AssetBundles.Add(fileName, asset);
        }
    }
}
