using Axon.Client.AssetBundle;
using Axon.Client.Components;
using Axon.Client.Event.Args;
using Axon.Shared.Event;
using Axon.Shared.Meta;
using MelonLoader;
using UnityEngine.UI;
using UnityEngine;

namespace Axon.Client.Event;

[Automatic]
public class AxonEventHandler : EventListener
{
    [EventHandler]
    public void OnHookCredits(Axon.Client.Event.Args.CreditHookEventArg ev)
    {
        MelonLogger.Msg("Appling Credits");
        var component = ev.CreditsHookComponent;

        // Axon Client Credits
        component.CreateCreditsCategory("Axon Client");
        component.CreateCreditsEntry("Dimenzio", "Maintainer", "Axon Client", CreditColors.CrabPink);
        component.CreateCreditsEntry("Tili", "Developer", "Axon Client", CreditColors.DevBlue);

        component.CreateCreditsEntry("Helight", "Helper", "Axon Client", CreditColors.Yellow);

        // Axon Server Credits
        component.CreateCreditsCategory("Axon Server");
        component.CreateCreditsEntry("Dimenzio", "Creator, Maintainer", "Axon Server", CreditColors.Red);

        component.CreateCreditsCategory("Axon Client - Honorable Mentions");
        component.CreateCreditsEntry("Lava Gang", "MelonLoader", "Axon Client - Honorable Mentions", CreditColors.Yellow);
        component.CreateCreditsEntry("Pardeike", "HarmonyX", "Axon Client - Honorable Mentions", CreditColors.Yellow);
        component.CreateCreditsEntry("ModdedMcPlayer", "GameAssembly Patch Support", "Axon Client - Honorable Mentions", CreditColors.Yellow);
    }

    [EventHandler]
    public void OnRoundRestart(RoundRestartEventArg _)
    {
        AssetBundleSpawner.SpawnedAssets = new(new List<SpawnedAsset>());
    }

    [EventHandler]
    public void OnCanvasReady(CanvasReadyEventArg _)
    {
        var texture = new Texture2D(600, 600);
        ImageConversion.LoadImage(texture, File.ReadAllBytes("axon.png"));
        GameObject.Find("Canvas/Logo").GetComponent<RawImage>().texture = texture;

        var text = GameObject.Find("Canvas/Version").GetComponent<Text>();
        var gameVersion = text.text;
        text.text = "Axon Version: " + AxonMod.AxonVersion + " Game Version: " + gameVersion;

        AuthHandler.Init();
    }
}
