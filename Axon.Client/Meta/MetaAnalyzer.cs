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
    public void AnalyzeAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            AnalyzeType(type);
        }
    }

    public void AnalyzeType(Type type)
    {
        //write later with event system
        if (type.GetCustomAttribute<Automatic>() == null) return;
    }
}
