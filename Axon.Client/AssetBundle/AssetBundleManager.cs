using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.AssetBundle;

public static class AssetBundleManager
{
    private static bool _initiated = false;

    public const string AssetDirectoryName = "AssetBundles";
    public static string AssetDirectory { get; private set; }
    public static ReadOnlyDictionary<string, Il2CppAssetBundle> AssetBundles { get; private set; } = new(new Dictionary<string, Il2CppAssetBundle>());

    internal static void Init()
    {
        if(_initiated) return;

        var current = Directory.GetCurrentDirectory();
        AssetDirectory = Path.Combine(current, AssetDirectoryName);
        if (!Directory.Exists(AssetDirectory))
            Directory.CreateDirectory(AssetDirectory);

        LoadAssets();
        _initiated = true;
    }

    private static void LoadAssets()
    {
        foreach(var assetPath in Directory.GetFiles(AssetDirectory))
        {
            var asset = Il2CppAssetBundleManager.LoadFromFile(assetPath);
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var dic = new Dictionary<string, Il2CppAssetBundle>(AssetBundles);
            dic[fileName] = asset;
            AssetBundles = new(dic);
        }
    }
}
