using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axon.Shared.CustomScripts;
using Axon.Shared.Meta;
using Exiled.API.Features;
using Mirror;

namespace Axon.Server.AssetBundle.CustomScript;

[Automatic]
[AxonScript("MyPlugin.Example")]
public class ExampleScript : AxonCustomScript
{
    public string MyField
    {
        get => myField;
        set
        {
            myField = value;
            UpdateSyncVar(AxonSyncVarId.one);
        }
    }

    [AxonSyncVar(AxonSyncVarId.one, false)]
    public string myField;

    protected override void WriteSyncVar(AxonSyncVarId syncVarId, NetworkWriter writer)
    {
        switch(syncVarId)
        {
            case AxonSyncVarId.one:
                writer.WriteString(myField);
                break;
        }
    }

    protected override void ReadSyncVar(AxonSyncVarId syncVarId, NetworkReader reader)
    {
        switch(syncVarId)
        {
            case AxonSyncVarId.one:
                myField = reader.ReadString();
                Log.Info("Example MyFiled SyncVar was set to: " + myField);
                return;
        }
        return;
    }
}
