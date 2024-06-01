using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Event;

/// <summary>
/// Extendable class for easily wrapping methods to event reactor delegates.
/// Can be registered via <see cref="EventManager.RegisterListener"/>
/// </summary>
public abstract class EventListener
{
    private Dictionary<Type, object> _subscriptions = new();

    internal void RegisterAll()
    {
        foreach (var methodInfo in GetType().GetMethods().Where(method => method.GetCustomAttribute<EventHandlerAttribute>() is not null))
        {
            var attribute = methodInfo.GetCustomAttribute<EventHandlerAttribute>();
            var parameters = methodInfo.GetParameters();
            if (parameters.Length != 1) throw new Exception("EventHandler must have a single Event parameter");
            var eventParameter = parameters[0];
            var eventType = eventParameter.ParameterType;
            var reactor = EventManager.GetUnsafe(eventType);
            var subscribeUnsafe = reactor.SubscribeUnsafe(this, methodInfo, attribute.Priority);
            _subscriptions[eventType] = subscribeUnsafe;
        }
    }

    /// <summary>
    /// Unregisters all handlers of the listener.
    /// </summary>
    /// <exception cref="Exception">if the listener is not linked</exception>
    public void UnregisterAll()
    {
        foreach (var subscription in _subscriptions)
        {
            EventManager.GetUnsafe(subscription.Key).UnsubscribeUnsafe(subscription.Value);
        }

        _subscriptions.Clear();
    }
}

public class EventHandlerAttribute : Attribute
{
    public int Priority { get; set; } = 0;
}