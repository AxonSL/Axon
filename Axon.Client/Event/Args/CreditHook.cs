using Axon.Client.Components;

namespace Axon.Client.Event.Args;

public class CreditHook : IEvent
{
    public CreditHook(CreditsHookComponent creditsHookComponent)
    {
        CreditsHookComponent = creditsHookComponent;
    }
    public CreditsHookComponent CreditsHookComponent { get; private set; }
}