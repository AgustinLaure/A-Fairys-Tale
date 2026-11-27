
public class OnSceneLoaded : IEvent
{
    public string sceneName = default;

    public void Set(params object[] data)
    {
        sceneName = (string)data[0];
    }
    public void Reset()
    {
        sceneName = default;
    }
}
