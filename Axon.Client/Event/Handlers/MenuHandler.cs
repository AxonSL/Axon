using Axon.Client.Event.Args;
using Axon.Shared.Event;
using Axon.Shared.Meta;

namespace Axon.Client.Event.Handlers;

public static class MenuHandler
{
    public static EventReactor<CreditHookEventArg> CreditsHook { get; } = new();

    public static EventReactor<CanvasReadyEventArg> CanvasReady { get; } = new();

    internal static void Init()
    {
        EventManager.RegisterEvent(CreditsHook);
        EventManager.RegisterEvent(CanvasReady);
    }
}