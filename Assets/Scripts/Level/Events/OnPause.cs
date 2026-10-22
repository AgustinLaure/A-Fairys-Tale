using UnityEngine;

public class OnPause : IEvent
{
    private bool isPaused;

    public bool IsPaused()
    {
        return isPaused; 
    }

    public void Set(params object[] data)
    {
        isPaused = (bool)data[0];
    }
    public void Reset()
    {
    }
}
