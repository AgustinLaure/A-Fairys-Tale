using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class EventBus
{
    private class FunctionNotFound : Exception
    {
        public FunctionNotFound(string message) : base(message)
        {

        }
    }

    private Dictionary<Type, List<Delegate>> statesToInstance;

    private Dictionary<Type, MethodInfo> genericMethod;

    private CompositePool compositePool;

    private MethodInfo baseRaiseMethod;

    public MethodInfo GetBaseRaiseMethod { get { return baseRaiseMethod; } }

    public EventBus()
    {
        statesToInstance = new Dictionary<Type, List<Delegate>>();
        genericMethod = new Dictionary<Type, MethodInfo>();

        compositePool = ServiceLocator.Instance.GetService<CompositePool>();

        baseRaiseMethod = this.GetType().GetMethod("Raise");
    }

    public void Subscribe<T>(Delegate function) where T : IEvent
    {
        if (statesToInstance.TryGetValue(typeof(T), out var delegates))
        {
            delegates.Add(function);
        }
        else
        {
            List<Delegate> newList = new List<Delegate>();
            newList.Add(function);

            statesToInstance.Add(typeof(T), newList);
        }
    }

    public void Unsubscribe<T>(Delegate function)
    {
        try
        {
            if (statesToInstance.TryGetValue(typeof(T), out var delegates) && delegates.Count > 0)
            {
                delegates.Remove(function);
            }
            else
            {
                throw new FunctionNotFound("Tried to unsubscribe a non existing function");
            }
        }
        catch (FunctionNotFound exception)
        {
            Debug.LogError(exception.Message);
        }
    }

    public void Raise<T>(params object[] data) where T : IEvent, new()
    {
        if (statesToInstance.TryGetValue(typeof(T), out var delegates))
        {
            if (data.Length <= 0)
            {
                for (int i = delegates.Count - 1; i >= 0; i--)
                {
                    Delegate delegateIter = delegates[i];

                    if (delegateIter.Target is UnityEngine.Object unityObj && unityObj == null)
                    {
                        delegates.RemoveAt(i);
                        continue;
                    }

                    delegateIter?.DynamicInvoke();
                }
            }
            else
            {
                T newEvent = compositePool.GetItemFromPool<T>();

                newEvent.Set(data);

                for (int i = delegates.Count - 1; i >= 0; i--)
                {
                    Delegate delegateIter = delegates[i];

                    if (delegateIter.Target is UnityEngine.Object unityObj && unityObj == null)
                    {
                        delegates.RemoveAt(i);
                        continue;
                    }

                    delegateIter?.DynamicInvoke(newEvent);
                }

                compositePool.ReturnItemFromPool(newEvent);
            }
        }
    }

    public MethodInfo GetGenericRaiseMethod(Type type)
    {
        if (genericMethod.TryGetValue(type, out MethodInfo genericRaise))
        {
            return genericRaise;
        }
        else
        {
            MethodInfo newGenericRaise = baseRaiseMethod.MakeGenericMethod(type);
            genericMethod.Add(type, newGenericRaise);
            return newGenericRaise;
        }
    }

    public void Clear()
    {
        statesToInstance.Clear();
    }
}
