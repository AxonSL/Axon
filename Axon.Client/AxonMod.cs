using Axon.Client;
using Axon.Client.AssetBundle;
using Axon.Client.Event;
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

        //Analyze should always be called last so that all handlers/events are registered
        MetaAnalyzer.Analyze();
        LoggerInstance.Msg("Axon Loaded");
    }
}