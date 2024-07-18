using Axon.Shared;
using Axon.NetworkMessages;
using Axon.Server.NetworkMessages;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using UnityEngine;
using Axon.Server.AssetBundle;
using Axon.Server.Event;
using Axon.Shared.Event;
using Axon.Shared.Meta;
using Axon.Server.AssetBundle.CustomScript;
using HarmonyLib;
using CentralAuth;
using Mirror;

namespace Axon.Server;

public class AxonPlugin : Plugin<AxonConfig>
{
    public override string Author => "Axon";
    public override string Name => "Axon server plugin";
    public override PluginPriority Priority => PluginPriority.Higher;
    public override Version Version => new Version(0, 0, 1);
    public Harmony Harmony { get; }

    public static AxonPlugin Instance { get; private set; }

    public AxonPlugin()
    {
        ShareMain.Init();
        Harmony = new Harmony("Axon.Server");
    }

    public override void OnEnabled()
    {
        PlayerAuthenticationManager.OnlineMode = false;
        Instance = this;

        HookEvents();
        MessageHandler.Init();
        AssetBundleSpawner.Init();
        MetaAnalyzer.Analyze();
        Harmony.PatchAll();
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        UnHookEvents();
        Harmony.UnpatchAll();
        base.OnDisabled();
    }

    private void HookEvents()
    {
        //AssetBundle
        Exiled.Events.Handlers.Player.Joined += AssetBundleSpawner.OnJoin;
        Exiled.Events.Handlers.Server.RestartingRound += AssetBundleSpawner.OnRoundRestart;

        //RoundHandler
        Exiled.Events.Handlers.Server.RoundStarted += RoundEventHandler.OnRoundStart;
        Exiled.Events.Handlers.Server.RoundEnded += RoundEventHandler.OnRoundEnd;
        Exiled.Events.Handlers.Server.RestartingRound += RoundEventHandler.OnRoundRestart;
        
    }

    private void UnHookEvents()
    {
        Exiled.Events.Handlers.Player.Joined -= AssetBundleSpawner.OnJoin;
        Exiled.Events.Handlers.Server.RestartingRound -= AssetBundleSpawner.OnRoundRestart;
    }
    
}
