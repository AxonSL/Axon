using Axon.Client.Event.Args;
using Axon.Shared.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Event.Handlers;

public static class UnityHandler
{
    public static EventReactor<SceneLoadedEventArg> SceneLoaded { get; } = new();

    internal static void Init()
    {
        EventManager.RegisterEvent(SceneLoaded);
    }
}
