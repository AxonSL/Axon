using Axon.Shared;
using Axon.Shared.AssetBundle;
using Axon.Shared.Components;
using Axon.Shared.Event;
using Axon.Shared.Event.Args;
using Axon.Shared.Event.Handlers;
using Axon.Shared.Meta;
using Axon.Shared.NetworkMessages;
using Il2Cpp;
using MelonLoader;
using CommandHandler = Axon.Shared.Command.CommandHandler;

[assembly: MelonInfo(typeof(AxonMod), "Axon", "0.1.0", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Shared;

public class AxonMod : MelonMod
{
    public static readonly Version AxonVersion = new Version(0, 1, 0);

    public static AxonMod Instance { get; private set; }

    public override void OnInitializeMelon()
    {
        CustomNetworkManager.Modded = true;
        Il2CppCentralAuth.CentralAuthManager.NoAuthStartupArg = "yes?";

        Instance = this;

        EventManager.Init();
        AssetBundleManager.Init();
        CommandHandler.Init();
        MessageHandler.Init();
        MenuHandler.Init();
        RoundHandler.Init();
        
        //Analyze should always be called last so that all handlers/events are registered
        MetaAnalyzer.Analyze();
        LoggerInstance.Msg("Axon Loaded");
    }
}