using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.Command;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AxonCommandAttribute : Attribute
{
}
