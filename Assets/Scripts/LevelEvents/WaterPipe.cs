using UnityEngine;

public class WaterPipe : MonoBehaviour
{
    [Header("Referecenes")]
    [SerializeField] private AreaTrigger areaTrigger;


    [Header("Configs")]
    [SerializeField] private float torrentForce;

    private const ForceMode torrentForceMode = ForceMode.Impulse;

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleTriggerEnter;
    }

    private void HandleTriggerEnter(Collider collider)
    {
        Origami origami = collider.GetComponentInParent<Origami>();

        if (origami != null)
        {
            collider.attachedRigidbody.linearVelocity = Vector3.zero;
            collider.attachedRigidbody.AddForce(new Vector3(0f, -torrentForce, 0f), torrentForceMode);
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleTriggerEnter;
    }
}
