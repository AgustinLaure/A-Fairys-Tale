using UnityEngine;

public class OnPlayerReparented : IEvent
{
    public GameObject currentLayer = null;

    public void Set(params object[] data)
    {
        currentLayer = data[0] as GameObject;
    }
    public void Reset()
    {
        currentLayer = null;
    }
}
