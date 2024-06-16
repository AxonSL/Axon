using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppGameObjectPools;
using Il2CppPlayerRoles;
using MelonLoader;
using UnityEngine;

namespace Axon.Client.Role;

[HarmonyPatch]
public static class RolePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerRoleLoader),nameof(PlayerRoleLoader.LoadRoles))]
    public static void OnLoadRoles()
    {
        MelonLogger.Msg("Try Load Roles");
        var rolePrefab = PlayerRoleLoader._loadedRoles[RoleTypeId.FacilityGuard];
        var newRolePrefab = GameObject.Instantiate(rolePrefab, Vector3.zero, Quaternion.identity);
        var comp = newRolePrefab.GetComponent<HumanRole>();
        comp._roleColor = Color.yellow;
        comp._roleId = (RoleTypeId)44;
        comp._team = Team.Scientists;
        PoolManager.Singleton.TryAddPool(newRolePrefab);
        PlayerRoleLoader._loadedRoles[(RoleTypeId)44] = newRolePrefab;
    }
}
