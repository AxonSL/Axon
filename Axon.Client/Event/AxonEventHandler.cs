using Axon.Shared.Components;
using Axon.Shared.Meta;
using MelonLoader;

namespace Axon.Shared.Event;

[Automatic]
public class AxonEventHandler : EventListener
{
    [EventHandler]
    public void OnHookCredits(Axon.Shared.Event.Args.CreditHookEventArg ev)
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
}
