public class OnOrigamiUsed : IEvent
{
    public string tag = default;

    public void Set(params object[] data)
    {
        tag = (string)data[0];
    }
    public void Reset()
    {
        tag = default;
    }
}
