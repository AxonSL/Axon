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
using Axon.Client.Event;

[assembly: MelonInfo(typeof(AxonMod), "Axon", "0.0.1", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Client;

public class AxonMod : MelonMod
{
    public static AxonMod Instance { get; private set; }
    public static AssetBundleManager AssetBundleManager { get; private set; }
    public static MetaAnalyzer MetaAnalyzer { get; private set; }
    public static EventManager EventManager { get; private set; }

    public override void OnInitializeMelon()
    {
        CustomNetworkManager.Modded = true;
        Il2CppCentralAuth.CentralAuthManager.NoAuthStartupArg = "yes?";

        Instance = this;
        EventManager = new EventManager();
        AssetBundleManager = new AssetBundleManager();
        MetaAnalyzer = new MetaAnalyzer();

        EventManager.RegisterEvent(OnTestEvent);

        MetaAnalyzer.Init();
        EventManager.Init();
        AssetBundleManager.Init();
        HarmonyInstance.PatchAll();
        //Analyze should always be called last so that all handlers/events are registered
        MetaAnalyzer.Analyze();

        OnTestEvent.Raise(new());
        LoggerInstance.Msg("Axon Loaded");
    }

    public EventReactor<TestEvent> OnTestEvent = new();
    public class TestEvent : IEvent { }
}

[Automatic]
public class Test : EventListener
{
    [EventHandler]
    public void OnMeta(AxonMod.TestEvent ev)
    {
        MelonLogger.Msg("Automatic Event Worked!");
    }
}
