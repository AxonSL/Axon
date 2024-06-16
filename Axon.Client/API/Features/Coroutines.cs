using MelonLoader;
using System.Collections;
using UnityEngine;

namespace Axon.Client.API.Features;

public static class Coroutines
{
    public static object Start(IEnumerator enumerator)
        => MelonCoroutines.Start(enumerator);

    public static void Stop(object coroutine)
        => MelonCoroutines.Stop(coroutine);

    public static void CallDelayed(float time, Action action)
        => MelonCoroutines.Start(CallDelayedCoroutine(time, action));

    private static IEnumerator CallDelayedCoroutine(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
}
