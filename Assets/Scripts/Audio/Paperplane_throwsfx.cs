using UnityEngine;
using System;


public class Paperplane_throwsfx : MonoBehaviour
{
    private ServiceLocator serviceLocator;
    private EventBus eventBus;
    [SerializeField] AK.Wwise.Event Paperplane;
    private void Start()
    {
        serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        eventBus.Subscribe<OnPlayerThrow>((Action)playPaperplane);
    }

    private void playPaperplane()
    {
        AkUnitySoundEngine.PostEvent("Paperplane", gameObject);
    }
}