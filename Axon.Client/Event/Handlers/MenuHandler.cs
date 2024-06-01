using Axon.Shared.Event.Args;

namespace Axon.Shared.Event.Handlers;

public static class MenuHandler
{
    public static EventReactor<CreditHookEventArg> CreditsHook { get; } = new EventReactor<CreditHookEventArg>();

    internal static void Init()
    {
        EventManager.RegisterEvent(CreditsHook);
    }
}