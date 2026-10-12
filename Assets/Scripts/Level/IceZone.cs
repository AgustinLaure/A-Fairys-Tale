using System;
using UnityEngine;

public class IceZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AreaTrigger areaTrigger;
    private EventBus eventBus;

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleAreaTriggerEnter;
        areaTrigger.OnTriggerExited += HandleAreaTriggerExit;
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private void HandleAreaTriggerEnter(Collider collider)
    {
        eventBus.Raise<OnPlayerIcedChanged>(true);
    }

    private void HandleAreaTriggerExit(Collider collider)
    {
        eventBus.Raise<OnPlayerIcedChanged>(false);
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleAreaTriggerEnter;
        areaTrigger.OnTriggerExited -= HandleAreaTriggerExit;

    }
}
