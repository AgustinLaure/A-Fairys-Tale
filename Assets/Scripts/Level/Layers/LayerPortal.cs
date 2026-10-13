using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LayerPortal : MonoBehaviour
{
    [SerializeField] public PortalTarget target;

    [SerializeField] public bool flipSpeedOnTeleport = true;
    [SerializeField] public float multiplySpeedOnTeleport = 1.0f;

    private BoxCollider portalCollider;

    private EventBus eventBus;

    private int layerIndex = -1;

    public int LayerIndex { get { return layerIndex; } }
    public void SetLayerIndex(int newIndex) { layerIndex = newIndex; }

    void Awake()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    void Start()
    {
        portalCollider = GetComponent<BoxCollider>();

        target.AddPortal(this);
    }

    void OnTriggerEnter(Collider other)
    {
        Teletransportable tp = other.gameObject.GetComponent<Teletransportable>();
        if (!tp)
        {
            //Debug.LogWarning("Non Teleportable object entered portal");
            return;
        }

        tp.EnterPortal(this);

        eventBus.Raise<OnEnterLayerPortal>(tp,target);
    }

    void OnTriggerExit(Collider other)
    {
        Teletransportable tp = other.gameObject.GetComponent<Teletransportable>();
        if (!tp) return;

        tp.ExitPortal(this);
    }
}
