using Axon.Client.Components;

namespace Axon.Client.Event.Args;

public class CreditHookEventArg : IEvent
{
    public CreditHookEventArg(CreditsHookComponent creditsHookComponent)
    {
        CreditsHookComponent = creditsHookComponent;
    }
    public CreditsHookComponent CreditsHookComponent { get; private set; }
}