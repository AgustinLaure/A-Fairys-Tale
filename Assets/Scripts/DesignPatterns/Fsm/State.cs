using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public List<MonoBehaviour> actions;

    public void SetActionState(bool state)
    {
        foreach (MonoBehaviour action in actions)
        {
            action.enabled = state;
        }
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
