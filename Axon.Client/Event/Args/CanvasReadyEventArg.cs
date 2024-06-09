using Axon.Shared.Event;
using UnityEngine;

namespace Axon.Client.Event.Args;

public class CanvasReadyEventArg : IEvent
{
    public CanvasReadyEventArg(GameObject canvas)
    {
        Canvas = canvas;
    }

    public GameObject Canvas { get; private set; }
}
