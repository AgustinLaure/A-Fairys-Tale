using UnityEngine;
using System;


public class SubscribeTest : MonoBehaviour
{
    private ServiceLocator serviceLocator;
    private EventBus eventBus;
    [SerializeField] AK.Wwise.Event Bird;
    private void Start()
    {
        serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        eventBus.Subscribe<OnBirdFlap>((Action) playBird);
    }

    private void playBird()
    {
        AkUnitySoundEngine.PostEvent("Bird", gameObject);
    }
}
