using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace Axon.Client.Patches.News;

[HarmonyPatch(typeof(NewsLoader), nameof(NewsLoader.Start))]
public class NewsLoaderStartPatch
{
    [HarmonyPrefix]
    public static bool Prefix(NewsLoader __instance)
    {

        __instance._announcements = new Il2CppSystem.Collections.Generic.List<NewsLoader.Announcement>();
        __instance._announcements.Clear();
        __instance._announcements.Add(new NewsLoader.Announcement(
            $"<color=blue>Axon</color>",
            "<b><size=20>Welcome to Axon, a SCP:SL modded version.</size></b>\n" +
            "\n<color=#ec0c02>Alpha Built</color>",
            "25.05.24",
            "https://scopesl.com", null));
        __instance.ShowAnnouncement(0);

        return false;
    }
}
