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
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
        Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;   
    }

    private void UnHookEvents()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
        Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
    }

    private void OnRoundStart()
    {
        Log.Info("StartRound");
        foreach (var player in ReferenceHub.AllHubs)
        {
            if (player.IsHost) continue;
            Log.Info(player.nicknameSync.Network_myNickSync);

            var msgTest = new TestMessage()
            {
                message = "Welcome on our Client Modded Server :D"
            };
            player.connectionToClient.Send(msgTest);


            var msg = new SpawnAssetMessage()
            {
                objectId = 1,
                bundleName = "v1",
                assetName = "Assets/v1.prefab",
                gameObjectName = "Axon Asset",
                position = currentPosition,
                rotation = Quaternion.Euler(currentRotation),
                scale = Vector3.one,
            };
            
            player.connectionToClient.Send(msg);
            Timing.RunCoroutine(V1Spin());
            Log.Info("Send spawnmessage to " + player.nicknameSync.Network_myNickSync);
        }
    }
    private Vector3 currentPosition = Vector3.zero;

    private Vector3 currentRotation = Vector3.zero;
    private void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        currentPosition = ev.Player.Transform.position;
    }
    private IEnumerator<float> V1Spin()
    {

        for (;;)
        {
            try
            {
                currentRotation.y += 2;

                var msg = new SpawnAssetMessage()
                {
                    objectId = 1,
                    bundleName = "v1",
                    assetName = "Assets/v1.prefab",
                    gameObjectName = "Axon Asset",
                    position = currentPosition,
                    rotation = Quaternion.Euler(currentRotation),
                    scale = Vector3.one*0.5f,
                };

                foreach (var player in ReferenceHub.AllHubs.Where(player => !player.IsHost))
                {
                    player.connectionToClient.Send(msg);
                }
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
