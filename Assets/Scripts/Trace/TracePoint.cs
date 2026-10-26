using UnityEngine;

public class TracePoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AreaTrigger areaTrigger;

    public SpriteRenderer GetSpriteRenderer { get { return spriteRenderer; } }

    private void OnEnable()
    {
        spriteRenderer.enabled = true;
    }

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleAreaTrigger;
    }

    private void HandleAreaTrigger(Collider collider)
    {
        if (collider.CompareTag(Crane.CraneTag))
        {
            spriteRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleAreaTrigger;
    }
}
