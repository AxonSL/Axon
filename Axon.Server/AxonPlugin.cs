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

namespace Axon.Server;

public class AxonPlugin : Plugin<AxonConfig>
{
    public override string Author => "Axon";
    public override string Name => "Axon server plugin";
    public override PluginPriority Priority => PluginPriority.Higher;
    public override Version Version => new Version(0, 0, 1);

    public static AxonPlugin Instance { get; private set; }

    public override void OnEnabled()
    {
        Instance = this;
        MessageHandler.Init();
        AssetBundleSpawner.Init();
        HookEvents();
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        UnHookEvents();
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
        foreach (var player in ReferenceHub.AllHubs)
        {
            if (player.IsHost) continue;

            var msgTest = new TestMessage()
            {
                message = "Welcome on our Client Modded Server :D"
            };
            player.connectionToClient.Send(msgTest);

            v1 = AssetBundleSpawner.SpawnAsset("v1", "Assets/v1.prefab", "Axon Asset", player.transform.position, player.transform.rotation, Vector3.one * 0.5f);
        }
        //Timing.RunCoroutine(V1Spin());
    }
    private GameObject v1;

    private void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        v1.transform.position = ev.Player.Transform.position;
    }

    private IEnumerator<float> V1Spin()
    {
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
