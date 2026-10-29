using UnityEngine;

public class DropKey : MonoBehaviour
{
    [SerializeField] private AreaTrigger areaTrigger;

    private EventBus eventBus;

    private const string foxTag = "Fox";

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleTrigger;
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private void HandleTrigger(Collider collider)
    {
        if (collider.CompareTag(foxTag))
        {
            eventBus.Raise<OnFoxDropKey>();
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleTrigger;
    }
}
