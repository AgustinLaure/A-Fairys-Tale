using UnityEngine;

public class OnOrigamiFolded : IEvent
{
    OrigamiFigure figure;

    public string GetOrigamiName()
    {
        return figure.name;
    }

    public void Set(params object[] data)
    {
        figure = (OrigamiFigure)data[0];
    }

    public void Reset()
    {
    }
}
