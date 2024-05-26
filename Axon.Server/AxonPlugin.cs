using Exiled.API.Enums;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Server;

public class AxonPlugin : Plugin<AxonConfig>
{
    public override string Author => "Axon";
    public override string Name => "Axon server plugin";
    public override PluginPriority Priority => PluginPriority.Higher;
    public override Version Version => new Version(0, 0, 1);

    public static AxonPlugin Instance { get; private set; }

    public override void OnEnabled()
    {
        Log.Info("Axon Server plugin loaded!");
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        base.OnDisabled();
    }
}
