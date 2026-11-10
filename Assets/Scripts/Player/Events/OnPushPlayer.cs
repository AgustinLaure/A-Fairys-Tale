using UnityEngine;

public class OnPushPlayer : IEvent
{
    public Vector3 push;
    public float pushTime;

    public void Set(params object[] data)
    {
        push = (Vector3)data[0];
        pushTime = (float)data[1];
    }

    public void Reset()
    {
        push = Vector3.zero;
        pushTime = 0f;
    }
}
