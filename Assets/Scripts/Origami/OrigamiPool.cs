using UnityEngine;
using System;
using System.Collections.Generic;

public class OrigamiPool : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnityPool aicraftUnityPool;
    [SerializeField] private UnityPool frogUnityPool;
    [SerializeField] private UnityPool craneUnityPool;
    private EventBus eventBus;

    private Dictionary<Type, UnityPool> typeToPool;

    private void Awake()
    {
        typeToPool = new Dictionary<Type, UnityPool>()
        {
            [typeof(Aircraft)] = aicraftUnityPool,
            [typeof(Frog)] = frogUnityPool,
            [typeof(Crane)] = craneUnityPool
        };
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();

        eventBus.Subscribe<OnDisableOrigami>((Action<OnDisableOrigami>)HandleOrigamiDisable);
    }

    public GameObject GetOrigami(Type type, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (typeToPool.TryGetValue(type, out UnityPool pool))
        {
            return pool.GetItem(position,rotation,parent);
        }

        return null;
    }

    public GameObject GetOrigami(Type type) 
    {
        if (typeToPool.TryGetValue(type, out UnityPool pool))
        {
            return pool.GetItem();
        }

        return null;
    }

    private void HandleOrigamiDisable(OnDisableOrigami data)
    {
        if (typeToPool.TryGetValue(data.type, out UnityPool pool))
        {
            pool.ReturnItem(data.gameObject);
        }
    }
}
