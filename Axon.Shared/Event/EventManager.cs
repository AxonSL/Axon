using Axon.Shared.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Shared.Event;

public static class EventManager
{
    public static Dictionary<Type, IEventReactor> Reactors = new();

    /// <summary>
    /// Registers an event reactor.
    /// </summary>
    /// <typeparam name="T">type of the reactor which shall be subscribed</typeparam>
    public static EventReactor<T> RegisterEvent<T>() where T : IEvent
    {
        var reactor = new EventReactor<T>();
        Reactors[typeof(T)] = reactor;
        return reactor;
    }

    /// <summary>
    /// Registers an event reactor.
    /// </summary>
    public static void RegisterEvent(IEventReactor reactor)
    {
        Reactors[reactor.TypeDelegate()] = reactor;
    }

    /// <summary>
    /// Unregisters an event reactor.
    /// </summary>
    /// <param name="eventType">type of the reactor which shall be unsubscribed</param>
    public static void UnregisterEvent(Type eventType)
    {
        Reactors.Remove(eventType);
    }

    /// <summary>
    /// Unregisters an event reactor.
    /// </summary>
    public static void UnregisterEvent(IEventReactor reactor)
    {
        Reactors.Remove(reactor.TypeDelegate());
    }

    /// <summary>
    /// Unregisters an event reactor.
    /// </summary>
    /// <typeparam name="T">type of the reactor which shall be unsubscribed</typeparam>
    public static void UnregisterEvent<T>()
    {
        Reactors.Remove(typeof(T));
    }

    /// <summary>
    /// Retrieves an event reactor.
    /// </summary>
    /// <typeparam name="T">type of the reactor</typeparam>
    public static EventReactor<T> Get<T>() where T : IEvent
    {
        return (EventReactor<T>)Reactors[typeof(T)];
    }

    /// <summary>
    /// Retrieves an event reactor.
    /// </summary>
    /// <param name="type">type of the reactor</param>
    public static IEventReactor GetUnsafe(Type type)
    {
        return Reactors[type];
    }

    /// <summary>
    /// Retrieves the reactor related to the event and invokes the multicast event system.
    /// See <see cref="EventReactor{T}"/> for details about required property attributes.
    /// </summary>
    /// <param name="evt">the event argument object</param>
    public static void Raise(IEvent evt)
    {
        Reactors[evt.GetType()].RaiseUnsafe(evt);
    }

    /// <summary>
    /// Registers an event listener.
    /// </summary>
    /// <param name="listener">the listener which shall be subscribed</param>
    public static void RegisterListener(EventListener listener)
    {
        listener.RegisterAll();
    }

    public static void Init() => MetaAnalyzer.OnMeta.Subscribe(LoadMeta);

    private static void LoadMeta(MetaEvent ev)
    {
        if (!ev.Is<EventListener>()) return;
        var listener = ev.CreateAs<EventListener>();
        listener.RegisterAll();
    }
}
