using System;
using System.Collections.Generic;
using System.Linq;
using Axon.Shared.Meta;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib;

namespace Axon.Server.Patches.ExiledPatches;

[HarmonyPatch]
public class PlayerGetPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.Get),typeof(string))]
    public static bool Prefix(string args, ref Player __result)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                __result = null;
                return false;

            }

            if (Player.UserIdsCache.TryGetValue(args, out var playerFound) && playerFound.IsConnected)
            {
                __result = playerFound;
                return false;
            }

            if (int.TryParse(args, out var id))
            {
                __result = Player.Get(id);
                return false;

            }

            if (args.EndsWith("@axon"))
            {
                foreach (var player in Player.Dictionary.Values.Where(player => player.UserId == args))
                {
                    playerFound = player;
                    break;
                }
            }
            else
            {
                var lastnameDifference = 31;
                var firstString = args.ToLower();

                foreach (var player in Player.Dictionary.Values)
                {
                    if (!player.IsVerified || player.Nickname is null)
                        continue;

                    if (!player.Nickname.Contains(args, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var secondString = player.Nickname;

                    var nameDifference = secondString.Length - firstString.Length;
                    if (nameDifference >= lastnameDifference)
                        continue;
                    lastnameDifference = nameDifference;
                    playerFound = player;
                }
            }

            if (playerFound is not null)
                Player.UserIdsCache[playerFound.UserId] = playerFound;

            __result = playerFound;
            return false;
        }
        catch (Exception exception)
        {
            Log.Error($"{typeof(Player).FullName}.{nameof(Player.Get)} error: {exception}");
            __result = null;
            return false;
        }
    }
}