using UnityEngine;

public class OnMoleSwapped : IEvent
{
    public bool isMoleShown = false;
    public void Set(params object[] data)
    {
        isMoleShown = (bool)data[0];
    }
    public void Reset()
    {
        isMoleShown = false;
    }
}
