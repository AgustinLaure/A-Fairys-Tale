using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public abstract class Cheat
{
    [SerializeField] public string name;
    [SerializeField] public InputActionReference action;

    private CheatManager manager;

    protected bool active = false;

    protected Cheat()
    {
        if (name == null)
        {
            Debug.LogWarning("You should name this cheat!");
            name = this.ToString();
        }

        manager = ServiceLocator.Instance.GetService<CheatManager>();

        if (manager == null)
        {
            Debug.LogError("Cheat couldnt find manager");
            return;
        }

        manager.AddCheat(this);

        Start();
    }

    protected virtual void Start() { }
    public virtual void CheatLogic() 
    {
        active = !active;
    }
}
