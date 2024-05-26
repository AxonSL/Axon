using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Axon.Client.Patches
{
    [HarmonyPatch(typeof(Il2CppGameCore.Console), nameof(Il2CppGameCore.Console.TypeCommand))]
    internal class test
    {
        [HarmonyPrefix]
        public static bool OnTypeCommand(string cmd)
        {
            try
            {
                if (!cmd.Contains("asset")) return true;

                MelonLogger.Msg("Received Command: " + cmd);
                var assetBundle = Mod.AssetBundleManager.AssetBundles.First().Value;

                foreach (var allAssetName in assetBundle.AllAssetNames())
                {
                    MelonLogger.Msg(allAssetName);
                }
                var asset = assetBundle.LoadAsset<GameObject>("Assets/v1.prefab");
                var obj = GameObject.Instantiate(asset);

                obj.name = "Asset";
                MelonLogger.Msg(obj.name);
                MelonLogger.Msg(obj.transform.position);
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                throw;
            }

        }
    }
}
