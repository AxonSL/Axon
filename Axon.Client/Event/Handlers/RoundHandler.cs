using Axon.Client.Event.Args;
using Axon.Shared.Event;
using Axon.Shared.Meta;

namespace Axon.Client.Event.Handlers;

[Automatic]
public static class RoundHandler
{
    public static EventReactor<RoundStartEventArg> RoundStart { get; } = new EventReactor<RoundStartEventArg>();

    public static EventReactor<RoundEndEventArg> RoundEnd { get; } = new EventReactor<RoundEndEventArg>();

    public static EventReactor<RoundRestartEventArg> RoundRestart { get; } = new EventReactor<RoundRestartEventArg>();

    internal static void Init()
    {
        EventManager.RegisterEvent(RoundStart);
        EventManager.RegisterEvent(RoundEnd);
        EventManager.RegisterEvent(RoundRestart);
    }
}
