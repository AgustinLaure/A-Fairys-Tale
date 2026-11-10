using UnityEngine;
using System;

public class Test : MonoBehaviour
{
    private ServiceLocator serviceLocator;
    private EventBus eventBus;

    private void Start()
    {
        serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        eventBus.Subscribe<OnPlayerJump>((Action)HandleFoo);
    }

    private void HandleFoo()
    {

    }
}
