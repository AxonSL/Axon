using Axon.Shared.Event;
using System;
using System.Reflection;

namespace Axon.Shared.Meta;

public static class MetaAnalyzer
{
    public static EventReactor<MetaEvent> OnMeta = new EventReactor<MetaEvent>();

    public static void Analyze()
    {
        var assembly = Assembly.GetCallingAssembly();
        AnalyzeAssembly(assembly);
    }

    public static void AnalyzeAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            AnalyzeType(type);
        }
    }

    public static void AnalyzeType(Type type)
    {
        if (type.GetCustomAttribute<Automatic>() == null) return;
        var ev = new MetaEvent(type);
        AnalyzeForInit(ev);
        OnMeta.Raise(ev);
    }

    private static void AnalyzeForInit(MetaEvent ev)
    {
        foreach(var method in ev.Type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if(method.GetCustomAttribute<Init>() == null) continue;
            method.Invoke(ev, null);
        }
    }
}

