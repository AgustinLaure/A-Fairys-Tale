using UnityEngine;

public class OnLayerChange : IEvent
{
    public int newLayer;

    public void Set(params object[] data)
    {
        newLayer = (int)data[0];

        Debug.Log("Changed layer event: to " + newLayer);
    }

    public void Reset()
    {
        newLayer = -1;
    }
}