using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Shared.Command;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AxonCommandAttribute : Attribute
{
    public string Name {  get; set; }
    public string[] Aliase {  get; set; }
    public string Description { get; set; }
}
