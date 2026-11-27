using static PlayerCraft;

public class OnCraftMove : IEvent
{
    public Move move = default;

    public void Set(params object[] data)
    {
        move = (Move)data[0];
    }
    public void Reset()
    {
        move = default;
    }
}
