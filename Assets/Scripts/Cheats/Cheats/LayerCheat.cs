using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LayerCheat : Cheat
{
    [SerializeField] public Teletransportable tp;

    [SerializeField] public InputActionReference nextLayer;
    [SerializeField] public InputActionReference previousLayer;

    private LevelManager levelManager;
    private EventBus eventBus;

    private GameObject currentPortal = null;
    private GameObject currentTarget = null;

    public override void Initialize(CheatManager cheatManager)
    {
        base.Initialize(cheatManager);

        levelManager = ServiceLocator.Instance.GetService<GameManager>().levelManager;

        if(levelManager == null)
        {
            Debug.LogError("Level manager is null!");
            return;
        }

        eventBus = ServiceLocator.Instance.GetService<EventBus>();

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
        if(currentPortal != null)
        {
            Destroy(currentPortal,1.0f);
            currentPortal = null;
        }

        if (currentTarget != null)
        {
            Destroy(currentTarget,1.0f);
            currentTarget = null;
        }

        if (currentPortal != null || currentTarget != null) return;

        if (nextLayer.action.WasPerformedThisFrame())
        {
            if (tp.LayerIndex > levelManager.Layers.Count) return;

            Debug.Log("Layer cheat logic applied next layer");

            FakeTeleport(1);
        }

        if (previousLayer.action.WasPerformedThisFrame())
        {
            if (tp.LayerIndex <= 0) return;

            Debug.Log("Layer cheat logic applied previous layer");

            FakeTeleport(-1);
        }
    }

    private void FakeTeleport(int direction)
    {
        Debug.Log(tp.LayerIndex);
        currentPortal = new GameObject();
        currentPortal.transform.SetParent(levelManager.Layers[tp.LayerIndex].transform);
        currentPortal.name = "CheatPortal";

        LayerPortal portal = currentPortal.AddComponent<LayerPortal>();

        portal.transform.position = tp.transform.position;

        currentTarget = new GameObject();
        currentTarget.transform.SetParent(levelManager.Layers[tp.LayerIndex + direction].transform);
        currentTarget.name = "CheatTarget";

        PortalTarget target = currentTarget.AddComponent<PortalTarget>();

        target.transform.position = new Vector3(tp.transform.position.x, tp.transform.position.y, levelManager.Layers[tp.LayerIndex + direction].transform.position.z);
        target.SetLayerIndex(tp.LayerIndex + direction);

        portal.target = target;

        tp.EnterPortal(portal);

        eventBus.Raise<OnEnterLayerPortal>(tp, target);
    }
}