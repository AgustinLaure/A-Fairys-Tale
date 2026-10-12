using UnityEngine;

public class OnPlayerIcedChanged : IEvent
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
