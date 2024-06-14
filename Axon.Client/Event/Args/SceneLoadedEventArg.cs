using Axon.Shared.Event;
using UnityEngine.SceneManagement;

namespace Axon.Client.Event.Args;

public class SceneLoadedEventArg : IEvent
{
    public SceneLoadedEventArg(Scene scene, LoadSceneMode sceneMode)
    {
        Scene = scene;
        SceneMode = sceneMode;
    }

    public Scene Scene { get; }

    public LoadSceneMode SceneMode { get; }
}
