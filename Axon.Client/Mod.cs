using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using Axon.Client;
using System.IO;
using Axon.Client.AssetBundle;
using Il2Cpp;
using Axon.Client.Meta;

[assembly: MelonInfo(typeof(Mod), "Axon", "0.0.1", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Client;

public class Mod : MelonMod
{
    public static Mod Instance { get; private set; }

    public static AssetBundleManager AssetBundleManager { get; private set; }

    public static MetaAnalyzer MetaAnalyzer { get; private set; }

    public override void OnInitializeMelon()
    {
        CustomNetworkManager.Modded = true;
        Il2CppCentralAuth.CentralAuthManager.NoAuthStartupArg = "yes?";

        HarmonyInstance.PatchAll();

        Instance = this;

        AssetBundleManager = new AssetBundleManager();
        AssetBundleManager.Init();

        MetaAnalyzer = new MetaAnalyzer();
        MetaAnalyzer.AnalyzeAssembly(MelonAssembly.Assembly);

        LoggerInstance.Msg("Axon Loaded");
    }
}
