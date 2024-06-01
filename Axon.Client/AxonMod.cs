using Axon.Client;
using Axon.Client.AssetBundle;
using Axon.Client.Components;
using Axon.Client.Event;
using Axon.Client.Event.Args;
using Axon.Client.Event.Handlers;
using Axon.Client.NetworkMessages;
using Axon.Shared;
using Axon.Shared.Event;
using Axon.Shared.Meta;
using Il2Cpp;
using MelonLoader;
using CommandHandler = Axon.Client.Command.CommandHandler;

[assembly: MelonInfo(typeof(AxonMod), "Axon", "0.1.0", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Client;

public class AxonMod : MelonMod
{
    public static readonly Version AxonVersion = new Version(0, 1, 0);

    public static AxonMod Instance { get; private set; }

    public override void OnInitializeMelon()
    {
        CustomNetworkManager.Modded = true;
        Il2CppCentralAuth.CentralAuthManager.NoAuthStartupArg = "yes?";

        Instance = this;

        ShareMain.Init();
        CommandHandler.Init();
        MessageHandler.Init();
        AssetBundleManager.Init();

        MenuHandler.Init();
        RoundHandler.Init();
        
        //Analyze should always be called last so that all handlers/events are registered
        MetaAnalyzer.Analyze();
        LoggerInstance.Msg("Axon Loaded");
    }
}