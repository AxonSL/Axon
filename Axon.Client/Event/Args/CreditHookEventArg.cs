using Axon.Shared.Components;

namespace Axon.Shared.Event.Args;

public class CreditHookEventArg : IEvent
{
    public CreditHookEventArg(CreditsHookComponent creditsHookComponent)
    {
        CreditsHookComponent = creditsHookComponent;
    }
    public CreditsHookComponent CreditsHookComponent { get; private set; }
}