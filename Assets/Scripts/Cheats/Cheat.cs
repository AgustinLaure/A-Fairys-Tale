using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public abstract class Cheat : MonoBehaviour
{
    [SerializeField] public string cheatName;
    [SerializeField] public bool active = true;

    private CheatManager manager;

    public virtual void Initialize(CheatManager cheatManager)
    {
        manager = cheatManager;

        if (string.IsNullOrEmpty(cheatName))
        {
            Debug.LogWarning("You should name this cheat!");
            cheatName = this.name;
        }

        if (manager == null)
        {
            Debug.LogError("Cheat couldnt find manager");
            return;
        }
    }

    public virtual void LogicUpdate() { }
    protected virtual void Update() { }
}
