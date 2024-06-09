using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Axon.Client.Event.Args;
using Axon.Client.Event.Handlers;

namespace Axon.Client.Patches.Menu;

[HarmonyPatch(typeof(NewMainMenu),nameof(NewMainMenu.Start))]
public static class CanvasPatch
{
    [HarmonyPostfix]
    public static void OnMenuStartLate()
    {
        var ev = new CanvasReadyEventArg(GameObject.Find("Canvas"));
        MenuHandler.CanvasReady.Raise(ev);
    }
}