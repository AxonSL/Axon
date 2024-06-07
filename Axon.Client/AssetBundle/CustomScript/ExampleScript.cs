using Axon.Shared.CustomScripts;
using Axon.Shared.Meta;
using Il2CppMirror;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Client.AssetBundle.CustomScript;

[Automatic]
[RegisterTypeInIl2Cpp]
[AxonScript("MyPlugin.Example")]
public class ExampleScript : AxonCustomScript
{
    public ExampleScript(IntPtr ptr) : base(ptr) { }

    public override void Awake()
    {
        base.Awake();
        MelonLogger.Warning("Example Script added");
    }

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
    public string myField = "";

    protected override void WriteSyncVar(AxonSyncVarId syncVarId, NetworkWriter writer)
    {
        switch (syncVarId)
        {
            case AxonSyncVarId.one:
                writer.WriteString(myField);
                break;
        }
    }

    protected override void ReadSyncVar(AxonSyncVarId syncVarId, NetworkReader reader)
    {
        switch (syncVarId)
        {
            case AxonSyncVarId.one:
                myField = reader.ReadString();
                MelonLogger.Msg("SyncVar MyField was set to:" + myField);
                MyField = "YEAH IT FUCKING WORKS THAT TOOK WAY TOO LONG";
                return;
        }
        return;
    }
}
