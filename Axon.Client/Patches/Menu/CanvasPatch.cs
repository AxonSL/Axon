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

namespace Axon.Shared.Patches.Menu;

[HarmonyPatch(typeof(NewMainMenu),nameof(NewMainMenu.Start))]
public static class CanvasPatch
{
    [HarmonyPrefix]
    public static void OnMenuStart()
    {
        var texture = new Texture2D(600, 600);
        ImageConversion.LoadImage(texture, File.ReadAllBytes("axon.png"));
        GameObject.Find("Canvas/Logo").GetComponent<RawImage>().texture = texture;
    }

    [HarmonyPostfix]
    public static void OnMenuStartLate()
    {
        var text = GameObject.Find("Canvas/Version").GetComponent<Text>();
        var gameVersion = text.text;
        text.text = "Axon Version: " + AxonMod.AxonVersion + " Game Version: " + gameVersion;
    }
}