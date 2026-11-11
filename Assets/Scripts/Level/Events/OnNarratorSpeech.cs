public class OnNarratorSpeech : IEvent
{
    public int phraseIndex = -1;

    public void Set(params object[] data)
    {
        phraseIndex = (int)data[0];
    }
    public void Reset()
    {
        phraseIndex = -1;
    }
}
