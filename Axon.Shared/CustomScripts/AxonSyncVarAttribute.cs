using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axon.Shared.CustomScripts;

[AttributeUsage(AttributeTargets.Field)]
public class AxonSyncVarAttribute : Attribute
{
    public AxonSyncVarAttribute(AxonSyncVarId syncVarId, bool serverOnly = true)
    {
        SyncVarId = syncVarId;
        ServerOnly = serverOnly;
    }

    public AxonSyncVarId SyncVarId { get; set; }

    /// <summary>
    /// When set to true only the Server is allowed to change this Variable
    /// </summary>
    public bool ServerOnly { get; set; }
}

