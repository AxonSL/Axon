using Axon.Shared.CustomScripts;
using Axon.Shared.Meta;
using Exiled.API.Features;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Server.AssetBundle.CustomScript;

[Automatic]
[AxonScript("Axon.AxonHandlerScript")]
public class AxonHandlerScript : AxonCustomScript
{
    public int Test
    {
        get => test;
        set
        {
            test = value;
            UpdateSyncVar(AxonSyncVarId.one);
        }
    }

    [AxonSyncVar(AxonSyncVarId.one, true)]
    public int test;

    protected override void WriteSyncVar(AxonSyncVarId syncVarId, NetworkWriter writer)
    {
        switch (syncVarId)
        {
            case AxonSyncVarId.one:
                writer.WriteInt(test);
                break;
        }
    }

    protected override void ReadSyncVar(AxonSyncVarId syncVarId, NetworkReader reader)
    {
        switch (syncVarId)
        {
            case AxonSyncVarId.one:
                test = reader.ReadInt();
                Log.Info("Test SyncVar was set to: " + test);
                return;
        }
        return;
    }
}
