using Axon.Shared.CustomScripts;
using Axon.Shared.Meta;
using Il2CppGameCore;
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
[AxonScript("Axon.AxonHandlerScript")]
public class AxonHandlerScript : AxonCustomScript
{
    public AxonHandlerScript(IntPtr ptr) : base(ptr) { }

    public override void Awake()
    {
        base.Awake();
        MelonLogger.Warning("AxonHandlerScript added");
    }

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
                MelonLogger.Msg("Test SyncVar was set to: " + test);
                return;
        }
        return;
    }
}
