using UnityEngine;

public class OnActionLockStateChanged : IEvent
{
    public bool state = false;
    public void Set(params object[] data)
    {
        state = (bool)data[0];
    }
    public void Reset()
    {
        state = false;
    }
}
