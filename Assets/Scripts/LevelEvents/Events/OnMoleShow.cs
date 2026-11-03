using UnityEngine;

public class OnMoleShow : IEvent
{
    public Mole mole = null;
    public void Set(params object[] data)
    {
        mole = data[0] as Mole;
    }
    public void Reset()
    {
        mole = null;
    }
}
