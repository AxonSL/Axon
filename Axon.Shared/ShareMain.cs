using Axon.Shared.Event;
using Axon.Shared.Meta;

namespace Axon.Shared;

public static class ShareMain
{
    public static void Init()
    {
        EventManager.Init();
        MetaAnalyzer.Init();
        //MetaAnalyzer.Analyze();
    }
}
