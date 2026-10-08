using UnityEngine;

public class OnLayerChange : IEvent
{
    public int lastLayer;
    public int newLayer;

    public void Set(params object[] data)
    {
        lastLayer = (int)data[0];
        newLayer = (int)data[1];

        Debug.Log("Changed layer event: from " + lastLayer + " to " + newLayer);
    }

    public void Reset()
    {
        lastLayer = -1;
        newLayer = -1;
    }
}