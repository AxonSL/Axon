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

        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
        Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;   
    }

    private void UnHookEvents()
    {
        Exiled.Events.Handlers.Player.Joined -= AssetBundleSpawner.OnJoin;
        Exiled.Events.Handlers.Server.RestartingRound -= AssetBundleSpawner.OnRoundRestart;

        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
        Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
    }

    private void OnRoundStart()
    {
        AssetBundleSpawner.SpawnAsset("dimenzio", "Assets/Map_v2.prefab", "custom map", new Vector3(0f,1020f,0f));
        AssetBundleSpawner.SpawnAsset("default", "empty", "AxonHandler", "Axon.AxonHandlerScript").GetComponent<AxonHandlerScript>().Test = 10;
        foreach (var player in ReferenceHub.AllHubs)
        {
            if (player.IsHost) continue;

            var msgTest = new TestMessage()
            {
                message = "Welcome on our Client Modded Server :D"
            };
            player.connectionToClient.Send(msgTest);

            v1 = AssetBundleSpawner.SpawnAsset("v1", "Assets/v1.prefab", "Axon Asset", player.transform.position - player.transform.forward, player.transform.rotation, Vector3.one * 0.5f, "MyPlugin.Example");
        }
        Timing.RunCoroutine(V1Spin());
    }
    private AxonAssetScript v1;

    private void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        v1.transform.position = ev.Player.Transform.position - ev.Player.Transform.forward;
    }

    private IEnumerator<float> V1Spin()
    {
        yield return Timing.WaitForSeconds(2f);
        v1.GetComponent<ExampleScript>().MyField = "Please just work and be set client side my brain is already molten";
        yield break;
        for (;;)
        {
            try
            {
                var currentRotation = v1.transform.rotation.eulerAngles;
                //currentRotation.y += 2;
                //currentRotation.x += 2;
                //currentRotation.z += 2;

                var pos = v1.transform.position;
                //pos.x += 0.01f;

                var scale = v1.transform.localScale;
                scale.y += 0.005f;

                v1.transform.rotation = Quaternion.Euler(currentRotation);
                v1.transform.position = pos;
                v1.transform.localScale = scale;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
            }
            yield return Timing.WaitForOneFrame;

        }
    } 
}
