using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using Axon.Client;
using Il2Cpp;
using Il2CppPlayerRoles.FirstPersonControl;
using Il2CppPlayerStatsSystem;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "Axon", "0.0.1", "Dimenzio & Tiliboyy")]
[assembly: MelonGame("Northwood", "SCPSL")]
namespace Axon.Client;

public class Mod : MelonMod
{
    public static Mod Instance { get; private set; }

    public override void OnInitializeMelon()
    {
        HarmonyInstance.PatchAll();
        Instance = this;
        LoggerInstance.Msg("Axon Loaded");
    }
    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        //Todo: Fix this cursed shit
        if (sceneName == "Facility")
        {
            var comps = UserMainInterface.singleton.PlyStats.transform.parent.gameObject;
     
            MelonLogger.Msg(comps.name);
        }
        base.OnSceneWasLoaded(buildIndex, sceneName);
    }
}
