using Axon.Client.Event;
using Il2CppCommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Meta;

public class MetaAnalyzer
{
    public EventReactor<MetaEvent> OnMeta = new EventReactor<MetaEvent>();

    internal MetaAnalyzer() { }

    public void Init()
    {
        AxonMod.EventManager.RegisterEvent(OnMeta);
    }

    public void Analyze()
    {
        var assembly = Assembly.GetCallingAssembly();
        AnalyzeAssembly(assembly);
    }

    public void AnalyzeAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            AnalyzeType(type);
        }
    }

    public void AnalyzeType(Type type)
    {
        if (type.GetCustomAttribute<Automatic>() == null) return;
        var ev = new MetaEvent(type);
        OnMeta.Raise(ev);
    }
}
