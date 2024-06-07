using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Shared.CustomScripts;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AxonScriptAttribute : Attribute
{
    public AxonScriptAttribute(string uniqueName)
    {
        UniqueName = uniqueName;
    }

    public string UniqueName { get; set; }
}