using System.ComponentModel;
using Axon.Client;
using Axon.Client.AssetBundle;
using Axon.Client.Components;
using Axon.Client.Event;
using Axon.Client.Event.Args;
using Axon.Client.Event.Handlers;
using Axon.Client.Meta;
using Axon.Client.NetworkMessages;
using Il2Cpp;
using MelonLoader;
using CommandHandler = Axon.Client.Command.CommandHandler;

[assembly: MelonInfo(typeof(AxonMod), "Axon", "0.0.1", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Client;

public class AxonMod : MelonMod
{
    public static readonly Version AxonVersion = new Version(0, 0, 1);

    public static AxonMod Instance { get; private set; }
    public static EventManager EventManager { get; private set; }

    public override void OnInitializeMelon()
    {
        CustomNetworkManager.Modded = true;
        Il2CppCentralAuth.CentralAuthManager.NoAuthStartupArg = "yes?";

        Instance = this;
        EventManager = new EventManager();

        MetaAnalyzer.Init();
        EventManager.Init();
        AssetBundleManager.Init();
        CommandHandler.Init();
        MessageHandler.Init();
        MenuHandler.Init();
        
        //Analyze should always be called last so that all handlers/events are registered
        MetaAnalyzer.Analyze();
        MenuHandler.CreditsHook.Subscribe(CreateCredits);
        LoggerInstance.Msg("Axon Loaded");
    }
    private void CreateCredits(CreditHook e)
    {
        MelonLogger.Msg("Appling Credits");
        var component = e.CreditsHookComponent;
        // Synapse Client Credits
        component.CreateCreditsCategory("Axon Client");
        component.CreateCreditsEntry("Dimenzio", "Maintainer", "Axon Client", CreditColors.CrabPink);
        component.CreateCreditsEntry("Tili", "Developer", "Axon Client", CreditColors.DevBlue);

        component.CreateCreditsEntry("Helight", "Helper", "Axon Client", CreditColors.Yellow);

        // Synapse Server Credits
        component.CreateCreditsCategory("Axon Server");
        component.CreateCreditsEntry("Dimenzio", "Creator, Maintainer", "Axon Server", CreditColors.Red);

        component.CreateCreditsCategory("Axon Client - Honorable Mentions");
        component.CreateCreditsEntry("Lava Gang", "MelonLoader", "Axon Client - Honorable Mentions", CreditColors.Yellow);
        component.CreateCreditsEntry("ModdedMcPlayer", "Executable Support", "Axon Client - Honorable Mentions", CreditColors.Yellow);
        component.CreateCreditsEntry("Pardeike", "HarmonyX", "Axon Client - Honorable Mentions", CreditColors.Yellow);
        component.CreateCreditsEntry("Zasbszk", "Who is this guy?", "Axon Client - Honorable Mentions", CreditColors.Purple); 
    }
}