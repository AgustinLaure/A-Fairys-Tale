
using UnityEngine;

public class OnPortalUsed : IEvent
{
    public GameObject user = null;
    public bool shouldFaceLeft = false;

    public void Set(params object[] data)
    {
        user = data[0] as GameObject;
        shouldFaceLeft = (bool)data[1];
    }

    public void Reset()
    {
        user = null;
        shouldFaceLeft = false;
    }
}
