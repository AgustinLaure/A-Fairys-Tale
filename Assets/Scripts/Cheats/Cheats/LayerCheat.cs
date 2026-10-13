using UnityEngine;
using UnityEngine.InputSystem;

public class LayerCheat : Cheat
{
    [SerializeField] public Teletransportable tp;

    [SerializeField] public InputActionReference nextLayer;
    [SerializeField] public InputActionReference previousLayer;

    public override void Initialize(CheatManager cheatManager)
    {
        base.Initialize(cheatManager);

        if (tp == null)
        {
            Debug.LogError("Teletransportable is null!");
            return;
        }

        if (nextLayer == null)
        {
            Debug.LogError("next layer action is null!");
            return;
        }

        if (previousLayer == null)
        {
            Debug.LogError("previous layer action is null!");
            return;
        }

        Debug.Log("Started Layer cheat!");
    }

    public override void LogicUpdate()
    {
        if (nextLayer.action.WasPerformedThisFrame())
        {
            Debug.Log("Layer cheat logic applied next layer");
        }

        if (previousLayer.action.WasPerformedThisFrame())
        {
            Debug.Log("Layer cheat logic applied previous layer");
        }
    }
}