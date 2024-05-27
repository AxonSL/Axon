using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axon.Server;

public class AxonPlugin : Plugin<AxonConfig>
{
    public const uint AssetBundleMirrorId = 34324;

    public override string Author => "Axon";
    public override string Name => "Axon server plugin";
    public override PluginPriority Priority => PluginPriority.Higher;
    public override Version Version => new Version(0, 0, 1);

    public static AxonPlugin Instance { get; private set; }

    public override void OnEnabled()
    {
        HookEvents();
        Log.Info("Axon Server plugin loaded!");
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        UnHookEvents();
        base.OnDisabled();
    }

    private void HookEvents()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers += WaitingForPlayers;
        Exiled.Events.Handlers.Server.RoundStarted += RoundStart;
        Exiled.Events.Handlers.Player.InteractingDoor += Door;   
    }

    private void UnHookEvents()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers -= WaitingForPlayers;
        Exiled.Events.Handlers.Server.RoundStarted -= RoundStart;
        Exiled.Events.Handlers.Player.InteractingDoor -= Door;
    }

    private void WaitingForPlayers()
    {
        
    }

    private void RoundStart()
    {
        Log.Info("StartRound");

        foreach (var player in ReferenceHub.AllHubs)
        {
            if (player.IsHost) continue;
            Log.Info(player.nicknameSync.Network_myNickSync);
            var msg = new SpawnMessage()
            {
                sceneId = 34324,
                position = player.transform.position,
                rotation = player.transform.rotation,
            };
            player.connectionToClient.Send(msg);
            Log.Info("Send spawnmessage to " + player.nicknameSync.Network_myNickSync);
        }
    }

    private void Door(InteractingDoorEventArgs ev)
    {
        var msg = new SpawnMessage()
        {
            sceneId = 34324,
            position = ev.Player.Position,
            rotation = ev.Player.Rotation,
            scale = Vector3.one,
            netId = 1,
        };
        ev.Player.Connection.Send(msg);
    }
}
