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

[assembly: MelonInfo(typeof(Mod), "Axon", "0.0.1", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Client;

public class Mod : MelonMod
{
    public static Mod Instance { get; private set; }

    public static AssetBundleManager AssetBundleManager { get; private set; }

    public override void OnInitializeMelon()
    {
        HarmonyInstance.PatchAll();
        Instance = this;
        AssetBundleManager = new AssetBundleManager();
        AssetBundleManager.Init();
        LoggerInstance.Msg("Axon Loaded");
        LoggerInstance.Msg(Directory.GetCurrentDirectory());
    }
}
