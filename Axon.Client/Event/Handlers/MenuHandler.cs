using Axon.Client.Event.Args;

namespace Axon.Client.Event.Handlers;

public static class MenuHandler
{
    internal static void Init()
    {
        AxonMod.EventManager.RegisterEvent(CreditsHook);
    }
    public static EventReactor<CreditHook> CreditsHook = new EventReactor<CreditHook>();
}