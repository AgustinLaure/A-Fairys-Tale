using UnityEngine;
using System;


public class JUMP_SFX : MonoBehaviour
{
    private ServiceLocator serviceLocator;
    private EventBus eventBus;
    [SerializeField] AK.Wwise.Event Jump;
    private void Start()
    {
        serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        eventBus.Subscribe<OnPlayerJump>((Action)playJump);
    }

    private void playJump()
    {
        AkUnitySoundEngine.PostEvent("Jump", gameObject);
    }
}