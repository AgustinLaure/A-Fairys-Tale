using UnityEngine;

public class OnDeathAreaCollision : IEvent
{

    public Collider collider = null;

    public void Set(params object[] data)
    {
        collider = data[0] as Collider;
    }
    public void Reset()
    {
        collider = null;
    }
}
