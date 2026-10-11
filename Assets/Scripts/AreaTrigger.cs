using System;
using System.Reflection;
using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    public event Action<Collider> OnTriggerEntered;
    public event Action<Collider> OnTriggerExited;

    [Header("References")]
    [SerializeField] private string triggerEnterEventClassName;
    [SerializeField] private string triggerExitEventClassName;

    [Header("Configuration")]
    [SerializeField] private bool passEnterCollisionData;
    [SerializeField] private bool passExitCollisionData;

    private EventBus eventBus;
    private MethodInfo genericTriggerEnterRaise;
    private MethodInfo genericTriggerExitRaise;

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();

        if (!string.IsNullOrEmpty(triggerEnterEventClassName))
        {
            Type enterType = Type.GetType(triggerEnterEventClassName);
            if (enterType != null)
            {
                genericTriggerEnterRaise = eventBus.GetGenericRaiseMethod(enterType);
            }
            else
            {
                Debug.LogError($"Could not find type '{triggerEnterEventClassName}'. Ensure the namespace is included if it has one.");
            }
        }

        if (!string.IsNullOrEmpty(triggerExitEventClassName))
        {
            Type exitType = Type.GetType(triggerExitEventClassName);
            if (exitType != null)
            {
                genericTriggerExitRaise = eventBus.GetGenericRaiseMethod(exitType);
            }
            else
            {
                Debug.LogError($"Could not find type '{triggerExitEventClassName}'. Ensure the namespace is included if it has one.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEntered?.Invoke(other);

        if (genericTriggerEnterRaise != null)
        {
            if (passEnterCollisionData)
            {
                genericTriggerEnterRaise.Invoke(eventBus, new object[] { new object[] { other } });
            }
            else
            {
                genericTriggerEnterRaise.Invoke(eventBus, new object[] { new object[0] });
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExited?.Invoke(other);

        if (genericTriggerExitRaise != null)
        {
            if (passExitCollisionData)
            {
                genericTriggerExitRaise.Invoke(eventBus, new object[] { new object[] { other } });
            }
            else
            {
                genericTriggerExitRaise.Invoke(eventBus, new object[] { new object[0] });
            }
        }
    }
}
