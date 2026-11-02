using UnityEngine;

public class OnMoleSwapped : IEvent
{
    public Mole mole;
    public bool isMoleShown = false;
    public void Set(params object[] data)
    {
        isMoleShown = (bool)data[0];
        mole = data[1] as Mole;
    }
    public void Reset()
    {
        isMoleShown = false;
        mole = null;
    }
}
