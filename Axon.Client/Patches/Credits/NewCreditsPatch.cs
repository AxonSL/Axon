using Axon.Client.Components;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Axon.Client.Patches.Credits;
[HarmonyPatch(typeof(NewCredits), nameof(NewCredits.OnButtonClick))]
public class NewCreditsPatch
{
    [HarmonyPrefix]
    public static bool InjectCreditsHook()
    {

        if (CreditsHookComponent.Singleton != null)
            return true;
        GameObject creditsHookObject = new GameObject
        {
            name = "Credits Hook"
        };
        creditsHookObject.AddComponent<CreditsHookComponent>();
            
        return true;
    }
}