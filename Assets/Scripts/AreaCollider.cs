using System;
using System.Reflection;
using UnityEngine;

public class AreaCollider : MonoBehaviour
{
    public event Action<Collision> OnColliderEntered;
    public event Action<Collision> OnColliderExited;

    [Header("References")]
    [SerializeField] private string colliderEnterEventClassName;
    [SerializeField] private string colliderExitEventClassName;

    [Header("Configuration")]
    [SerializeField] private bool passEnterCollisionData;
    [SerializeField] private bool passExitCollisionData;

    private EventBus eventBus;
    private MethodInfo genericTriggerEnterRaise;
    private MethodInfo genericTriggerExitRaise;

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();

        if (!string.IsNullOrEmpty(colliderEnterEventClassName))
        {
            Type enterType = Type.GetType(colliderEnterEventClassName);
            if (enterType != null)
            {
                genericTriggerEnterRaise = eventBus.GetGenericRaiseMethod(enterType);
            }
            else
            {
                Debug.LogError($"[AreaCollider] Type '{colliderEnterEventClassName}' could not be found.");
            }
        }

        if (!string.IsNullOrEmpty(colliderExitEventClassName))
        {
            Type exitType = Type.GetType(colliderExitEventClassName);
            if (exitType != null)
            {
                genericTriggerExitRaise = eventBus.GetGenericRaiseMethod(exitType);
            }
            else
            {
                Debug.LogError($"[AreaCollider] Type '{colliderExitEventClassName}' could not be found.");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnColliderEntered?.Invoke(collision);

        if (genericTriggerEnterRaise != null)
        {
            if (passEnterCollisionData)
            {
                genericTriggerEnterRaise.Invoke(eventBus, new object[] { new object[] { collision } });
            }
            else
            {
                genericTriggerEnterRaise.Invoke(eventBus, new object[] { new object[0] });
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        OnColliderExited?.Invoke(collision);

        if (genericTriggerExitRaise != null)
        {
            if (passExitCollisionData)
            {
                genericTriggerExitRaise.Invoke(eventBus, new object[] { new object[] { collision } });
            }
            else
            {
                genericTriggerExitRaise.Invoke(eventBus, new object[] { new object[0] });
            }
        }
    }
}
