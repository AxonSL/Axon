using Exiled.API.Features;
using GameObjectPools;
using HarmonyLib;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Axon.Server.Patches;

[HarmonyPatch]
public static class RolePatch
{
    private static PlayerRoleBase _role;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerRoleLoader), nameof(PlayerRoleLoader.LoadRoles))]
    public static void OnLoadRoles()
    {
        Log.Info("Try Load Roles");
        var rolePrefab = PlayerRoleLoader._loadedRoles[RoleTypeId.FacilityGuard];
        var newRolePrefab = GameObject.Instantiate(rolePrefab, Vector3.zero, Quaternion.identity);
        var comp = newRolePrefab.GetComponent<HumanRole>();
        comp._roleColor = Color.yellow;
        comp._roleId = (RoleTypeId)44;
        comp._team = Team.Scientists;
        PoolManager.Singleton.TryAddPool(newRolePrefab);
        PlayerRoleLoader._loadedRoles[(RoleTypeId)44] = newRolePrefab;

        PlayerRoleLoader.TryGetRoleTemplate<PlayerRoleBase>((RoleTypeId)44, out var test);
        Log.Info("Role null?: " + test == null);
        _role = newRolePrefab;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerRoleManager),nameof(PlayerRoleManager.GetRoleBase))]
    public static bool OnGetRoleBase(PlayerRoleManager __instance,out PlayerRoleBase __result,RoleTypeId targetId)
    {
        if(targetId == (RoleTypeId)44)
        {
            var rolePrefab = PlayerRoleLoader._loadedRoles[RoleTypeId.FacilityGuard];
            var newRolePrefab = GameObject.Instantiate(rolePrefab, Vector3.zero, Quaternion.identity);
            var comp = newRolePrefab.GetComponent<HumanRole>();
            comp._roleColor = Color.yellow;
            comp._roleId = (RoleTypeId)44;
            comp._team = Team.Scientists;

            __result = GameObject.Instantiate(newRolePrefab);
            return false;
        }
        __result = null;
        return true;
    }
}